using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using DrWPF.Windows.Data;
using Roniz.Diagnostics.Logging;
using Roniz.WCF.P2P.ApplicationTester.Messages;
using Roniz.WCF.P2P.Messages.Presence;
using Roniz.WCF.P2P.Sync.Enums;
using Roniz.WCF.P2P.Sync.Helpers;
using Roniz.WCF.P2P.Sync.Interfaces;
using Roniz.WCF.P2P.Sync.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.ApplicationTester
{
    public class MySynchronizationBusinessLogic : ISynchronizationBusinessLogic, INotifyPropertyChanged
    {
        #region members

        private ObservableDictionary<Guid, MyUserUpdateState> stateDictionary;
        private readonly object syncLock;
        private readonly Guid id;
        private readonly MyUserUpdateState userUpdateState;

        private string fullPresenceInfoData;
        private string compactPresenceInfoData;
        #endregion

        #region properties

        public ObservableDictionary<Guid, MyUserUpdateState> MyStateDictionary
        {
            get
            {
                return stateDictionary;
            }
            private set
            {
                stateDictionary = value;
                InvokePropertyChanged("MyStateDictionary");
            }
        }

        public string FullPresenceInfoData
        {
            get { return fullPresenceInfoData; }
            set
            {
                fullPresenceInfoData = value;
                InvokePropertyChanged("FullPresenceInfoData");
            }
        }

        public string CompactPresenceInfoData
        {
            get { return compactPresenceInfoData; }
            set
            {
                compactPresenceInfoData = value;
                InvokePropertyChanged("CompactPresenceInfoData");
            }
        }

        #region Implementation of ISynchronizationBusinessLogic

        /// <summary>
        /// determine if the bussiness logic should need a full detailed synchronization or not
        /// </summary>
        /// <remarks>
        /// When synchronization process begin the requeting peer ask the mesh or for partial or for full synchronization
        /// and the decision what to ask is based on the value of this property.
        /// for example: in secnario of peer that initiate without any data , maybe it will be better to ask for full synchronization
        /// but in peer that already have some data , maybe it will ask only to synchronize the data it don't have yet
        /// </remarks>
        public bool IsNeedFullSynchronization
        {
            get;
            set;
        }

        #endregion

        #endregion

        #region constructor
        public MySynchronizationBusinessLogic(Guid id, MyUserUpdateState userUpdateState)
        {
            this.id = id;
            this.userUpdateState = userUpdateState;
            syncLock = new object();
            MyStateDictionary = new ObservableDictionary<Guid, MyUserUpdateState>(Dispatcher.CurrentDispatcher);
        }
        #endregion

        #region methods

        #region Implementation of ISynchronizationBusinessLogic

        /// <summary>
        /// called when the State changed
        /// </summary>
        /// <param name="oldState">The last state prior to the change</param>
        /// <param name="newState">The new state after the change</param>
        /// <remarks>
        /// The developer does not must implement this method , this is only extenstion point
        /// </remarks>
        void ISynchronizationBusinessLogic.OnCommunicationStateChanged(SynchronizationCommunicationState oldState, SynchronizationCommunicationState newState)
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("OnCommunicationStateChanged {0} -> {1}", oldState, newState);
#endif
            if (SynchronizationCommunicationStateHelper.IsOpened(newState))
            {
                //become opened or online
                AddOwnDataStateDictionary();
            }
            else if (newState != SynchronizationCommunicationState.Opening)
            {
                //become oflline , close , fault etc
                ClearStateDictionary();
            }
        }

        /// <summary>
        /// Generate SynchronizationResponse message that will be returned back to the mesh based on given synchronizationRequest message
        /// should override to produce application specific response
        /// </summary>
        /// <remarks>this method will invoked On receiver peer side based on prior SynchronizationRequest operation</remarks>
        /// <param name="synchronizationRequest">the request received from the mesh</param>
        /// <returns>SynchronizationResponse instance or null if don't want to response</returns>
        BusinessLogicMessageBase ISynchronizationBusinessLogic.ProvideSynchronizationResponse(SynchronizationRequest synchronizationRequest)
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("ProvideSynchronizationResponse: {0}", synchronizationRequest);
#endif
            lock (syncLock)
            {
                //return all keys
                return new MyKeysStateIdsContainer
                {
                    StateIds = MyStateDictionary.Select(kvp => kvp.Key).ToList()
                };
            }
        }

        /// <summary>
        /// Generate SynchronizationDetailsRequest message that will be send back to the mesh by the peer that want to synchronized itself based on given SynchronizationResponse message
        /// should override to produce application specific request
        /// </summary>
        /// <remarks>this method will invoked On sender peer based on prior SynchronizationResponse operation</remarks>
        /// <param name="synchronizationResponse">the response from the mesh</param>
        BusinessLogicMessageBase ISynchronizationBusinessLogic.ProvideSynchronizationDetailRequest(BusinessLogicMessageBase synchronizationResponse)
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("ProvideSynchronizationDetailRequest: {0}", synchronizationResponse);
#endif
            var myStateResponse = synchronizationResponse as MyStateContainer;

            //full response
            if (myStateResponse != null)
            {
                UpdateState(myStateResponse.StateDictionary);
            }
            //partial keys only response
            else if (synchronizationResponse is MyKeysStateIdsContainer)
            {
                var myKeysStateResponse = synchronizationResponse as MyKeysStateIdsContainer;
                myKeysStateResponse.StateIds.SkipWhile(key => MyStateDictionary.ContainsKey(key));
                if (myKeysStateResponse.StateIds.Count > 0)
                {
                    return myKeysStateResponse;
                }
            }
            /*
             * returning null would stop the synchronization process
             * in this case there is no need to continue the synchronization because already got the full details
             */
            return null;
        }

        /// <summary>
        /// Generate BusinessLogicMessageBase message that will be returned back to the mesh based on given synchronizationRequest with FullDetailedResponse set to true
        /// should override to produce application specific response
        /// </summary>
        /// <param name="synchronizationDetailsRequest">the detailed request received from the mesh contain the id's that should respond with full details (partial detailed response)</param>
        /// <returns>BusinessLogicMessageBase instance or null if don't want to response</returns>
        BusinessLogicMessageBase ISynchronizationBusinessLogic.ProvideSynchronizationDetailResponse(BusinessLogicMessageBase synchronizationDetailsRequest)
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("ProvideSynchronizationDetailResponse: {0}", synchronizationDetailsRequest);
#endif
            if (synchronizationDetailsRequest is MyKeysStateIdsContainer)
            {
                var myKeysStateResponse = synchronizationDetailsRequest as MyKeysStateIdsContainer;
                lock (syncLock)
                {
                    //Filter the response to contain only the keys requested
                    var filtered = from myUserUpdateState in MyStateDictionary
                                   where myKeysStateResponse.StateIds.Contains(myUserUpdateState.Key)
                                   select myUserUpdateState;
                    var response = new MyStateContainer
                                       {
                                           StateDictionary = filtered.ToDictionary(x => x.Key, x => x.Value)
                                       };
                    return response;
                }
            }
            return null;
        }

        /// <summary>
        /// Generate response message that will be returned back to the mesh based on given synchronizationRequest with FullDetailedResponse set to true 
        /// should override to produce application specific response
        /// </summary>
        /// <returns>BusinessLogicMessageBase instance or null if don't want to response</returns>
        BusinessLogicMessageBase ISynchronizationBusinessLogic.ProvideFullSynchronizationDetailResponse()
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("ProvideFullSynchronizationDetailResponse");
#endif
            lock (syncLock)
            {
                return new MyStateContainer
                           {
                               StateDictionary = MyStateDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                           };
            }
        }

        /// <summary>
        /// Invoke when SynchronizationDetailsResponse operation received
        /// </summary>
        /// <param name="synchronizationDetailsResponse">the full detailed response</param>
        void ISynchronizationBusinessLogic.OnSynchronizationDetailsResponseReceived(BusinessLogicMessageBase synchronizationDetailsResponse)
        {
#if DEBUG
            LogManager.GetCurrentClassLogger().Debug("OnReceivedSynchronizationDetailsResponse: {0}", synchronizationDetailsResponse);
#endif
            var myStateResponse = synchronizationDetailsResponse as MyStateContainer;
            if (myStateResponse != null)
            {
                lock (syncLock)
                {
                    UpdateState(myStateResponse.StateDictionary);
                }
            }
        }

        /// <summary>
        /// called when the peer send update message , and on the other peers when they received update
        /// </summary>
        /// <param name="stateMessage">the update message</param>
        void ISynchronizationBusinessLogic.OnUpdateReceived(BusinessLogicMessageBase stateMessage)
        {
            LogManager.GetCurrentClassLogger().Info("stateMessage: {0}", stateMessage);
            var myStateResponse = stateMessage as MyStateContainer;

            //full response
            if (myStateResponse != null)
            {
                UpdateState(myStateResponse.StateDictionary);
            }
        }

        #endregion

        #region Implementation of IAnnouncementBusinessLogic

        /// <summary>
        /// called on the peer that become online to provide FullPresenceInfo that will send to the mesh with the online message
        /// </summary>
        /// <returns>FullPresenceInfo instance or null if don't want to send online message</returns>
        FullPresenceInfo IAnnouncementBusinessLogic.ProvideFullPresenceInfo()
        {
            return new MyFullPresenceInfo
            {
                CorrelateKey = id,
                UniqueName = id.ToString(),
                MyData = FullPresenceInfoData
            };
        }

        /// <summary>
        /// called when receive from the other peer online message
        /// </summary>
        /// <param name="fullPresenceInfo">contain the presence information of the peer that send the online message</param>
        void IAnnouncementBusinessLogic.OnOnlineAnnouncementReceived(FullPresenceInfo fullPresenceInfo)
        {
            if (!(fullPresenceInfo is MyFullPresenceInfo))
                return;

            var info = fullPresenceInfo as MyFullPresenceInfo;
            lock (syncLock)
            {
                MyUserUpdateState myUserUpdateState;
                if (!MyStateDictionary.TryGetValue(info.CorrelateKey, out myUserUpdateState))
                {
                    myUserUpdateState = new MyUserUpdateState
                                            {
                                                FullPresenceInfo = info
                                            };
                    MyStateDictionary.Add(info.CorrelateKey, myUserUpdateState);
                }
                else
                {
                    MyStateDictionary[info.CorrelateKey].FullPresenceInfo = info;
                }
            }
        }

        /// <summary>
        /// called on the peer closing and become offline to provide CompactPresenceInfo that will send to the mesh with the offline message
        /// </summary>
        /// <returns>CompactPresenceInfo instance or null if don't want to send offline message</returns>
        CompactPresenceInfo IAnnouncementBusinessLogic.ProvideCompactPresenceInfo()
        {
            var response = new MyCompactPresenceInfo
            {
                UniqueName = id.ToString(),
                CorrelateKey = id
            };
            return response;
        }

        /// <summary>
        /// called when receive from the other peer offline message
        /// </summary>
        /// <param name="compactPresenceInfo">contain the presence information of the peer that send the message</param>
        void IAnnouncementBusinessLogic.OnOfflineAnnouncementReceived(CompactPresenceInfo compactPresenceInfo)
        {
            if (compactPresenceInfo is MyCompactPresenceInfo)
            {
                var info = compactPresenceInfo as MyCompactPresenceInfo;
                var removed = false;
                MyUserUpdateState state;
                lock (syncLock)
                {
                    if (MyStateDictionary.TryGetValue(info.CorrelateKey, out state))
                    {
                        removed = MyStateDictionary.Remove(info.CorrelateKey);
                    }
                }
                if (removed)
                    LogManager.GetCurrentClassLogger().Debug("peer with key: {0} and name: {1} go offline",
                                                             info.CorrelateKey, state.Name);
                else
                    LogManager.GetCurrentClassLogger().Debug("peer with key: {0} was not found !",
                                                             info.CorrelateKey);
            }
        }

        /// <summary>
        /// called on other peer when some peer changed his presence information
        /// </summary>
        /// <param name="fullPresenceInfo">The new presence information of the peer that sent the message</param>
        void IAnnouncementBusinessLogic.OnPresenceInfoChangedReceived(FullPresenceInfo fullPresenceInfo)
        {
            LogManager.GetCurrentClassLogger().Debug("OnReceivePresenceInfoChanged , do nothing in this example");
        }

        #endregion

        #region private methods

        private void UpdateState(Dictionary<Guid, MyUserUpdateState> myUserUpdateStates)
        {
            lock (syncLock)
            {
                foreach (var kvp in myUserUpdateStates)
                {
                    MyUserUpdateState value;
                    if (MyStateDictionary.TryGetValue(kvp.Key,out value))
                    {
                        if(value.IsOwnPeerData)
                            continue;
                    }
                    MyStateDictionary[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// cleaning the MyStateDictionary
        /// </summary>
        private void ClearStateDictionary()
        {
            lock (syncLock)
            {
                MyStateDictionary.Clear();
            }
        }

        /// <summary>
        /// add the own data to the MyStateDictionary
        /// </summary>
        private void AddOwnDataStateDictionary()
        {
            LogManager.GetCurrentClassLogger().Debug("AddOwnDataStateDictionary before lock {0}", userUpdateState.Name);
            //update own data
            lock (syncLock)
            {
                //if (!MyStateDictionary.ContainsKey(id))
                //{
                //MyStateDictionary.Add(id, userUpdateState);
                //}
                MyStateDictionary[id] = userUpdateState;
                LogManager.GetCurrentClassLogger().Debug("AddOwnDataStateDictionary {0}", userUpdateState.Name);
            }
        }
        #endregion

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}