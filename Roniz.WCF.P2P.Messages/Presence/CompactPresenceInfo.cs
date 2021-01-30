using System.Runtime.Serialization;

namespace Roniz.WCF.P2P.Messages.Presence
{
    /// <summary>
    /// class that will provide compact presence information
    /// </summary>
    /// <see cref="FullPresenceInfo"/>
    /// <see cref="Roniz.WCF.P2P.Messages.Announcment.OfflineAnnouncementMessage"/>
    /// <remarks>
    /// For example , when peer want to announce that he listening on some endpoint address
    /// he can specified this information in class derived from FullPresenceInfo class.
    /// messages like OnlineAnnouncementMessage will provide this information.
    /// but other messages that only want to tell "who is the sender" will use derived class of CompactPresenceInfo instead to allow smaller message size
    /// </remarks>
    [DataContract(Namespace = "http://Roniz.WCF.P2P.Messages.Presence")]
    public class CompactPresenceInfo : PresenceInfoBase
    {
    }
}