using System;
using System.Runtime.Serialization;
using Roniz.WCF.P2P.Messages.Presence;

namespace Roniz.WCF.P2P.ApplicationTester.Messages
{
    [DataContract(Namespace = "http://Roniz.WCF.P2P.ApplicationTester.Messages")]
    public class MyFullPresenceInfo : FullPresenceInfo
    {
        #region properties
        [DataMember]
        public string MyData
        {
            get; set;
        }

        [DataMember]
        public Guid CorrelateKey { get; set; }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format("UniqueName: {0} , MyData: {1} , CorrelateKey: {2}", UniqueName ?? "null", MyData ?? "null", CorrelateKey);
        }
        #endregion
    }
}
