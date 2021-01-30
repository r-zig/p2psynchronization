using Roniz.WCF.P2P.Sync.Enums;
using Roniz.WCF.P2P.Sync.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.Sync.Interfaces
{
    /// <summary>
    /// the base interface that injected into the SynchronizationStateManager
    /// </summary>
    /// <see cref="SynchronizationStateManager"/>
    public interface ISynchronizationBusinessLogic : IAnnouncementBusinessLogic
    {
        #region properties
        /// <summary>
        /// determine if the bussiness logic should need a full detailed synchronization or not
        /// </summary>
        /// <remarks>
        /// When synchronization process begin the requeting peer ask the mesh or for partial or for full synchronization
        /// and the decision what to ask is based on the value of this property.
        /// for example: in secnario of peer that initiate without any data , maybe it will be better to ask for full synchronization
        /// but in peer that already have some data , maybe it will ask only to synchronize the data it don't have yet
        /// </remarks>
        bool IsNeedFullSynchronization { get; set; }
        #endregion

        #region methods
        
        /// <summary>
        /// called when the State changed
        /// </summary>
        /// <param name="oldState">The last state prior to the change</param>
        /// <param name="newState">The new state after the change</param>
        /// <remarks>
        /// The developer does not must implement this method , this is only extension point
        /// </remarks>
        void OnCommunicationStateChanged(SynchronizationCommunicationState oldState,SynchronizationCommunicationState newState);

        /// <summary>
        /// Generate SynchronizationResponse message that will be returned back to the mesh based on given synchronizationRequest message
        /// should override to produce application specific response
        /// </summary>
        /// <remarks>this method will invoked On receiver peer side based on prior SynchronizationRequest operation</remarks>
        /// <param name="synchronizationRequest">the request received from the mesh</param>
        /// <returns>BusinessLogicMessageBase instance or null if don't want to response</returns>
        BusinessLogicMessageBase ProvideSynchronizationResponse(SynchronizationRequest synchronizationRequest);

        /// <summary>
        /// Generate SynchronizationDetailsRequest message that will be send back to the mesh by the peer that want to synchronized itself based on given SynchronizationResponse message
        /// should override to produce application specific request
        /// </summary>
        /// <remarks>this method will invoked On sender peer based on prior SynchronizationResponse operation</remarks>
        /// <param name="synchronizationResponse">the response from the mesh</param>
        /// <returns>BusinessLogicMessageBase instance or null if don't want to request nothing</returns>
        BusinessLogicMessageBase ProvideSynchronizationDetailRequest(BusinessLogicMessageBase synchronizationResponse);

        /// <summary>
        /// Generate response message that will be returned back to the mesh based on given synchronizationRequest with FullDetailedResponse set to true or based on request message
        /// should override to produce application specific response
        /// </summary>
        /// <param name="synchronizationDetailsRequest">the detailed request received from the mesh contain the id's that should respond with full details (partial detailed response)</param>
        /// <returns>BusinessLogicMessageBase instance or null if don't want to response</returns>
        BusinessLogicMessageBase ProvideSynchronizationDetailResponse(BusinessLogicMessageBase synchronizationDetailsRequest);

        /// <summary>
        /// Generate response message that will be returned back to the mesh based on given synchronizationRequest with FullDetailedResponse set to true 
        /// should override to produce application specific response
        /// </summary>
        /// <returns>BusinessLogicMessageBase instance or null if don't want to response</returns>
        BusinessLogicMessageBase ProvideFullSynchronizationDetailResponse();

        /// <summary>
        /// Invoke when SynchronizationDetailsResponse operation received
        /// </summary>
        /// <param name="synchronizationDetailsResponse">the full detailed response</param>
        void OnSynchronizationDetailsResponseReceived(BusinessLogicMessageBase synchronizationDetailsResponse);

        /// <summary>
        /// called when receive from the other peer update message
        /// </summary>
        /// <param name="stateMessage">the update message</param>
        void OnUpdateReceived(BusinessLogicMessageBase stateMessage);
        #endregion
    }
}