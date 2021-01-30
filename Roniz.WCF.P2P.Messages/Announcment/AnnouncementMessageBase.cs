using System.ServiceModel;

namespace Roniz.WCF.P2P.Messages.Announcment
{
    [MessageContract(WrapperNamespace = "http://Roniz.WCF.P2P.Messages.Announcment")]
    public abstract class AnnouncementMessageBase : FloodMessageBase
    {
        #region constructores

        protected AnnouncementMessageBase()
        {

        }

        protected AnnouncementMessageBase(bool neighborOnly)
            : base(neighborOnly)
        {

        }

        protected AnnouncementMessageBase(int hopCount)
            : base(hopCount)
        {

        }
        #endregion

        #region Properties

        #endregion
    }
}