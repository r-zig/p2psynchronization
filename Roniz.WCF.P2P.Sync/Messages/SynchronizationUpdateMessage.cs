using System.ServiceModel;
using Roniz.WCF.P2P.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.Sync.Messages
{
    /// <summary>
    /// The update message from peer to all mesh
    /// </summary>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Sync.Messages")]
    public sealed class SynchronizationUpdateMessage : FloodMessageBase
    {
        #region properties
        /// <summary>
        /// The actual state , represent by class to support application specific (derived class) detailed state
        /// </summary>
        [MessageBodyMember]
        public BusinessLogicMessageBase State { get; set; }
        #endregion
    }
}