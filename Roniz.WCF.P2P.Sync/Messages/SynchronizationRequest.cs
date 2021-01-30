using System.ServiceModel;
using Roniz.WCF.P2P.Messages;

namespace Roniz.WCF.P2P.Sync.Messages
{
    /// <summary>
    /// The first request from peer to his neighbors for their states
    /// </summary>
    /// <see cref="SynchronizationResponseContainer"/>
    /// <seealso cref="SynchronizationDetailsResponseContainer"/>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Sync.Messages")]
    public sealed class SynchronizationRequest:NeighborOnlyMessage
    {
        #region properties
        /// <summary>
        /// determine if the response should contain full detailed response (via SynchronizationResponse), or only list of guid that points to the full response (via SynchronizationResponse)
        /// </summary>
        [MessageBodyMember]
        public bool FullDetailedResponse { get; set; }

        #endregion
    }
}
