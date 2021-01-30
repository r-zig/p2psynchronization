using System.Runtime.Serialization;

namespace Roniz.WCF.P2P.Sync.Messages.BusinessLogic
{
    /// <summary>
    /// base class for synchronization response
    /// </summary>
    [DataContract(Namespace = "http://Roniz.WCF.P2P.Sync.Messages.BusinessLogic")]
    public abstract class BusinessLogicSynchronizationResponseBase : BusinessLogicResponseBase
    {
    }
}