using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.ApplicationTester.Messages
{
    [DataContract]
    public class MyKeysStateIdsContainer : BusinessLogicMessageBase
    {
        [DataMember]
        public List<Guid> StateIds {get; set;}
    }
}