using System;
using System.Diagnostics.Contracts;
using Roniz.WCF.P2P.Messages.Announcment;
using Roniz.WCF.P2P.Messages.Presence;
using Roniz.WCF.P2P.Sync.Messages;

namespace Roniz.WCF.P2P.Sync.Interfaces
{
    [ContractClassFor(typeof(ISynchronizedState))]
    abstract class ContractForISynchronizedState : ISynchronizedState
    {
        #region Implementation of ISynchronizedState

        /// <summary>
        /// Peer should send it automatically prior closing the connection to the mesh
        /// </summary>
        void ISynchronizedState.OfflineAnnouncement(OfflineAnnouncementMessage announcementMessage)
        {
            Contract.Requires<ArgumentNullException>(announcementMessage != null);
        }

        /// <summary>
        /// Peer should send it automatically when the mesh become online
        /// </summary>
        /// <param name="announcementMessage"></param>
        void ISynchronizedState.OnlineAnnouncement(OnlineAnnouncementMessage announcementMessage)
        {
            Contract.Requires<ArgumentNullException>(announcementMessage != null);
        }

        /// <summary>
        /// When presence changed
        /// </summary>
        /// <remarks>For example , when peer change it's listening endpoint address and want to announce it to the mesh</remarks>
        /// <param name="presenceInfoChangedMessage"></param>
        void ISynchronizedState.PresenceInfoChanged(PresenceInfoChangedMessage presenceInfoChangedMessage)
        {
            Contract.Requires<ArgumentNullException>(presenceInfoChangedMessage != null);
        }

        /// <summary>
        /// The first request usually from the peer that joined to the mesh
        /// that ask for synchronization
        /// </summary>
        /// <param name="request">the request from the peer to his neighbors</param>
        void ISynchronizedState.SynchronizationRequest(SynchronizationRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null);
        }

        /// <summary>
        /// The response that usually from the peers that received prior the SynchronizationRequest
        /// </summary>
        /// <param name="response">the response from the peers to the requester peer</param>
        void ISynchronizedState.SynchronizationKeysResponse(SynchronizationResponseContainer response)
        {
            Contract.Requires<ArgumentNullException>(response != null);
        }

        /// <summary>
        /// The second request that usually from the peer that received prior the SynchronizationResponse
        /// now he ask for response details from one of the peers (usually the first one to response prior)  more detailed message
        /// </summary>
        /// <param name="request">the request from the peer</param>
        void ISynchronizedState.SynchronizationDetailsRequest(SynchronizationDetailsRequestContainer request)
        {
            Contract.Requires<ArgumentNullException>(request != null);
        }

        /// <summary>
        /// The response the peer that received prior the SynchronizationDetailsRequest
        /// </summary>
        /// <param name="response">the request from the peer</param>
        void ISynchronizedState.SynchronizationDetailsResponse(SynchronizationDetailsResponseContainer response)
        {
            Contract.Requires<ArgumentNullException>(response != null);
        }

        /// <summary>
        /// Update the mesh with new data
        /// </summary>
        /// <param name="synchronizationUpdateMessage">the update message that sent from the peer to the mesh</param>
        void ISynchronizedState.SynchronizationUpdate(SynchronizationUpdateMessage synchronizationUpdateMessage)
        {
            Contract.Requires<ArgumentNullException>(synchronizationUpdateMessage != null);
        }

        #endregion
    }
}
