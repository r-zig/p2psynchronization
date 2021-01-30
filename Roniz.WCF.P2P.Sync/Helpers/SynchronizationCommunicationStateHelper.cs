using System;
using System.ServiceModel;
using Roniz.WCF.P2P.Sync.Enums;
using Roniz.WCF.P2P.Sync.Properties;

namespace Roniz.WCF.P2P.Sync.Helpers
{
    public static class SynchronizationCommunicationStateHelper
    {
        public static SynchronizationCommunicationState GetState(ICommunicationObject communicationObject, PeerNode peerNode)
        {
            if (communicationObject == null)
                return SynchronizationCommunicationState.Created;
            if (peerNode != null && peerNode.IsOnline)
                return SynchronizationCommunicationState.Online;

            return Convert(communicationObject.State);
        }

        private static SynchronizationCommunicationState Convert(CommunicationState communicationState)
        {
            switch (communicationState)
            {
                case CommunicationState.Closed:
                    return SynchronizationCommunicationState.Closed;
                case CommunicationState.Closing:
                    return SynchronizationCommunicationState.Closing;
                case CommunicationState.Created:
                    return SynchronizationCommunicationState.Created;
                case CommunicationState.Faulted:
                    return SynchronizationCommunicationState.Faulted;
                case CommunicationState.Opened:
                    return SynchronizationCommunicationState.Opened;
                case CommunicationState.Opening:
                    return SynchronizationCommunicationState.Opening;
                default:
                    throw new ArgumentException(Resources.SynchronizationCommunicationStateHelper_Convert_communicationState_contain_state_that_cannot_converted, "communicationState");
            }
        }

        /// <summary>
        /// Determine if the given state is opened (online is also opened state)
        /// </summary>
        /// <param name="state">the SynchronizationCommunicationState to test</param>
        /// <returns>
        /// return true if the given state is in opened or online state
        /// otherwise false
        /// </returns>
        public static bool IsOpened(SynchronizationCommunicationState state)
        {
            switch (state)
            {
                case SynchronizationCommunicationState.Opened:
                    return true;
                case SynchronizationCommunicationState.Online:
                    return true;
                default:
                    return false;
            }
        }
    }
}