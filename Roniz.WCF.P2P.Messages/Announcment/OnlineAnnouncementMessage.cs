using System.ServiceModel;
using Roniz.WCF.P2P.Messages.Presence;

namespace Roniz.WCF.P2P.Messages.Announcment
{
    /// <summary>
    /// Online message to the other peers
    /// </summary>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Messages.Announcment")]
    public sealed class OnlineAnnouncementMessage : AnnouncementMessageBase
    {
        #region properties

        [MessageBodyMember]
        public FullPresenceInfo PresenceInfo { get; set; }

        #endregion

        #region methods

        public override string ToString()
        {
            return string.Format("{0} , PresenceInfo: {1}", base.ToString(), PresenceInfo != null ? PresenceInfo.ToString() : "null");
        }

        #endregion
    }
}