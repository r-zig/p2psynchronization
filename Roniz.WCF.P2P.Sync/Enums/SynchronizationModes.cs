namespace Roniz.WCF.P2P.Sync.Enums
{
    /// <summary>
    /// Determine the mode that synchronization should work
    /// </summary>
    /// <remarks>
    /// The synchronization process try to be most efficient as possible 
    /// in the matter of messages each peer send and received in order to sync its state.
    /// for this reason its use the Online & Offline peerchannel events to determine when to begin the synchronization process.
    /// However there a problem can occur if mesh with more than 3 peers have multi peer groups that connected together but not connected between the groups.
    /// In this situation if for example there is a mesh with peers:
    /// A , B , C and D
    /// where A & B well connected and have online state (group 1)
    /// and C & D well connected and have online state (group 2)
    /// but peers from groups 1 and 2 does not connected , both groups peers will "think" that they are synchronized when they are not !
    /// a more complex solution required to sync between both groups also.
    /// Any solution will cause more message traffic in the mesh , so its up to the developer to decide which mode he want:
    /// Reliable which try to ensure that as soon as possible all the mesh will sync in the price of more messages,
    /// Or Economical which will based only on the online event but in the price that it cannot ensure full sync
    /// </remarks>
    public enum SynchronizationMode
    {
        /// <summary>
        /// Every peer process all neighbor peers messages , and will also try to obtain its state in some interval
        /// </summary>
        Reliable ,
        /// <summary>
        /// The peer that receive synchronization messages will process only the first one
        /// </summary>
        Economical ,
    }
}