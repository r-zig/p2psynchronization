using System.Diagnostics.Contracts;
using System.ServiceModel;
using Roniz.WCF.P2P.Messages.Announcment;
using Roniz.WCF.P2P.Messages.Presence;
using Roniz.WCF.P2P.Sync.Messages;

namespace Roniz.WCF.P2P.Sync.Interfaces
{
    [ServiceContract(Namespace = "http://Roniz.WCF.P2P.Sync.Interfaces.ISynchronizedState", CallbackContract = typeof(ISynchronizedState))]
    [ContractClass(typeof(ContractForISynchronizedState))]
    public interface ISynchronizedState
    {
        #region Announcement messages
        /// <summary>
        /// Peer should send it automatically prior closing the connection to the mesh
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OfflineAnnouncement(OfflineAnnouncementMessage announcementMessage);

        /// <summary>
        /// Peer should send it automatically when the mesh become online
        /// </summary>
        /// <param name="announcementMessage"></param>
        [OperationContract(IsOneWay = true)]
        void OnlineAnnouncement(OnlineAnnouncementMessage announcementMessage);

        /// <summary>
        /// When presence changed
        /// </summary>
        /// <remarks>For example , when peer change it's listening endpoint address and want to announce it to the mesh</remarks>
        /// <param name="presenceInfoChangedMessage"></param>
        [OperationContract(IsOneWay = true)]
        void PresenceInfoChanged(PresenceInfoChangedMessage presenceInfoChangedMessage);

        #endregion

        /// <summary>
        /// The first request usually from the peer that joined to the mesh
        /// that ask for synchronization
        /// </summary>
        /// <param name="request">the request from the peer to his neighbors</param>
        [OperationContract(IsOneWay = true)]
        void SynchronizationRequest(SynchronizationRequest request);

        /// <summary>
        /// The response that usually from the peers that received prior the SynchronizationRequest
        /// </summary>
        /// <param name="response">the response from the peers to the requester peer</param>
        [OperationContract(IsOneWay = true)]
        void SynchronizationKeysResponse(SynchronizationResponseContainer response);

        /// <summary>
        /// The second request that usually from the peer that received prior the SynchronizationResponse
        /// now he ask for response details from one of the peers (usually the first one to response prior)  more detailed message
        /// </summary>
        /// <param name="request">the request from the peer</param>
        [OperationContract(IsOneWay = true)]
        void SynchronizationDetailsRequest(SynchronizationDetailsRequestContainer request);

        /// <summary>
        /// The response the peer that received prior the SynchronizationDetailsRequest
        /// </summary>
        /// <param name="response">the request from the peer</param>
        [OperationContract(IsOneWay = true)]
        void SynchronizationDetailsResponse(SynchronizationDetailsResponseContainer response);

        /// <summary>
        /// Update the mesh with new data
        /// </summary>
        /// <param name="synchronizationUpdateMessage">the update message that sent from the peer to the mesh</param>
        [OperationContract(IsOneWay = true)]
        void SynchronizationUpdate(SynchronizationUpdateMessage synchronizationUpdateMessage);
    }
}