using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.ServiceModel;
using Roniz.Diagnostics.Logging;
using Roniz.WCF.P2P.Channels;
using Roniz.WCF.P2P.Messages;
using Roniz.WCF.P2P.Messages.Announcment;
using Roniz.WCF.P2P.Sync.Enums;
using Roniz.WCF.P2P.Sync.Helpers;
using Roniz.WCF.P2P.Sync.Interfaces;
using Roniz.WCF.P2P.Sync.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;
using ServiceModelEx;

namespace Roniz.WCF.P2P.Sync
{
    /// <summary>
    /// Provide class that guaranty that all peers will share synchronized state
    /// </summary>
    public sealed class SynchronizationStateManager : ISynchronizationStateManager, INotifyPropertyChanged , IDisposable
    {
        #region members
        private bool useGenericResolver = Defaults.UseGenericResolver;
        private readonly string endpointConfigurationName;
        private DuplexChannelFactory<ISynchronizedState> channelFactory;
        private ChannelWrapper channel;
        private ISynchronizedState synchronizedStateProxy;

        /// <summary>
        /// The callback instance that that receive p2p callback messages
        /// Used because don't want that the manager will expose those method to the consumer
        /// </summary>
        private SynchronizationCallback callbackInstance;

        /// <summary>
        /// The current peer node instance
        /// </summary>
        private PeerNode peerNode;

        private bool disposed;
        
        #endregion

        #region events
        /// <summary>
        /// when peer become online
        /// </summary>
        public event EventHandler PeerOnline;

        /// <summary>
        /// when peer become offline
        /// </summary>
        public event EventHandler PeerOffline;

        public event EventHandler CommunicationStateChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// should hold specific derived application business logic implementation , this is the extensibility point for consumer
        /// </summary>
        private readonly ISynchronizationBusinessLogic businessLogic;

        private SynchronizationCommunicationState oldSynchronizationCommunicationState;
        private readonly SynchronizationMode synchronizationMode;
        private readonly double reSyncInterval;

        #endregion

        #region constructores

        /// <summary>
        /// Initialize SynchronizationStateManager instance will all default values except the business logic
        /// </summary>
        /// <param name="synchronizationBusinessLogic">The business logic instance provide by the consumer</param>
        public SynchronizationStateManager(ISynchronizationBusinessLogic synchronizationBusinessLogic) :
            this(synchronizationBusinessLogic, 
            Defaults.SyncDefaultEndpointConfigurationName,
            Defaults.DefaultSynchronizationMode, Defaults.ReSyncDefaultInterval)
        {
            Contract.Requires<ArgumentNullException>(synchronizationBusinessLogic != null);
        }

        /// <summary>
        /// Initialize SynchronizationStateManager instance will all default values except the business logic and the endpointConfigurationName
        /// </summary>
        /// <param name="synchronizationBusinessLogic">The business logic instance provide by the consumer</param>
        /// <param name="endpointConfigurationName">the endpointConfigurationName that will used to obtain the wcf configuration and create the channel, if not provide the default is SyncDefaultEndpointConfigurationName</param>
        public SynchronizationStateManager(ISynchronizationBusinessLogic synchronizationBusinessLogic,
            string endpointConfigurationName) :
            this(synchronizationBusinessLogic,
            endpointConfigurationName,
            Defaults.DefaultSynchronizationMode,
            Defaults.ReSyncDefaultInterval)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(endpointConfigurationName));
            Contract.Requires<ArgumentNullException>(synchronizationBusinessLogic != null);
        }

        /// <summary>
        /// Initialize SynchronizationStateManager instance will all default values except the business logic and the endpointConfigurationName
        /// </summary>
        /// <param name="synchronizationBusinessLogic">The business logic instance provide by the consumer</param>
        /// <param name="endpointConfigurationName">the endpointConfigurationName that will used to obtain the wcf configuration and create the channel, if not provide the default is SyncDefaultEndpointConfigurationName</param>
        /// <param name="synchronizationMode">the synchronization mode that will used , if not provide the default is SynchronizationMode.Reliable</param>
        /// <param name="reSyncInterval">The interval in milliseconds that the peer try to re-sync itself - work only when synchronizationMode is Reliable</param>
        public SynchronizationStateManager(ISynchronizationBusinessLogic synchronizationBusinessLogic,
            string endpointConfigurationName,
            SynchronizationMode synchronizationMode,
            double reSyncInterval = Defaults.ReSyncDefaultInterval
            )
        {
            Contract.Requires<ArgumentNullException>(synchronizationBusinessLogic != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(endpointConfigurationName));
            Contract.Requires<ArgumentOutOfRangeException>(synchronizationMode != SynchronizationMode.Reliable || reSyncInterval > 0, Properties.Resources.SynchronizationModeReliableReSyncIntervalBiggerThenZero);
            this.endpointConfigurationName = endpointConfigurationName;
            this.businessLogic = synchronizationBusinessLogic;
            this.synchronizationMode = synchronizationMode;
            this.reSyncInterval = reSyncInterval;
        }

        #endregion

        #region properties
        /// <summary>
        /// If to use generic resolver that use reflection (slower) but guaranty that all known types will add , and eliminate the developer from manual adding known types
        /// </summary>
        public bool UseGenericResolver
        {
            get { return useGenericResolver; }
            set { useGenericResolver = value; }
        }

        public SynchronizationCommunicationState State
        {
            get
            {
                return SynchronizationCommunicationStateHelper.GetState(synchronizedStateProxy as ICommunicationObject, peerNode);
            }
        }
        #endregion

        #region methods
        
        private void InvokeCommunicationStateChanged(object sender, EventArgs e)
        {
            try
            {
                businessLogic.OnCommunicationStateChanged(oldSynchronizationCommunicationState, State);
            }
            catch (Exception exception)
            {
                HelperMethods.HandleBusinessLogicException(exception);
            }
            
            oldSynchronizationCommunicationState = State;
            InvokePropertyChanged("State");
            var handler = CommunicationStateChanged;
            if (handler == null)
                return;
            handler(sender, e);
        }

        private void InvokePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InvokePeerOnline(EventArgs e)
        {
            var handler = PeerOnline;
            if (handler != null) handler(this, e);
        }

        private void InvokePeerOffline(EventArgs e)
        {
            var handler = PeerOffline;
            if (handler != null) handler(this, e);
        }

        #region ISynchronizationStateManager members

        /// <summary>
        /// open the p2p channel and register for online and offline events
        /// </summary>
        public void Open()
        {
            //Open p2p channel
            OpenCore(endpointConfigurationName);
        }

        /// <summary>
        /// open the p2p channel and register for online and offline events
        /// </summary>
        /// <param name="endpointConfigName">The configuration name used for the endpoint</param>
        private void OpenCore(string endpointConfigName)
        {
            callbackInstance = new SynchronizationCallback(businessLogic, synchronizationMode, reSyncInterval);
            //Open p2p channel
            channelFactory = new DuplexChannelFactory<ISynchronizedState>(callbackInstance, endpointConfigName);
            channelFactory.Closing += InvokeCommunicationStateChanged;
            channelFactory.Closed += InvokeCommunicationStateChanged;
            channelFactory.Faulted += InvokeCommunicationStateChanged;
            channelFactory.Opened += InvokeCommunicationStateChanged;
            channelFactory.Opening += InvokeCommunicationStateChanged;

            if(UseGenericResolver)
                channelFactory.AddGenericResolver();

            channelFactory.BeginOpen(ar =>
            {
                try
                {
                    channelFactory.EndOpen(ar);
                }
                catch (Exception exception)
                {
                    LogManager.GetCurrentClassLogger().Fatal(exception);
                    return;
                }

                synchronizedStateProxy = channelFactory.CreateChannel();
                channel = new ChannelWrapper((IClientChannel)synchronizedStateProxy);

                callbackInstance.Proxy = synchronizedStateProxy;
                RegisterProxyEvent(channel);

                /*
                 * both receive online event but then the second participant receive Exception:
                 * "Cannot make a call on this channel because a call to Open() is in progress."
                 * when calling SynchronizationCallback.BeginSynchronization() from Online event handler , 
                 * this is because of issue of raising the Online event prior to the Opened state set.
                 **/

                //The Online event not raised on the first participant that is already registered
                channel.BeginOpen(ar2 =>
                {
                    try
                    {
                        channel.EndOpen(ar2);
                        LogManager.GetCurrentClassLogger().Debug("channel opened");
                    }
                    catch (Exception exception)
                    {
                        LogManager.GetCurrentClassLogger().Fatal(exception);
                    }
                }, null);
            }, null);
        }

        private void RegisterProxyEvent(ChannelWrapper registeredChannel)
        {
            registeredChannel.Closing += InvokeCommunicationStateChanged;
            registeredChannel.Closed += InvokeCommunicationStateChanged;
            registeredChannel.Faulted += InvokeCommunicationStateChanged;
            registeredChannel.Opened += InvokeCommunicationStateChanged;
            registeredChannel.Opening += InvokeCommunicationStateChanged;

            peerNode = registeredChannel.GetProperty<PeerNode>();
            if (peerNode == null)
                throw new InvalidOperationException("incorrect behavior - peerNode is null");

            peerNode.Online += (o, e) => InvokePeerOnline(e);
            peerNode.Offline += (o, e) => InvokePeerOffline(e);

            peerNode.Online += InvokeCommunicationStateChanged;
            peerNode.Offline += InvokeCommunicationStateChanged;
        }

        /// <summary>
        /// Close gracefully the communication
        /// </summary>
        public void Close()
        {
            try
            {
                if (State == SynchronizationCommunicationState.Online)
                {
                    //send announce that is leaving the mesh
                    SendOfflineAnnouncementMessage();
                }

                //close the p2p channel
                if(synchronizedStateProxy != null)
                    ((IClientChannel)synchronizedStateProxy).Close();

                DisposeCallbackIfNeed();
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Fatal(exception);
            }
            finally
            {
                if (State != SynchronizationCommunicationState.Closed)
                {
                    try
                    {
                        //close the p2p channel
                        ((IClientChannel) synchronizedStateProxy).Abort();
                    }
                    catch (Exception exception)
                    {
                        LogManager.GetCurrentClassLogger().Fatal(exception);
                    }
                }

                DisposeCallbackIfNeed();
            }
        }

        public void Update<TState>(TState state) where TState : BusinessLogicMessageBase
        {
            Update(state,false);
        }

        public void Update<TState>(TState state, bool neighborOnly = false) where TState : BusinessLogicMessageBase
        {
            Update(state, neighborOnly ? 1 : FloodMessageBase.MaxHopCount);
        }

        public void Update<TState>(TState state, int hops = FloodMessageBase.MaxHopCount) where TState : BusinessLogicMessageBase
        {
            var updateMessage = new SynchronizationUpdateMessage
                                                    {
                                                        HopCount = hops,
                                                        State = state
                                                    };
            try
            {
                synchronizedStateProxy.SynchronizationUpdate(updateMessage);
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Fatal(exception);
            }
        }
        #endregion

        /// <summary>
        /// Send offline announcement message to the mesh
        /// </summary>
        private void SendOfflineAnnouncementMessage()
        {
            var message = businessLogic.ProvideCompactPresenceInfo();
            var offineAnnouncementMessage = new OfflineAnnouncementMessage
            {
                PresenceInfo = message
            };
            synchronizedStateProxy.OfflineAnnouncement(offineAnnouncementMessage);
        }

        /// <summary>
        /// Dispose the callback instance if its not null
        /// and assign it null value
        /// </summary>
        private void DisposeCallbackIfNeed()
        {
            if (callbackInstance != null)
            {
                callbackInstance.Dispose();
                callbackInstance = null;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (disposed)
                return;

            if (channel != null)
            {
                channel.Dispose();
                channel = null;
            }

            if (channelFactory != null)
            {
                channelFactory.Close();
                channelFactory = null;
            }

            DisposeCallbackIfNeed();

            GC.SuppressFinalize(this);
            disposed = true;
        }

        #endregion
        #endregion
    }
}
