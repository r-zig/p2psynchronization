using System.ServiceModel;

namespace Roniz.WCF.P2P.Messages.Presence
{
    /// <summary>
    /// Online message to the other peers
    /// </summary>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Messages.Presence")]
    public sealed class PresenceInfoChangedMessage : FloodMessageBase
    {
        [MessageBodyMember]
        public FullPresenceInfo PresenceInfo { get; set; }

        #region methods
        public override string ToString()
        {
            return string.Format("{0} , PresenceInfo: {1}", base.ToString() , PresenceInfo != null ? PresenceInfo.ToString() : "null");
        }
        #endregion
    }
}