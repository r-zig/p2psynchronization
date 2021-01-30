using System;
using System.ServiceModel;

namespace Roniz.WCF.P2P.Messages
{
    /// <summary>
    /// Base class that all peerchannel messages should inherit from if want more details about the originator of message that the PeerChannel not give yet
    /// </summary>
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Messages")]
    public abstract class FloodMessageBase
    {
        #region members
        public const int MaxHopCount = Int32.MaxValue;

        [PeerHopCount]
        protected int CurrentHopCount;

        [MessageHeader]
        protected int OriginalHopCount;
        #endregion

        #region constructors
        /// <summary>
        /// Initialize default message that will flood to all mesh
        /// </summary>
        protected FloodMessageBase():this(MaxHopCount)
        {
        }

        /// <summary>
        /// Initialize the message with the number of hops that it can reach
        /// </summary>
        /// <param name="hopCount">the number of hops the message will flood in the mesh</param>
        protected FloodMessageBase(int hopCount)
        {
            HopCount = hopCount;
        }

        /// <summary>
        /// Initialize the message in neighborOnly mode or regular flood mode
        /// </summary>
        /// <param name="neighborOnly">when true the message will be sent only to neighbors , otherwise to all peers</param>
        protected FloodMessageBase(bool neighborOnly):this(neighborOnly?1:MaxHopCount)
        {
        }
        #endregion

        #region properties
        /// <summary>
        /// return the hop count from the originator of this message
        /// for example , if peer X send message to all data mesh
        /// the message flow to his neighbor "peer Y" and he forward it to "peer Z"
        /// the distance will be this.originalHopCount - this.currentHopCount
        /// </summary>
        public int HopCountDistance
        {
            get
            {
                return OriginalHopCount - CurrentHopCount;
            }
        }

        /// <summary>
        /// determine if the message came from Neighbour
        /// which is a peer that connected directly to this peer
        /// </summary>
        public bool IsNeighbourMessage
        {
            get
            {
                return HopCountDistance == 1 ? true : false;
            }
        }

        /// <summary>
        /// determine if the message sent by this peer , and maybe should ignored
        /// </summary>
        public bool IsOwnMessage
        {
            get
            {
                return CurrentHopCount == OriginalHopCount;
            }
        }

        public int HopCount
        {
            get
            {
                return CurrentHopCount;
            }
            set
            {
                if (value == CurrentHopCount)
                    return;

                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "must be > 0");
                OriginalHopCount = CurrentHopCount = value;
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format("CurrentHopCount: {0}, OriginalHopCount: {1}, HopCount: {2}, HopCountDistance: {3}, IsNeighbourMessage: {4}, IsOwnMessage: {5}", CurrentHopCount, OriginalHopCount, HopCount, HopCountDistance, IsNeighbourMessage, IsOwnMessage);
        }
        #endregion
    }
}
