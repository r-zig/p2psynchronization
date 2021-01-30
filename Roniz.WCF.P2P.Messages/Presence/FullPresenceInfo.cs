using System.Runtime.Serialization;

namespace Roniz.WCF.P2P.Messages.Presence
{
    /// <summary>
    /// abstract class that will provide full presence information
    /// </summary>
    /// <see cref="Roniz.WCF.P2P.Messages.Announcment.OnlineAnnouncementMessage"/>
    /// <see cref="CompactPresenceInfo"/>
    /// <remarks>
    /// For example , when peer want to announce that he listening on some endpoint address
    /// he can specified this information in this class derived from this class.
    /// messages like OnlineAnnouncementMessage will provide this information.
    /// but other messages that only want to tell "who is the sender" will use the CompactPresenceInfo instead to allow smaller message size
    /// </remarks>
    [DataContract(Namespace = "http://Roniz.WCF.P2P.Messages.Presence")]
    public abstract class FullPresenceInfo : PresenceInfoBase
    {
    }
}