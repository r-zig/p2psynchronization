using System.ServiceModel;
using Roniz.WCF.P2P.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.Sync.Messages
{
    /// <summary>
    /// The second response from peer to his neighbors for their full states
    /// This is the container class for the application specific business logic response
    /// The response for the second request from peer to his neighbors for their detailed states , or for SynchronizationRequest when it's FullDetailedResponse flag is true
    /// </summary>
    /// <see cref="SynchronizationRequest"/>
    /// <seealso cref="SynchronizationDetailsRequestContainer"/>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Sync.Messages")]
    public sealed class SynchronizationDetailsResponseContainer : NeighborOnlyMessage
    {
        #region properties
        /// <summary>
        /// The actual response , represent by class to support application specific (derived class) details
        /// </summary>
        [MessageBodyMember]
        public BusinessLogicMessageBase Response { get; set; }

        #endregion
    }
}
