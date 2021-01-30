namespace Roniz.WCF.P2P.Sync.Enums
{
    /// <summary>
    /// The various states that the sync manager can be in.
    /// "inherit" (logic , not code because it's enum) from CommunicationState , and add the Online state
    /// </summary>
    public enum SynchronizationCommunicationState
    {
        Created = 0,
        Opening = 1,
        Opened = 2,
        Closing = 3,
        Closed = 4,
        Faulted = 5,
        /// <summary>
        /// Determine that at least one peer is connected to this one
        /// This is the next state after opened
        /// </summary>
        Online = 6,
    }
}