using System.Runtime.Serialization;

namespace Roniz.WCF.P2P.Messages.Presence
{
    /// <summary>
    /// Base class for presence information
    /// </summary>
    [DataContract(Namespace = "http://Roniz.WCF.P2P.Messages.Presence")]
    public class PresenceInfoBase
    {
        #region properties

        /// <summary>
        /// unique name identifier of the peer
        /// </summary>
        [DataMember]
        public string UniqueName { get; set; }

        #endregion

        #region methods

        public override string ToString()
        {
            return string.Format("UniqueName: {0}", UniqueName ?? "null");
        }

        #endregion
    }
}
