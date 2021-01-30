using System.ServiceModel;

namespace Roniz.WCF.P2P.Messages
{
    class DefaultPeerMessagePropagationFilter : PeerMessagePropagationFilter
    {
        public override PeerMessagePropagation ShouldMessagePropagate(System.ServiceModel.Channels.Message message, PeerMessageOrigination origination)
        {
            return PeerMessagePropagation.LocalAndRemote;
        }
    }
}
