using System;
using System.Runtime.Serialization;
using Roniz.WCF.P2P.Messages.Presence;

namespace Roniz.WCF.P2P.ApplicationTester.Messages
{
    [DataContract]
    public class MyCompactPresenceInfo : CompactPresenceInfo
    {
        [DataMember]
        public Guid CorrelateKey { get; set; }
    }
}