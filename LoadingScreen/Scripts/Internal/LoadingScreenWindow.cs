// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using TransitionType = DaggerfallWorkshop.Game.PlayerEnterExit.TransitionType;

namespace LoadingScreen
{
    public interface ILoadingScreenWindow
    {
        bool Enabled { get; set; }
        LoadingScreenPanel Panel { get; }
        void Setup();
        void Draw();
    }

    /// <summary>
    /// Implements a Loading Screen window.
    /// </summary>
    public class LoadingScreenWindow : ILoadingScreenWindow
    {
        protected bool enabled = false;
        protected readonly LoadingScreenPanel panel = new LoadingScreenPanel();

        // Logic
        DeathScreen deathScreen;
        LoadingLogic loadingLogic;

        // Settings
        bool dungeons, buildings;
        bool useLocation, useSeason;

        /// <summary>
        /// Show or hide the loading screen.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Panel containing all components.
        /// </summary>
        public LoadingScreenPanel Panel
        {
            get { return panel; }
        }

        #region Public Methods

        public virtual void Setup()
        {
            var settings = LoadingScreen.Instance.Settings;

            // General
            const string generalSection = "General";
            dungeons = settings.GetBool(generalSection, "Dungeons");
            buildings = settings.GetBool(generalSection, "Buildings");
            float minimumWait = settings.GetFloat(generalSection, "ShowForMinimum");
            bool pressAnyKey = settings.GetBool(generalSection, "PressAnyKey");
            loadingLogic = new LoadingLogic(minimumWait, pressAnyKey);

            // Background
            const string backgroundSection = "Background";
            useLocation = settings.GetBool(backgroundSection, "UseLocation");
            useSeason = settings.GetBool(backgroundSection, "UseSeason");

            // Death Screen
            const string deathScreenSection = "DeathScreen";
            if (settings.GetBool(deathScreenSection, "Enable"))
                deathScreen = new DeathScreen(settings.GetBool(deathScreenSection, "DisableVideo"));

            // Components
            var setup = new LoadingScreenSetup(settings);
            panel.Components.AddValid(setup.InitLabel());
            panel.Components.AddValid(setup.InitTips());
            panel.Components.AddValid(setup.InitQuestMessages());
            panel.Components.AddValid(setup.InitLevelCounter());

            SubscribeToLoading();
        }

        public virtual void Draw()
        {
            if (enabled)
                panel.Draw();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Start showing loading screen.
        /// </summary>
        private void StartLoadingScreen(int loadingType = LoadingType.Default)
        {
            panel.background = BackgroundImage.Get(loadingType, useSeason);
            enabled = true;
            loadingLogic.StartLogic();
        }

        private bool ShowOnTransitionType(TransitionType transition)
        {
            switch (transition)
            {
                case TransitionType.ToDungeonInterior:
                case TransitionType.ToDungeonExterior:
                    return dungeons;

                case TransitionType.ToBuildingInterior:
                case TransitionType.ToBuildingExterior:
                    return buildings;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Subscribe to loading as per user settings.
        /// </summary>
        private void SubscribeToLoading()
        {
            SaveLoadManager.OnStartLoad += SaveLoadManager_OnStartLoad;
            SaveLoadManager.OnLoad += SaveLoadManager_OnLoad;

            if (dungeons || buildings)
            {
                PlayerEnterExit.OnPreTransition += PlayerEnterExit_OnPreTransition;

                if (dungeons)
                {
                    PlayerEnterExit.OnTransitionDungeonInterior += PlayerEnterExit_OnTransition;
                    PlayerEnterExit.OnTransitionDungeonExterior += PlayerEnterExit_OnTransition;
                }

                if (buildings)
                {
                    PlayerEnterExit.OnTransitionInterior += PlayerEnterExit_OnTransition;
                    PlayerEnterExit.OnTransitionExterior += PlayerEnterExit_OnTransition;
                }
            }

            if (deathScreen != null)
                PlayerDeath.OnPlayerDeath += deathScreen.StartLogic;

            Debug.Log("LoadingScreen: subscribed to loadings as per user settings.");
        }

        /// <summary>
        /// Unsubscribe from all loadings.
        /// </summary>
        private void UnsubscribeFromLoading()
        {
            SaveLoadManager.OnStartLoad -= SaveLoadManager_OnStartLoad;
            SaveLoadManager.OnLoad -= SaveLoadManager_OnLoad;

            if (dungeons || buildings)
            {
                PlayerEnterExit.OnPreTransition -= PlayerEnterExit_OnPreTransition;

                if (dungeons)
                {
                    PlayerEnterExit.OnTransitionDungeonInterior -= PlayerEnterExit_OnTransition;
                    PlayerEnterExit.OnTransitionDungeonExterior -= PlayerEnterExit_OnTransition;
                }

                if (buildings)
                {
                    PlayerEnterExit.OnTransitionInterior -= PlayerEnterExit_OnTransition;
                    PlayerEnterExit.OnTransitionExterior -= PlayerEnterExit_OnTransition;
                }
            }

            if (deathScreen != null)
                PlayerDeath.OnPlayerDeath -= deathScreen.StartLogic;

            Debug.Log("LoadingScreen: unsubscribed from all loadings.");
        }

        #endregion

        #region Event Handlers

        // Start of save loading
        private void SaveLoadManager_OnStartLoad(SaveData_v1 saveData)
        {
            panel.OnLoadingScreen(saveData);
            int loadingType = useLocation ? LoadingType.Get(saveData.playerData.playerPosition) : LoadingType.Default;
            StartLoadingScreen(loadingType);
        }

        // End of save loading
        private void SaveLoadManager_OnLoad(SaveData_v1 saveData)
        {
            loadingLogic.StopLogic();
        }

        // Start of transition
        private void PlayerEnterExit_OnPreTransition(PlayerEnterExit.TransitionEventArgs args)
        {
            if (ShowOnTransitionType(args.TransitionType))
            {
                panel.OnLoadingScreen(args);
                int loadingType = useLocation ? LoadingType.Get(args.TransitionType) : LoadingType.Default;
                StartLoadingScreen(loadingType);
            }
        }

        // End of transition
        private void PlayerEnterExit_OnTransition(PlayerEnterExit.TransitionEventArgs args)
        {
            loadingLogic.StopLogic();
        }

        #endregion
    }
}