using System.Runtime.Serialization;

namespace Roniz.WCF.P2P.Sync.Messages.BusinessLogic
{
    /// <summary>
    /// base business logic update state message that used when want to mix message contract to allow infrastructure 
    /// specific control over the message , and DataContract with [KnownType] to allow application specific details in derived classes
    /// </summary>
    [DataContract(Namespace = "http://Roniz.WCF.P2P.Sync.Messages.BusinessLogic")]
    public class BusinessLogicUpdateStateMessage : BusinessLogicMessageBase
    {
    }
}