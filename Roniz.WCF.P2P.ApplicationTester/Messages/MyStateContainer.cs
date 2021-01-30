using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.ApplicationTester.Messages
{
    [DataContract]
    public class MyStateContainer : BusinessLogicMessageBase
    {
        [DataMember]
        public Dictionary<Guid, MyUserUpdateState> StateDictionary { get; set; }
    }
}