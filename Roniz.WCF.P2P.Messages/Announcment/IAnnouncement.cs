using System.ServiceModel;
using Roniz.WCF.P2P.Messages.Presence;

namespace Roniz.WCF.P2P.Messages.Announcment
{
    [ServiceContract(Namespace = "http://Roniz.WCF.P2P.Messages.Announcment")]
    public interface IAnnouncement
    {
        /// <summary>
        /// Peer should send it automatically prior closing the connection to the mesh
        /// </summary>
        [OperationContract(IsOneWay=true)]
        void OfflineAnnouncement(OfflineAnnouncementMessage announcementMessage);

        /// <summary>
        /// Peer should send it automatically when the mesh become online
        /// </summary>
        /// <param name="announcementMessage"></param>
        [OperationContract(IsOneWay = true)]
        void OnlineAnnouncement(OnlineAnnouncementMessage announcementMessage);

        /// <summary>
        /// When presence changed
        /// </summary>
        /// <remarks>For example , when peer change it's listening endpoint address and want to announce it to the mesh</remarks>
        /// <param name="presenceInfoChangedMessage"></param>
        [OperationContract(IsOneWay = true)]
        void PresenceInfoChanged(PresenceInfoChangedMessage presenceInfoChangedMessage);
    }
}
