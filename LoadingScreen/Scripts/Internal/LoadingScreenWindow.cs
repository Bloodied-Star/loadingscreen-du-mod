// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using System.IO;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using TransitionType = DaggerfallWorkshop.Game.PlayerEnterExit.TransitionType;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using LoadingScreen.Components;

namespace LoadingScreen
{
    /// <summary>
    /// Implements a Loading Screen window.
    /// </summary>
    public class LoadingScreenWindow
    {
        #region Fields

        protected readonly LoadingScreenPanel panel = new LoadingScreenPanel();

        // Logic
        DeathScreen deathScreen;
        LoadingLogic loadingLogic;

        // Settings
        bool dungeons, buildings;
        bool useLocation, useSeason;

        #endregion

        #region Properties

        /// <summary>
        /// Show or hide the loading screen.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Panel containing all components.
        /// </summary>
        public LoadingScreenPanel Panel
        {
            get { return panel; }
        }

        #endregion

        #region Public Methods

        public virtual void Setup()
        {
            var settings = LoadingScreen.Instance.LoadSettings();

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
            panel.AddValidComponent(MakeLabel(settings));
            panel.AddValidComponent(MakeTips(settings));
            panel.AddValidComponent(MakeQuestMessages(settings));
            panel.AddValidComponent(MakeLevelCounter(settings));
            panel.AddValidComponent(MakeEnemyPreview(settings));

            SubscribeToLoading();
        }

        public virtual void Draw()
        {
            if (Enabled)
                panel.Draw();
        }

        #endregion

        #region Loading Logic

        /// <summary>
        /// Start showing loading screen.
        /// </summary>
        private void StartLoadingScreen(int loadingType = LoadingType.Default)
        {
            panel.SetBackground(loadingType, useSeason);
            Enabled = true;
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

        #region Load Settings

        /// <summary>
        /// Gets Loading Counter with user settings.
        /// </summary>
        /// <param name="labelText">Text shown while loading.</param>
        /// <param name="labelTextFinish">Text shown after loading.</param>
        public LoadingLabel MakeLabel(ModSettings settings)
        {
            const string loadingLabelSection = "LoadingLabel";
            if (!settings.GetBool(loadingLabelSection, "Enable"))
                return null;

            string labelText = settings.GetString(loadingLabelSection, "LoadingText");
            bool isDynamic = settings.GetBool(loadingLabelSection, "IsDynamic");
            string labelTextFinish = settings.GetString(loadingLabelSection, "EndText");
            string deathLabel = settings.GetString(loadingLabelSection, "DeathText");

            Rect rect = GetRect(
                settings.GetTupleInt(loadingLabelSection, "Position"),
                new Tuple<int, int>(10, 3));

            return new LoadingLabel(rect, labelText, labelTextFinish, isDynamic, ".", deathLabel)
            {
                Font = settings.GetInt(loadingLabelSection, "Font"),
                FontSize = settings.GetInt(loadingLabelSection, "FontSize"),
                FontStyle = (FontStyle)settings.GetInt(loadingLabelSection, "FontStyle"),
                FontColor = settings.GetColor(loadingLabelSection, "FontColor")
            };
        }

        /// <summary>
        /// Gets Daggerfall Tips with user settings.
        /// </summary>
        public DfTips MakeTips(ModSettings settings)
        {
            const string tipsSection = "Tips";
            if (!settings.GetBool(tipsSection, "Enable"))
                return null;

            Rect rect = GetRect(
                settings.GetTupleInt(tipsSection, "Position"),
                settings.GetTupleInt(tipsSection, "Size"));
            string path = Path.Combine(LoadingScreen.Mod.DirPath, "Tips");
            string language = settings.GetString(tipsSection, "Language");

            return new DfTips(rect, path, language)
            {
                Font = settings.GetInt(tipsSection, "Font"),
                FontStyle = (FontStyle)settings.GetInt(tipsSection, "FontStyle"),
                FontColor = settings.GetColor(tipsSection, "FontColor")
            };
        }

        /// <summary>
        /// Gets Quests Messages with user settings.
        /// </summary>
        public QuestsMessages MakeQuestMessages(ModSettings settings)
        {
            const string questsSection = "Quests";
            if (!settings.GetBool(questsSection, "Enable"))
                return null;

            Rect rect = GetRect(
                settings.GetTupleInt(questsSection, "Position"),
                settings.GetTupleInt(questsSection, "Size"));

            return new QuestsMessages(rect)
            {
                Font = settings.GetInt(questsSection, "Font"),
                FontStyle = (FontStyle)settings.GetInt(questsSection, "FontStyle"),
                FontColor = settings.GetColor(questsSection, "FontColor")
            };
        }

        /// <summary>
        /// Gets Level Counter with user settings.
        /// </summary>
        public LevelCounter MakeLevelCounter(ModSettings settings)
        {
            const string levelProgressSection = "LevelProgress";
            if (!settings.GetBool(levelProgressSection, "Enable"))
                return null;

            Rect rect = GetRect(
                settings.GetTupleInt(levelProgressSection, "Position"),
                settings.GetTupleInt(levelProgressSection, "Size"));

            return new LevelCounter(rect)
            {
                LabelFormat = settings.GetString(levelProgressSection, "Text"),
                Font = settings.GetInt(levelProgressSection, "Font"),
                FontColor = settings.GetColor(levelProgressSection, "FontColor"),
            };
        }

        /// <summary>
        /// Gets Enemy Preview with user settings.
        /// </summary>
        public EnemyShowCase MakeEnemyPreview(ModSettings settings)
        {
            const string enemyPreviewSection = "EnemyPreview";
            if (!settings.GetBool(enemyPreviewSection, "Enable"))
                return null;

            Rect rect = GetRect(
                settings.GetTupleInt(enemyPreviewSection, "Position"),
                settings.GetTupleInt(enemyPreviewSection, "Size"));
            bool enableBackground = settings.GetBool(enemyPreviewSection, "Background");

            return new EnemyShowCase(rect, enableBackground)
            {
                Font = 8,
                FontColor = Color.yellow
            };
        }

        private static Rect GetRect(Tuple<int, int> position, Tuple<int, int> size)
        {
            return new Rect(
                position.First, position.Second,
                size.First, size.Second
                );
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