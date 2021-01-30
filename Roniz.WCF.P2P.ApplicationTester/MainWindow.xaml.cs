using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Roniz.Diagnostics.Logging;
using Roniz.WCF.P2P.Sync;
using Roniz.WCF.P2P.ApplicationTester.Messages;
using Roniz.WCF.P2P.Sync.Enums;

namespace Roniz.WCF.P2P.ApplicationTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region properties
        public SynchronizationStateManager SyncManager { get; set;}
        public MyUserUpdateState UserState { get; set;}
        public int ProcessId { get; set; }
        public MySynchronizationBusinessLogic MySynchronizationBusinessLogic { get; set; }
        #endregion
        
        #region members
        private Guid id;
        #endregion

        #region constructores
        public MainWindow()
        {
            InitializeComponent();
            ProcessId = Process.GetCurrentProcess().Id;
            UserState = new MyUserUpdateState
            {
                IsOwnPeerData = true,
                Name = string.Format("Peer {0}-{1}", Environment.MachineName,ProcessId),
                Data = string.Format("{0}-{1}", ProcessId,Environment.TickCount)
            };
            
            id = Guid.NewGuid();

            MySynchronizationBusinessLogic = new MySynchronizationBusinessLogic(id, UserState);
            CreateSyncManager();
            DataContext = this;
        }
        #endregion

        #region methods
        void SyncManagerPeerOnline(object sender, EventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug( "SyncManagerPeerOnline...");

            Dispatcher.Invoke((Action)(() =>
            {
                if (menuAutoPublish.IsChecked)
                {
                    UpdateOthersWithOwnData();
                }
            }));
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            SyncManager.UseGenericResolver = menuUseGenericResolver.IsChecked;
            SyncManager.Open();
        }

        private void CreateSyncManager()
        {
            if (SyncManager == null)
            {
                if (rbReliable.IsChecked.GetValueOrDefault())
                {
                    //SynchronizationMode.Reliable with default refresh values
                    SyncManager = new SynchronizationStateManager(MySynchronizationBusinessLogic);
                }
                else
                {
                    //SynchronizationMode.Economical
                    SyncManager = new SynchronizationStateManager(MySynchronizationBusinessLogic,Defaults.SyncDefaultEndpointConfigurationName,SynchronizationMode.Economical);
                }
            }

            SyncManager.PeerOnline += SyncManagerPeerOnline;
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            SyncManager.Close();
        }

        private void SendUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateOthersWithOwnData();
        }

        private void UpdateOthersWithOwnData()
        {
            var updateState = new MyStateContainer
            {
                StateDictionary =
                    new Dictionary<Guid, MyUserUpdateState>(1)
            };
            updateState.StateDictionary.Add(id, UserState);
            SyncManager.Update(updateState);

            //example to update neighbors only
            //syncManager.Update(updateState,true);

            //example to update by specified hop count
            //syncManager.Update(updateState, 111);
        }

        #endregion
    }
}
