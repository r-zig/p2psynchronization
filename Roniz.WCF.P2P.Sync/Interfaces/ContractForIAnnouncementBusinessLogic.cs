using System;
using System.Diagnostics.Contracts;
using Roniz.WCF.P2P.Messages.Presence;

namespace Roniz.WCF.P2P.Sync.Interfaces
{
    [ContractClassFor(typeof(IAnnouncementBusinessLogic))]
    abstract class ContractForIAnnouncementBusinessLogic : IAnnouncementBusinessLogic
    {
        void IAnnouncementBusinessLogic.OnPresenceInfoChangedReceived(FullPresenceInfo fullPresenceInfo)
        {

        }

        void IAnnouncementBusinessLogic.OnOfflineAnnouncementReceived(CompactPresenceInfo compactPresenceInfo)
        {
            Contract.Requires<ArgumentNullException>(compactPresenceInfo != null);
        }

        CompactPresenceInfo IAnnouncementBusinessLogic.ProvideCompactPresenceInfo()
        {
            throw new NotImplementedException();
        }

        void IAnnouncementBusinessLogic.OnOnlineAnnouncementReceived(FullPresenceInfo fullPresenceInfo)
        {
            Contract.Requires<ArgumentNullException>(fullPresenceInfo != null);
        }

        FullPresenceInfo IAnnouncementBusinessLogic.ProvideFullPresenceInfo()
        {
            throw new NotImplementedException();
        }
    }
}
