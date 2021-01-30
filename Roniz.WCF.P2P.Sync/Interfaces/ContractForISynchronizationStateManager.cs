using System;
using System.Diagnostics.Contracts;
using Roniz.WCF.P2P.Sync.Enums;

namespace Roniz.WCF.P2P.Sync.Interfaces
{
    [ContractClassFor(typeof(ISynchronizationStateManager))]
    abstract class ContractForISynchronizationStateManager : ISynchronizationStateManager
    {
        #region Implementation of ISynchronizationStateManager

        public SynchronizationCommunicationState State { get; private set; }

        event EventHandler ISynchronizationStateManager.PeerOnline
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler ISynchronizationStateManager.PeerOffline
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        void ISynchronizationStateManager.Open()
        {
            Contract.Requires<InvalidOperationException>(
                (State != SynchronizationCommunicationState.Opening && 
                 State != SynchronizationCommunicationState.Opened),
                Properties.Resources.OpenErrorOpeningOpened);
        }

        void ISynchronizationStateManager.Close()
        {
            throw new NotImplementedException();
        }

        void ISynchronizationStateManager.Update<TState>(TState state)
        {
            Contract.Requires<ArgumentNullException>(state != null);
            Contract.Requires<InvalidOperationException>(
                (State == SynchronizationCommunicationState.Opened || State == SynchronizationCommunicationState.Online),
                Properties.Resources.OperationNotAllowedNotOpenedOrOnlineComunicationState);
        }

        void ISynchronizationStateManager.Update<TState>(TState state, bool neighborOnly)
        {
            Contract.Requires<ArgumentNullException>(state != null);
            Contract.Requires<InvalidOperationException>(
                (State == SynchronizationCommunicationState.Opened || State == SynchronizationCommunicationState.Online),
                Properties.Resources.OperationNotAllowedNotOpenedOrOnlineComunicationState);
        }

        void ISynchronizationStateManager.Update<TState>(TState state, int hops)
        {
            Contract.Requires<ArgumentNullException>(state != null);
            Contract.Requires<InvalidOperationException>(
                (State == SynchronizationCommunicationState.Opened || State == SynchronizationCommunicationState.Online),
                Properties.Resources.OperationNotAllowedNotOpenedOrOnlineComunicationState);
        }

        #endregion
    }
}