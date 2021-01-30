using System.ServiceModel;
using Roniz.WCF.P2P.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.Sync.Messages
{
    /// <summary>
    /// The first request from peer to his neighbors for their states
    /// </summary>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Sync.Messages")]
    public sealed class SynchronizationResponseContainer : NeighborOnlyMessage
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
