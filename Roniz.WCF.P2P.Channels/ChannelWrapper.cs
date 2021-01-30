using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Roniz.Diagnostics.Logging;

namespace Roniz.WCF.P2P.Channels
{
    /// <summary>
    /// Wrap p2p channel and provide fixes event
    /// </summary>
    public sealed class ChannelWrapper : IChannel, IDisposable
    {
        #region events
        public event EventHandler Online;
        public event EventHandler Offline;

        #region Implementation of ICommunicationObject
        /// <summary>
        /// Occurs when the communication object completes its transition from the closing state into the closed state.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Occurs when the communication object first enters the closing state.
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Occurs when the communication object first enters the faulted state.
        /// </summary>
        public event EventHandler Faulted;

        /// <summary>
        /// Occurs when the communication object completes its transition from the opening state into the opened state.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// Occurs when the communication object first enters the opening state.
        /// </summary>
        public event EventHandler Opening;
        #endregion

        #endregion

        #region members
        /// <summary>
        /// sync between the online event & opened state
        /// </summary>
        private readonly ManualResetEventSlim openedStateResetEvent = new ManualResetEventSlim(false, 10);

        /// <summary>
        /// The original channel
        /// </summary>
        private readonly IChannel channel;

        private PeerNode peerNode;
        private bool disposed;

        #endregion

        #region constructores

        public ChannelWrapper(IChannel channel)
        {
            this.channel = channel;
            InitiateChannel(channel);
        }

        #endregion

        #region properties

        #region Implementation of ICommunicationObject
        /// <summary>
        /// Gets the current state of the communication-oriented object.
        /// </summary>
        /// <returns>
        /// The value of the <see cref="T:System.ServiceModel.CommunicationState"/> of the object.
        /// </returns>
        public CommunicationState State
        {
            get { return channel.State; }
        }

        #endregion

        public PeerNode PeerNode
        {
            get
            {
                return peerNode;
            }
            private set
            {
                if (ReferenceEquals(value, peerNode))
                    return;
                peerNode = value;

                if (peerNode == null)
                    return;

                peerNode.Online += OnPeerNodeOnOnline;
                peerNode.Offline += (s, e) =>
                                        {
                                            var handler = Offline;
                                            if (handler == null)
                                                return;
                                            handler(s, e);
                                        };
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Fire the Online event only when the channel is in Opened state
        /// resolve:
        /// 1. bug when opening peer channel asynchronously using Task , and the Online event fired before the channel is in Opened state causing problem to using the channel in this state.
        /// 2. bug when opening peer channel asynchronously using BeginOpen and the first participant that opened does receive Online event
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void OnPeerNodeOnOnline(object s, EventArgs e)
        {
            var handler = Online;
            if (handler == null)
                return;

            //using ManualResetEventSlim for waiting
            openedStateResetEvent.Wait();

            handler(s, e);
        }

        private void InitiateChannel(IChannel initiatedChannel)
        {
            initiatedChannel.Closed += (s, e) =>
                                        {
                                            var handler = Closed;
                                            if (handler == null)
                                                return;
                                            handler(s, e);
                                        };

            initiatedChannel.Closing += (s, e) =>
                                        {
                                            var handler = Closing;
                                            if (handler == null)
                                                return;
                                            handler(s, e);
                                        };

            initiatedChannel.Faulted += (s, e) =>
                                        {
                                            var handler = Faulted;
                                            if (handler == null)
                                                return;
                                            handler(s, e);
                                        };

            initiatedChannel.Opened += (s, e) =>
                                        {
                                            openedStateResetEvent.Set();
                                            var handler = Opened;
                                            if (handler == null)
                                                return;
                                            handler(s, e);
                                        };

            initiatedChannel.Opening += (s, e) =>
                                        {
                                            openedStateResetEvent.Reset();
                                            var handler = Opening;
                                            if (handler == null)
                                                return;
                                            handler(s, e);
                                        };
            PeerNode = initiatedChannel.GetProperty<PeerNode>();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            if (openedStateResetEvent != null)
                openedStateResetEvent.Dispose();

            disposed = true;
            GC.SuppressFinalize(this);
        }


        #region Implementation of ICommunicationObject

        /// <summary>
        /// Causes a communication object to transition immediately from its current state into the closed state.  
        /// </summary>
        public void Abort()
        {
            channel.Abort();
        }

        /// <summary>
        /// Causes a communication object to transition from its current state into the closed state.  
        /// </summary>
        /// <exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="M:System.ServiceModel.ICommunicationObject.Close"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The default close timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public void Close()
        {
            channel.Close();
        }

        /// <summary>
        /// Causes a communication object to transition from its current state into the closed state.  
        /// </summary>
        /// <param name="timeout">The <see cref="TimeSpan"/> that specifies how long the send operation has to complete before timing out.</param><exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="M:System.ServiceModel.ICommunicationObject.Close"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public void Close(TimeSpan timeout)
        {
            channel.Close(timeout);
        }

        /// <summary>
        /// Begins an asynchronous operation to close a communication object.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that references the asynchronous close operation. 
        /// </returns>
        /// <param name="callback">The <see cref="T:System.AsyncCallback"/> delegate that receives notification of the completion of the asynchronous close operation.</param><param name="state">An object, specified by the application, that contains state information associated with the asynchronous close operation.</param><exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="ICommunicationObject.BeginClose(System.TimeSpan,System.AsyncCallback,object)"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The default timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return channel.BeginClose(callback, state);
        }

        /// <summary>
        /// Begins an asynchronous operation to close a communication object with a specified timeout.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that references the asynchronous close operation.
        /// </returns>
        /// <param name="timeout">The <see cref="T:System.TimeSpan"/> that specifies how long the send operation has to complete before timing out.</param><param name="callback">The <see cref="T:System.AsyncCallback"/> delegate that receives notification of the completion of the asynchronous close operation.</param><param name="state">An object, specified by the application, that contains state information associated with the asynchronous close operation.</param><exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="ICommunicationObject.BeginClose(System.TimeSpan,System.AsyncCallback,object)"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The specified timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channel.BeginClose(timeout, callback, state);
        }

        /// <summary>
        /// Completes an asynchronous operation to close a communication object.
        /// </summary>
        /// <param name="result">The <see cref="T:System.IAsyncResult"/> that is returned by a call to the <see cref="ICommunicationObject.BeginClose(System.TimeSpan,System.AsyncCallback,object)"/> method.</param><exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="ICommunicationObject.BeginClose(System.TimeSpan,System.AsyncCallback,object)"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public void EndClose(IAsyncResult result)
        {
            channel.EndClose(result);
        }

        /// <summary>
        /// Causes a communication object to transition from the created state into the opened state.  
        /// </summary>
        /// <exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The default open timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public void Open()
        {
            channel.Open();
        }

        /// <summary>
        /// Causes a communication object to transition from the created state into the opened state within a specified interval of time.
        /// </summary>
        /// <param name="timeout">The <see cref="T:System.TimeSpan"/> that specifies how long the send operation has to complete before timing out.</param><exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The specified timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public void Open(TimeSpan timeout)
        {
            channel.Open(timeout);
        }

        /// <summary>
        /// Begins an asynchronous operation to open a communication object.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that references the asynchronous open operation. 
        /// </returns>
        /// <param name="callback">The <see cref="T:System.AsyncCallback"/> delegate that receives notification of the completion of the asynchronous open operation.</param><param name="state">An object, specified by the application, that contains state information associated with the asynchronous open operation.</param><exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The default open timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            //return channel.BeginOpen(callback, state);
            return Task.Factory.StartNew(s =>channel.Open(), state).HandleException().
                ContinueWith(t => callback(t)).HandleException();
        }

        /// <summary>
        /// Begins an asynchronous operation to open a communication object within a specified interval of time.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that references the asynchronous open operation. 
        /// </returns>
        /// <param name="timeout">The <see cref="T:System.TimeSpan"/> that specifies how long the send operation has to complete before timing out.</param><param name="callback">The <see cref="T:System.AsyncCallback"/> delegate that receives notification of the completion of the asynchronous open operation.</param><param name="state">An object, specified by the application, that contains state information associated with the asynchronous open operation.</param><exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The specified timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            //return channel.BeginOpen(timeout, callback, state);
            return Task.Factory.StartNew(s =>channel.Open(timeout), state).HandleException().
                ContinueWith(t => callback(t)).HandleException();
        }

        /// <summary>
        /// Completes an asynchronous operation to open a communication object.
        /// </summary>
        /// <param name="result">The <see cref="T:System.IAsyncResult"/> that is returned by a call to the <see cref="ICommunicationObject.BeginOpen(System.TimeSpan,System.AsyncCallback,object)"/> method.</param><exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public void EndOpen(IAsyncResult result)
        {
            //comment here because the inner channel is opened synchronously due to the bug of online event that does not raised correctly in asynchronously behavior
            //channel.EndOpen(result);
        }

        /// <summary>
        /// Returns a typed object requested, if present, from the appropriate layer in the channel stack.
        /// </summary>
        /// <returns>
        /// The typed object requested if it is present or null if it is not.
        /// </returns>
        /// <typeparam name="T">The typed object for which the method is querying.</typeparam>
        public T GetProperty<T>() where T : class
        {
            return channel.GetProperty<T>();
        }

        #endregion
        #endregion
    }
}
