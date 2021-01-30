using Roniz.WCF.P2P.Messages.Presence;

namespace Roniz.WCF.P2P.Sync.Interfaces
{
    /// <summary>
    /// The interface that used to provide online , offline and presence changed information
    /// </summary>
    [System.Diagnostics.Contracts.ContractClass(typeof(ContractForIAnnouncementBusinessLogic))]
    public interface IAnnouncementBusinessLogic
    {
        /// <summary>
        /// called on the peer that become online to provide FullPresenceInfo that will send to the mesh with the online message
        /// </summary>
        /// <returns>FullPresenceInfo instance or null if don't want to send online message</returns>
        FullPresenceInfo ProvideFullPresenceInfo();

        /// <summary>
        /// called when receive from the other peer online message
        /// </summary>
        /// <param name="fullPresenceInfo">contain the presence information of the peer that send the online message</param>
        void OnOnlineAnnouncementReceived(FullPresenceInfo fullPresenceInfo);


        /// <summary>
        /// called on the peer closing and become offline to provide CompactPresenceInfo that will send to the mesh with the offline message
        /// </summary>
        /// <returns>CompactPresenceInfo instance or null if don't want to send offline message</returns>
        CompactPresenceInfo ProvideCompactPresenceInfo();

        /// <summary>
        /// called when receive from the other peer offline message
        /// </summary>
        /// <param name="compactPresenceInfo">contain the presence information of the peer that send the message</param>
        void OnOfflineAnnouncementReceived(CompactPresenceInfo compactPresenceInfo);

        /// <summary>
        /// called on other peer when some peer changed his presence information
        /// </summary>
        /// <param name="fullPresenceInfo">The new presence information of the peer that sent the message</param>
        void OnPresenceInfoChangedReceived(FullPresenceInfo fullPresenceInfo);
    }
}
