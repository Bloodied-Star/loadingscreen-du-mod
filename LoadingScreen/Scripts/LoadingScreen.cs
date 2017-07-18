// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    
// Version:         1.5 for Daggerfall Unity 0.4.12

using System.IO;
using System.Collections;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using LoadingScreen.Plugins;

namespace LoadingScreen
{
    #region Structs

    struct LoadingType
    {
        public const int 
            
            Default = 0,
            Building = 1,
            Dungeon = 2;
    }

    public struct PluginsStatus
    {
        public bool
            loadingCounter,
            tips,
            questMessages,
            levelCounter;
    }
    
    #endregion

    /// <summary>
    /// Implement a loading screen in Daggerfall Unity.
    /// Use settings and image files from disk for customization.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        #region Fields

        // This mod
        private static Mod LoadingScreenMod;

        // The loading screen
        LoadingScreenWindow loadingScreen = new LoadingScreenWindow();

        // Is the game loading ?
        bool isLoading = false;

        // Fade from black to clear after the splash screen
        bool fadeFromBlack = false;

        #endregion

        #region Settings

        // Depth on GUI
        int guiDepth;

        // Loading screen settings
        bool dungeons;
        bool buildings;
        bool useLocation;
        bool useSeason;
        float MinimumWait;
		bool PressAnyKey;

        // Death screen
        bool showDeathScreen;
        bool DisableVideo;

        PluginsStatus pluginsStatus;

        #endregion

        #region Properties

        public LoadingScreenWindow LoadingScreenWindow
        {
            get { return loadingScreen; }
        }

        // GUI elements/Plugins
        public Plugins.LoadingLabel loadingLabel { get; set; }
        public DfTips dfTips { get; set; }
        public QuestsMessages questsMessages { get; set; }
        public LevelCounter levelCounter { get; set; }
        public DeathScreen deathScreen { get; set; }

        /// <summary>
        /// Load/transition finished but loading screen is still on GUI
        /// (MinimumWait and/or PressAnyKey).
        /// </summary>
        public bool IsWaiting
        {
            get { return fadeFromBlack; }
            internal set { fadeFromBlack = value; }
        }

        /// <summary>
        /// The sorting depth of the loading screen.
        /// </summary>
        public int GuiDepth
        {
            get { return guiDepth; }
            internal set { guiDepth = value; }
        }

        #endregion

        #region Unity

        /// <summary>
        /// Initialize mod.
        /// </summary>
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            LoadingScreenMod = initParams.Mod;

            // Add script to scene
            GameObject go = new GameObject("LoadingScreen");
            go.AddComponent<LoadingScreen>();

            // Set mod as Ready
            LoadingScreenMod.IsReady = true;
        }

        /// <summary>
        /// Start mod.
        /// </summary>
        void Start()
        {
            // Load settings and initialize plugins
            LoadSettings();

            // ModMessages
            LoadingScreenMod.MessageReceiver = MessageReceiver;

            // Suscribe to Loading
            SubscribeToLoading();
        }

        /// <summary>
        /// Draw on GUI.
        /// </summary>
        void OnGUI()
        {
            // Place on top of Daggerfall Unity panels.
            GUI.depth = guiDepth;

            // Draw on GUI.
            loadingScreen.Draw();
        }

        #endregion

        #region Loading Screen

        /// <summary>
        /// Start showing splash screen (LoadSave).
        /// </summary>
        /// <param name="saveData">Save game being loaded.</param>
        private void StartLoadingScreen(SaveData_v1 saveData)
        {
            loadingScreen.OnLoadingScreen(saveData);
            int loadingType = useLocation ? GetLoadingType(saveData.playerData.playerPosition) : LoadingType.Default;
            StartLoadingScreen(loadingType);
        }

        /// <summary>
        /// Start showing splash screen (enter/exit transition).
        /// </summary>
        private void StartLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            if (ShowOnTransitionType(args.TransitionType))
            {
                loadingScreen.OnLoadingScreen(args);
                int loadingType = useLocation ? GetLoadingType(args.TransitionType) : LoadingType.Default;
                StartLoadingScreen(loadingType);
            }
        }

        /// <summary>
        /// Start showing splash screen.
        /// </summary>
        private void StartLoadingScreen(int loadingType = LoadingType.Default)
        {
            LoadImage(loadingType);
            loadingScreen.Enabled = true;
            isLoading = true;
            StartCoroutine(ShowLoadingScreenOnGui());
        }

        /// <summary>
        /// Check end of loading.
        /// </summary>
        private void StopLoadingScreen<T>(T args)
        {
            isLoading = false;
        }

        /// <summary>
        /// Show loading screen and check progress.
        /// </summary>
        private IEnumerator ShowLoadingScreenOnGui()
        {
            // Time spent on the loading screen
            float timeCounter = 0;

            // Wait for end of loading
            while (isLoading)
            {
                // Update label
                //loadingLabel.UpdateLoadingLabel();
                loadingScreen.UpdateScreen();

                timeCounter += Time.deltaTime;
                yield return null;
            }

            // Pause the game and wait for MinimumWait
            // If this is not required by settings, MinimumWait is zero
            if (timeCounter < MinimumWait)
            {
                fadeFromBlack = true;
                GameManager.Instance.PauseGame(true, true);
                while (timeCounter < MinimumWait)
                {
                    timeCounter += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // Pause the game and show 'press-any-key' screen (if required by settings)
            if (PressAnyKey)
            {
                fadeFromBlack = true;

                // Display new string and pause the game
                //loadingLabel.SetEndLabel();
                loadingScreen.OnEndScreen();
                GameManager.Instance.PauseGame(true, true);

                // Wait for imput
                while (!Input.anyKey)
                    yield return null;
            }

            // Unpause the game (if paused)
            GameManager.Instance.PauseGame(false);

            // Terminate loading screen
            loadingScreen.Enabled = false;
            if (fadeFromBlack)
            {
                DaggerfallUI.Instance.FadeHUDFromBlack(0.5f);
                yield return new WaitForSeconds(0.5f);
                fadeFromBlack = false;
            }
        }

        #endregion

        #region Setup Methods

        /// <summary>
        /// Load settings from ModSettings and initialize plugins.
        /// </summary>
        private void LoadSettings()
        {
            ModSettings settings = new ModSettings(LoadingScreenMod);
            guiDepth = settings.GetInt(ModSettingsReader.internalSection, "GuiDepth");

            // Splash screen
            const string SplashScreenSection = "SplashScreen";
            dungeons = settings.GetBool(SplashScreenSection, "Dungeons");
            buildings = settings.GetBool(SplashScreenSection, "Buildings");
            useLocation = settings.GetBool(SplashScreenSection, "UseLocation");
            useSeason = settings.GetBool(SplashScreenSection, "UseSeason");
            MinimumWait = settings.GetFloat(SplashScreenSection, "ShowForMinimum");
            PressAnyKey = settings.GetBool(SplashScreenSection, "PressAnyKey");

            // Plugins
            var loadingScreenSetup = new LoadingScreenSetup(settings);
            pluginsStatus = loadingScreenSetup.GetSettingsPluginsStatus();

            // Label
            if (pluginsStatus.loadingCounter)
            {
                loadingLabel = loadingScreenSetup.InitLabel();
                loadingScreen.Components.Add(loadingLabel);
            }

            // Tips
            if (pluginsStatus.tips)
            {
                dfTips = loadingScreenSetup.InitTips(Path.Combine(LoadingScreenMod.DirPath, "Tips"), out pluginsStatus.tips);
                dfTips.Enabled = pluginsStatus.tips;
                loadingScreen.Components.Add(dfTips);
            }

            // Quest Message
            if (pluginsStatus.questMessages)
            {
                questsMessages = loadingScreenSetup.InitQuestMessages();
                loadingScreen.Components.Add(questsMessages);
            }

            // Level Counter
            if (pluginsStatus.levelCounter)
            {
                var levelCounterConstructor = loadingScreenSetup.InitLevelCounter();
                levelCounterConstructor.background = LoadingScreenMod.GetAsset<Texture2D>("BarBackground");
                levelCounterConstructor.progressBar = LoadingScreenMod.GetAsset<Texture2D>("BarProgress");
                levelCounter = new LevelCounter(levelCounterConstructor);
                loadingScreen.Components.Add(levelCounter);
            }

            // Death Screen
            const string DeathScreenSection = "DeathScreen";
            showDeathScreen = settings.GetBool(DeathScreenSection, "ShowDeathScreen");
            if (showDeathScreen)
            {
                DisableVideo = settings.GetBool(DeathScreenSection, "DisableVideo");
                deathScreen = new DeathScreen(this, DisableVideo);
            }
        }

        /// <summary>
        /// Subscribe to loading as per user settings.
        /// </summary>
        private void SubscribeToLoading()
        {
            SaveLoadManager.OnStartLoad += StartLoadingScreen;
            SaveLoadManager.OnLoad += StopLoadingScreen;

            if (dungeons || buildings)
            {
                PlayerEnterExit.OnPreTransition += StartLoadingScreen;

                if (dungeons)
                {
                    PlayerEnterExit.OnTransitionDungeonInterior += StopLoadingScreen;
                    PlayerEnterExit.OnTransitionDungeonExterior += StopLoadingScreen;
                }

                if (buildings)
                {
                    PlayerEnterExit.OnTransitionInterior += StopLoadingScreen;
                    PlayerEnterExit.OnTransitionExterior += StopLoadingScreen;
                }
            }

            if (showDeathScreen)
                PlayerDeath.OnPlayerDeath += deathScreen.ShowDeathScreen;

            Debug.Log("LoadingScreen: subscribed to loadings as per user settings.");
        }

        /// <summary>
        /// Unsubscribe from all loadings.
        /// </summary>
        private void UnsubscribeFromLoading()
        {
            SaveLoadManager.OnStartLoad -= StartLoadingScreen;
            SaveLoadManager.OnLoad -= StopLoadingScreen;

            if (dungeons || buildings)
            {
                PlayerEnterExit.OnPreTransition -= StartLoadingScreen;

                if (dungeons)
                {
                    PlayerEnterExit.OnTransitionDungeonInterior -= StopLoadingScreen;
                    PlayerEnterExit.OnTransitionDungeonExterior -= StopLoadingScreen;
                }

                if (buildings)
                {
                    PlayerEnterExit.OnTransitionInterior -= StopLoadingScreen;
                    PlayerEnterExit.OnTransitionExterior -= StopLoadingScreen;
                }
            }

            if (showDeathScreen)
                PlayerDeath.OnPlayerDeath -= deathScreen.ShowDeathScreen;

            Debug.Log("LoadingScreen: unsubscribed from all loadings.");
        }
        
        #endregion

        #region Private Methods

        private bool ShowOnTransitionType(PlayerEnterExit.TransitionType transition)
        {
            switch(transition)
            {
                case PlayerEnterExit.TransitionType.ToDungeonInterior:
                case PlayerEnterExit.TransitionType.ToDungeonExterior:
                    return dungeons;

                case PlayerEnterExit.TransitionType.ToBuildingInterior:
                case PlayerEnterExit.TransitionType.ToBuildingExterior:
                    return buildings;

                default:
                    return false;
            }
        }

        private int GetLoadingType(PlayerPositionData_v1 playerPosition)
        {
            if (playerPosition.insideBuilding)
                return LoadingType.Building;

            if (playerPosition.insideDungeon)
                return LoadingType.Dungeon;

            return LoadingType.Default;
        }

        private int GetLoadingType(PlayerEnterExit.TransitionType transitionType)
        {
            switch (transitionType)
            {
                case PlayerEnterExit.TransitionType.ToBuildingInterior:
                    return LoadingType.Building;

                case PlayerEnterExit.TransitionType.ToDungeonInterior:
                    return LoadingType.Dungeon;

                default:
                    return LoadingType.Default;
            }
        }

        /// <summary>
        /// Import image from disk.
        /// </summary>
        private void LoadImage(int loadingType)
        {
            // Get path
            string path = Path.Combine(LoadingScreenMod.DirPath, "Images");
            if (loadingType == LoadingType.Building)
                path = Path.Combine(path, "Building");
            else if (loadingType == LoadingType.Dungeon)
                path = Path.Combine(path, "Dungeon");
            else if (useSeason)
            {
                if (GameManager.Instance.PlayerGPS.ClimateSettings.ClimateType == DaggerfallConnect.DFLocation.ClimateBaseType.Desert)
                    path = Path.Combine(path, "Desert");
                else if (DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Winter)
                    path = Path.Combine(path, "Winter");
                else
                    path = Path.Combine(path, "Summer");
            }

            const string imageException = "\nPlease place one or more images in png format inside this folder to be used as a background\n" +
                "for the loading screen. As a fallback, a black image is being used.";

            try
            {
                string[] images = Directory.GetFiles(path, "*.png");
                if (images.Length != 0)
                {
                    // Get name
                    int index = Random.Range(0, images.Length);

                    // Import image
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(File.ReadAllBytes(Path.Combine(path, images[index])));
                    if (tex != null)
                    {
                        loadingScreen.background = tex;
                        return;
                    }

                    Debug.LogError("Loading Screen: Failed to import " + images[index] + " from " + path + imageException);
                }

                Debug.LogError("Loading Screen: Failed to get any image from " + path + imageException);

            }
            catch (DirectoryNotFoundException)
            {
                Debug.LogError("Loading Screen: directory " + path + " is missing." + imageException);
            }

            // Use a black texture as fallback
            loadingScreen.background = LoadingScreenMod.GetAsset<Texture2D>("defaultBackground");
        }

        #endregion

        #region Mod Messages

        /// <summary>
        /// Exchange messages with other mods.
        /// </summary>
        private void MessageReceiver(string message, object data = null, DFModMessageCallback callback = null)
        {
            try
            {
                switch (message)
                {
                    case "ShowLoadingScreen":
                        loadingScreen.Enabled = (bool)data;
                        break;
                    case "IsWaiting":
                        callback("IsWaiting", fadeFromBlack);
                        break;
                    case "GuiDepth":
                        callback("GuiDepth", guiDepth);
                        break;
                    default:
                        Debug.LogError("Loading Screen: Unknown message!\nmessage: " + message);
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Loading Screen: Failed to exchange messages\nException: " + e.Message + "\nMessage: " + message);
            } 
        }

        #endregion
    }
}