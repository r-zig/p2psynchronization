using System.ServiceModel;
using Roniz.WCF.P2P.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.Sync.Messages
{
    /// <summary>
    /// The second request from peer to his neighbors for their full states
    /// </summary>
    /// <see cref="SynchronizationDetailsResponseContainer"/>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Sync.Messages")]
    public sealed class SynchronizationDetailsRequestContainer : NeighborOnlyMessage
    {
        #region properties
        [MessageBodyMember]
        public BusinessLogicMessageBase Request { get; set; }
        #endregion
    }
}
