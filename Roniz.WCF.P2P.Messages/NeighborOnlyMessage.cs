using System.ServiceModel;

namespace Roniz.WCF.P2P.Messages
{
    /// <summary>
    /// Represent message that will send only to neighbor peers (peers that connect directly with hop count 1 to the sender peer)
    /// </summary>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Messages")]
    public abstract class NeighborOnlyMessage:FloodMessageBase
    {
        #region constructor

        protected NeighborOnlyMessage():base(true)
        {
        }

        #endregion
    }
}
