using System;
using System.Diagnostics.Contracts;
using Roniz.WCF.P2P.Messages;
using Roniz.WCF.P2P.Sync.Enums;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.WCF.P2P.Sync.Interfaces
{
    [ContractClass(typeof(ContractForISynchronizationStateManager))]
    public interface ISynchronizationStateManager
    {
        #region properties
        SynchronizationCommunicationState State { get; }
        #endregion

        #region events
        /// <summary>
        /// when peer become online
        /// </summary>
        event EventHandler PeerOnline;

        /// <summary>
        /// when peer become offline
        /// </summary>
        event EventHandler PeerOffline;
        #endregion

        #region methods
        /// <summary>
        /// open the p2p channel and register for online and offline events
        /// </summary>
        void Open();

        /// <summary>
        /// Close gracefully the communication
        /// </summary>
        void Close();

        /// <summary>
        /// Update the neighbors peers with new state information
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">The new state information</param>
        void Update<TState>(TState state) where TState : BusinessLogicMessageBase;

        /// <summary>
        /// Update the peers with new state information
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">The new state information</param>
        /// <param name="neighborOnly">determine if to update only the neighbor peers or all the mesh</param>
        void Update<TState>(TState state, bool neighborOnly = false) where TState : BusinessLogicMessageBase;

        /// <summary>
        /// Update the peers with new state information
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">The new state information</param>
        /// <param name="hops">determine how much hops the update message will flow in the mesh</param>
        void Update<TState>(TState state, int hops = FloodMessageBase.MaxHopCount) where TState : BusinessLogicMessageBase;
        #endregion
    }
}
