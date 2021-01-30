using System.ServiceModel;

namespace Roniz.WCF.P2P.Sync.Filters
{
    /// <summary>
    /// Only messages from remote peers should propagate
    /// </summary>
    class RemoteOnlyMessagePropagationFilter : PeerMessagePropagationFilter
    {
        public override PeerMessagePropagation ShouldMessagePropagate(System.ServiceModel.Channels.Message message, PeerMessageOrigination origination)
        {
            var destination = PeerMessagePropagation.LocalAndRemote;

            if (origination == PeerMessageOrigination.Local)

                destination = PeerMessagePropagation.Remote;

            return destination;
        }
    }
}
