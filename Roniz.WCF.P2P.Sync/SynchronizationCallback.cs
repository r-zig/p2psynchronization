using System;
using System.Diagnostics.Contracts;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Timers;
using Roniz.Diagnostics.Logging;
using Roniz.WCF.P2P.Channels;
using Roniz.WCF.P2P.Messages.Announcment;
using Roniz.WCF.P2P.Messages.Presence;
using Roniz.WCF.P2P.Sync.Enums;
using Roniz.WCF.P2P.Sync.Filters;
using Roniz.WCF.P2P.Sync.Helpers;
using Roniz.WCF.P2P.Sync.Interfaces;
using Roniz.WCF.P2P.Sync.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.Sync
{
    class SynchronizationCallback : ISynchronizedState, IDisposable
    {
        #region members

        #region synchronization mode members

        /// <summary>
        /// the synchronization mode that will used
        /// </summary>
        private readonly SynchronizationMode synchronizationMode;

        private readonly Timer reSyncTimer;

        /// <summary>
        /// determine if SynchronizationDetailedRequest for this session already in progress or sent
        /// </summary>
        private bool alreadySynchronizationDetailedRequest;

        /// <summary>
        /// determine if SynchronizationDetailedResponse for this session already received
        /// </summary>
        private bool alreadySynchronizationDetailsResponse;
        #endregion

        private ChannelWrapper channel;

        private ISynchronizedState proxy;

        /// <summary>
        /// should hold specific derived application business logic implementation , this is the extensibility point for consumer
        /// </summary>
        private readonly ISynchronizationBusinessLogic businessLogic;

        private bool disposed;

        #endregion

        #region constructores
        /// <summary>
        /// Initialize the callback instance
        /// </summary>
        /// <param name="synchronizationBusinessLogic">The business logic instance provide by the consumer</param>
        /// <param name="synchronizationMode">the synchronization mode that will used , if not provide the default is SynchronizationMode.Reliable</param>
        /// <param name="reSyncInterval">The interval in milliseconds that the peer try to re-sync itself - work only when synchronizationMode is Reliable</param>
        public SynchronizationCallback(ISynchronizationBusinessLogic synchronizationBusinessLogic,
            SynchronizationMode synchronizationMode = Defaults.DefaultSynchronizationMode,
            double reSyncInterval = Defaults.ReSyncDefaultInterval)
        {
            Contract.Requires<ArgumentNullException>(synchronizationBusinessLogic != null);
            Contract.Requires<ArgumentOutOfRangeException>(synchronizationMode != SynchronizationMode.Reliable || reSyncInterval > 0, Properties.Resources.SynchronizationModeReliableReSyncIntervalBiggerThenZero);
            businessLogic = synchronizationBusinessLogic;
            this.synchronizationMode = synchronizationMode;
            if (synchronizationMode == SynchronizationMode.Reliable)
            {
                reSyncTimer = new Timer(reSyncInterval)
                                  {
                                      AutoReset = false
                                  };
                reSyncTimer.Elapsed += OnSyncTimerElapsed;
            }
        }
        #endregion

        #region properties

        internal ISynchronizedState Proxy
        {
            get { return proxy; }
            set
            {
                if (ReferenceEquals(proxy, value))
                    return;

                proxy = value;
                OnProxyChanged(proxy);
            }
        }

        #endregion

        #region methods

        private void OnProxyChanged(ISynchronizedState changedProxy)
        {
            RegisterProxy(new ChannelWrapper((IChannel)changedProxy));
        }

        private void RegisterProxy(ChannelWrapper channelWrapper)
        {
            if (channel != null)
                channel.Dispose();

            channel = channelWrapper;
#if DEBUG
            channel.Closing += (o, e) => LogManager.GetCurrentClassLogger().Debug("channel closing");
            channel.Closed += (o, e) => LogManager.GetCurrentClassLogger().Debug("channel closed");
            channel.Faulted += (o, e) => LogManager.GetCurrentClassLogger().Debug("channel faulted");
            channel.Opened += (o, e) => LogManager.GetCurrentClassLogger().Debug("channel opened");
            channel.Opening += (o, e) => LogManager.GetCurrentClassLogger().Debug("channel opening");
#endif
            channel.PeerNode.MessagePropagationFilter = new RemoteOnlyMessagePropagationFilter();
            channel.Online += PeerNodeOnline;
            channel.Online += PeerNodeOffline;
        }

        /// <summary>
        /// When the peer become online
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeerNodeOnline(object sender, EventArgs e)
        {
            //when online - send own presence
            SendOnlineAnnouncementMessage();

            //when online - start sync flow...
            BeginSynchronization();
        }

        /// <summary>
        /// When the peer become offline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeerNodeOffline(object sender, EventArgs e)
        {
            if (reSyncTimer != null)
                reSyncTimer.Stop();
        }

        private void OnSyncTimerElapsed(object sender, ElapsedEventArgs e)
        {
            BeginSynchronization();
        }
        /// <summary>
        /// Send online announcement message to the mesh
        /// </summary>
        private void SendOnlineAnnouncementMessage()
        {
            Task<FullPresenceInfo>.Factory.StartNew(() =>
                businessLogic.ProvideFullPresenceInfo())
                .ContinueWith(task =>
                {
                    //exception handling
                    if (task.IsFaulted)
                    {
                        task.Exception.Handle(exception =>
                        {
                            LogManager.Fatal(exception);
                            return true;
                        });
                        return;
                    }

                    if (task.Result == null)
                    {
                        LogManager.GetCurrentClassLogger().Debug("businessLogic.ProvideFullPresenceInfo() return null - OnlineAnnouncement will not sent to other peers - - its legal , but for debugging information for the implementer should consider if this is the correct behavior");
                        return;
                    }

                    var onlineAnnouncementMessage = new OnlineAnnouncementMessage
                    {
                        PresenceInfo = task.Result
                    };

                    //send the online message to the mesh
                    Proxy.OnlineAnnouncement(onlineAnnouncementMessage);
                });
        }

        /// <summary>
        /// Begin Synchronization process
        /// </summary>
        private void BeginSynchronization()
        {
            if (reSyncTimer != null)
                reSyncTimer.Stop();

            Task.Factory.StartNew(() =>
            {
                var synchronizationRequest = new SynchronizationRequest
                {
                    FullDetailedResponse = businessLogic.IsNeedFullSynchronization,
                };
                Proxy.SynchronizationRequest(synchronizationRequest);
            }).HandleException();

            alreadySynchronizationDetailedRequest = false;
            alreadySynchronizationDetailsResponse = false;
        }

        /// <summary>
        /// Occur when the synchronization flow complete
        /// </summary>
        private void OnSynchronizationComplete()
        {
            //restart the timer to try re synchronize again later
            if (synchronizationMode == SynchronizationMode.Reliable && !reSyncTimer.Enabled)
            {
                reSyncTimer.Start();
            }
        }

        #region Implementation of IAnnouncement

        void ISynchronizedState.OfflineAnnouncement(OfflineAnnouncementMessage announcementMessage)
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("OfflineAnnouncement: {0}", announcementMessage);
#endif
            try
            {
                businessLogic.OnOfflineAnnouncementReceived(announcementMessage.PresenceInfo);
            }
            catch (Exception exception)
            {
                HelperMethods.HandleBusinessLogicException(exception);
            }
        }

        void ISynchronizedState.OnlineAnnouncement(OnlineAnnouncementMessage announcementMessage)
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("OnlineAnnouncement {0}",announcementMessage);
#endif
            try
            {
                businessLogic.OnOnlineAnnouncementReceived(announcementMessage.PresenceInfo);
            }
            catch (Exception exception)
            {
                HelperMethods.HandleBusinessLogicException(exception);
            }
        }

        void ISynchronizedState.PresenceInfoChanged(PresenceInfoChangedMessage presenceInfoChangedMessage)
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("presenceInfoChangedMessage: {0}",presenceInfoChangedMessage);
#endif
            try
            {
                businessLogic.OnPresenceInfoChangedReceived(presenceInfoChangedMessage.PresenceInfo);
            }
            catch (Exception exception)
            {
                HelperMethods.HandleBusinessLogicException(exception);
            }
        }
        #endregion

        #region ISynchronizedState Members

        /// <summary>
        /// The first request usually from the peer that joined to the mesh
        /// that ask for synchronization
        /// </summary>
        /// <param name="request">the request from the peer to his neighbors</param>
        void ISynchronizedState.SynchronizationRequest(SynchronizationRequest request)
        {
            if (request.IsOwnMessage)
                return;

            if (request.FullDetailedResponse)
            {
                Task<BusinessLogicMessageBase>.Factory.StartNew(() =>
                    businessLogic.ProvideFullSynchronizationDetailResponse()).ContinueWith(task =>
                    {
                        //exception handling
                        if (task.IsFaulted)
                        {
                            task.Exception.Handle(exception =>
                                                      {
                                                          LogManager.Fatal(exception);
                                                          return true;
                                                      });
                            return;
                        }

                        if (task.Result == null)
                        {
                            LogManager.GetCurrentClassLogger().Debug("businessLogic.ProvideFullSynchronizationDetailResponse() return null - SynchronizationDetailsResponse will not sent to neighbors peers - its legal , but for debugging information for the implementer should consider if this is the correct behavior");
                            return;
                        }

                        var response = new SynchronizationDetailsResponseContainer
                                           {
                                               Response = task.Result
                                           };
                        //send the response
                        Proxy.SynchronizationDetailsResponse(response);
                    });
            }
            else
            {
                Task<BusinessLogicMessageBase>.Factory.StartNew(() =>
                    businessLogic.ProvideSynchronizationResponse(request)).ContinueWith(task =>
                    {
                        //exception handling
                        if (task.IsFaulted)
                        {
                            task.Exception.Handle(exception =>
                            {
                                LogManager.Fatal(exception);
                                return true;
                            });
                            return;
                        }

                        if (task.Result == null)
                        {
                            LogManager.GetCurrentClassLogger().Debug("businessLogic.ProvideSynchronizationResponse() return null - SynchronizationKeysResponse will not sent to neighbors peers - its legal , but for debugging information for the implementer should consider if this is the correct behavior");
                            return;
                        }

                        //send the response
                        var response = new SynchronizationResponseContainer
                                           {
                                               Response = task.Result
                                           };
                        Proxy.SynchronizationKeysResponse(response);
                    });
            }
        }

        /// <summary>
        /// The response that usually from the peers that received prior the SynchronizationRequest
        /// </summary>
        /// <param name="response">the response from the peers to the requester peer</param>
        void ISynchronizedState.SynchronizationKeysResponse(SynchronizationResponseContainer response)
        {
            if (response.IsOwnMessage)
                return;

            //first message win , other messages ignore pattern
            if (synchronizationMode == SynchronizationMode.Economical && alreadySynchronizationDetailedRequest)
            {
                LogManager.GetCurrentClassLogger().Debug("synchronization stopped -> synchronizationMode == SynchronizationMode.Economical && alreadySynchronizationDetailedRequest - its legal , but for debugging information for the implementer should consider if this is the correct behavior");
                return;
            }

            alreadySynchronizationDetailedRequest = true;

            Task<BusinessLogicMessageBase>.Factory.StartNew(() =>
                businessLogic.ProvideSynchronizationDetailRequest(response.Response)).ContinueWith(task =>
                {
                    //exception handling
                    if (task.IsFaulted)
                    {
                        task.Exception.Handle(exception =>
                        {
                            LogManager.Fatal(exception);
                            return true;
                        });
                        return;
                    }

                    if (task.Result == null)
                    {
                        LogManager.GetCurrentClassLogger().Debug("businessLogic.ProvideSynchronizationDetailRequest() return null - SynchronizationDetailsRequest will not sent to neighbors peers - its legal , but for debugging information for the implementer should consider if this is the correct behavior");
                        return;
                    }

                    var detailRequest = new SynchronizationDetailsRequestContainer { Request = task.Result };

                    //send the request back to the mesh
                    Proxy.SynchronizationDetailsRequest(detailRequest);
                });
        }

        /// <summary>
        /// The second request that usually from the peer that received prior the SynchronizationResponse
        /// now he ask for response details from one of the peers (usually the first one to response prior)  more detailed message
        /// </summary>
        /// <param name="request">the request from the peer</param>
        void ISynchronizedState.SynchronizationDetailsRequest(SynchronizationDetailsRequestContainer request)
        {
            if (request.IsOwnMessage)
                return;

            Task<BusinessLogicMessageBase>.Factory.StartNew(() =>
                businessLogic.ProvideSynchronizationDetailResponse(request.Request))
                .ContinueWith(task =>
                {
                    //exception handling
                    if (task.IsFaulted)
                    {
                        task.Exception.Handle(exception =>
                        {
                            LogManager.Fatal(exception);
                            return true;
                        });
                        return;
                    }

                    if (task.Result == null)
                    {
                        LogManager.GetCurrentClassLogger().Debug("businessLogic.ProvideSynchronizationDetailResponse() return null - SynchronizationDetailsResponse will not sent to neighbors peers - its legal , but for debugging information for the implementer should consider if this is the correct behavior");
                        return;
                    }

                    var response = new SynchronizationDetailsResponseContainer { Response = task.Result };

                    //send the response back to the mesh
                    Proxy.SynchronizationDetailsResponse(response);
                });
        }

        /// <summary>
        /// The response the peer that received prior the SynchronizationDetailsRequest
        /// </summary>
        /// <param name="response">the response from the peer</param>
        void ISynchronizedState.SynchronizationDetailsResponse(SynchronizationDetailsResponseContainer response)
        {
            if (synchronizationMode == SynchronizationMode.Economical && alreadySynchronizationDetailsResponse)
            {
                LogManager.GetCurrentClassLogger().Debug("synchronization stopped -> synchronizationMode == SynchronizationMode.Economical && alreadySynchronizationDetailedRequest - its legal , but for debugging information for the implementer should consider if this is the correct behavior");
                return;
            }
            alreadySynchronizationDetailsResponse = true;

            try
            {
                businessLogic.IsNeedFullSynchronization = false;
                businessLogic.OnSynchronizationDetailsResponseReceived(response.Response);
            }
            catch (Exception exception)
            {
                HelperMethods.HandleBusinessLogicException(exception);
            }
            finally
            {
                OnSynchronizationComplete();
            }
        }

        /// <summary>
        /// Update the mesh with new data
        /// </summary>
        /// <param name="synchronizationUpdateMessage">the update message that sent from the peer , this message is base class that usually inherit from SynchronizationUpdateMessage<T/> that contain state property</param>
        void ISynchronizedState.SynchronizationUpdate(SynchronizationUpdateMessage synchronizationUpdateMessage)
        {
            if (synchronizationUpdateMessage.IsOwnMessage)
                return;
            try
            {
                businessLogic.OnUpdateReceived(synchronizationUpdateMessage.State);
            }
            catch (Exception exception)
            {
                HelperMethods.HandleBusinessLogicException(exception);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (disposed)
                return;

            if (reSyncTimer != null)
            {
                reSyncTimer.Dispose();
            }

            GC.SuppressFinalize(this);
            disposed = true;
        }

        #endregion
        #endregion
    }
}
