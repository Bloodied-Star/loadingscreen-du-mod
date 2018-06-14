// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using System.Collections;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using TransitionType = DaggerfallWorkshop.Game.PlayerEnterExit.TransitionType;

namespace LoadingScreen
{
    /// <summary>
    /// Implement a loading screen in Daggerfall Unity.
    /// Use settings and image files from disk for customization.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        #region Fields

        static LoadingScreen instance;

        bool dungeons;
        bool buildings;
        float minimumWait;
        bool pressAnyKey;
        bool deathScreen;
        bool disableVideo;

        LoadingScreenWindow window;
        int guiDepth;

        bool isEnabled = false;
        bool isLoading = false;
        bool fadeFromBlack = false;

        #endregion

        #region Properties

        /// <summary>
        /// Loading Screen mod.
        /// </summary>
        public static Mod Mod { get; private set; }

        /// <summary>
        /// Loading Screen instance.
        /// </summary>
        public static LoadingScreen Instance
        {
            get { return instance ?? (instance = FindObjectOfType<LoadingScreen>()); }
        }

        #endregion

        #region Unity

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            Mod = initParams.Mod;

            // Add script to scene
            GameObject go = new GameObject("LoadingScreen");
            instance = go.AddComponent<LoadingScreen>();

            // Set mod as Ready
            Mod.IsReady = true;
        }

        void Awake()
        {
            SetupMod();
            Mod.MessageReceiver = MessageReceiver;
            LoadingScreenConsoleCommands.RegisterCommands();
        }

        void OnGUI()
        {
            // Place on top of Daggerfall Unity panels.
            GUI.depth = guiDepth;

            // Draw on GUI.
            window.Draw();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            if (Mod == null)
                return base.ToString();

            return string.Format("{0} v.{1}", Mod.Title, Mod.ModInfo.ModVersion);
        }

        public void Toggle()
        {
            Toggle(!isEnabled);
        }

        public void Toggle(bool toggle)
        {
            if (isEnabled == toggle)
                return;

            if (toggle)
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

                if (deathScreen)
                    PlayerDeath.OnPlayerDeath += PlayerDeath_OnPlayerDeath;

                LogMessage("Subscribed to loadings as per user settings.");
            }
            else
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

                if (deathScreen)
                    PlayerDeath.OnPlayerDeath -= PlayerDeath_OnPlayerDeath;

                LogMessage("Unsubscribed from all loadings.");
            }

            isEnabled = toggle;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Print a log message.
        /// </summary>
        internal void LogMessage(string message)
        {
            Debug.LogFormat("{0}: {1}", this, message);
        }

        /// <summary>
        /// Print a formatted error to log.
        /// </summary>
        internal void LogError(string format, params object[] args)
        {
            Debug.LogErrorFormat("{0}: {1}", this, string.Format(format, args));
        }

        /// <summary>
        /// Simulate a loading screen.
        /// </summary>
        /// <param name="seconds">Time in seconds for a single loading screen.</param>
        /// <param name="times">Number of loading screens.</param>
        internal void Simulate(float seconds, int times = 1)
        {
            StartCoroutine(DoSimulation(seconds, times));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load settings and init window.
        /// </summary>
        private void SetupMod()
        {
            var settings = Mod.GetSettings();
            guiDepth = settings.GetValue<int>("UiSettings", "GuiDepth");

            // General
            const string generalSection = "General";
            dungeons = settings.GetBool(generalSection, "Dungeons");
            buildings = settings.GetBool(generalSection, "Buildings");
            minimumWait = settings.GetFloat(generalSection, "ShowForMinimum");
            pressAnyKey = settings.GetBool(generalSection, "PressAnyKey");

            // Death Screen
            const string deathScreenSection = "DeathScreen";
            deathScreen = settings.GetBool(deathScreenSection, "Enable");
            disableVideo = settings.GetBool(deathScreenSection, "DisableVideo");

            window = new LoadingScreenWindow(settings);
            Toggle(true);
        }

        /// <summary>
        /// Start showing loading screen.
        /// </summary>
        private void StartLoadingScreen()
        {
            isLoading = window.Enabled = true;
            StartCoroutine(DoLoadingScreen());
        }

        /// <summary>
        /// Manages loading screen during loading times.
        /// </summary>
        private IEnumerator DoLoadingScreen()
        {
            // Time spent on the loading screen
            float timeCounter = 0;

            // Wait for end of loading
            while (isLoading)
            {
                window.Panel.UpdateScreen();
                timeCounter += Time.deltaTime;
                yield return null;
            }

            // Pause the game and wait for MinimumWait
            // If this is not required by settings, MinimumWait is zero
            if (timeCounter < minimumWait)
            {
                fadeFromBlack = true;
                GameManager.Instance.PauseGame(true, true);
                while (timeCounter < minimumWait)
                {
                    timeCounter += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // Pause the game and show 'press-any-key' screen (if required by settings)
            if (pressAnyKey)
            {
                fadeFromBlack = true;
                window.Panel.OnEndScreen();
                GameManager.Instance.PauseGame(true, true);

                // Wait for imput
                while (!Input.anyKey)
                    yield return null;
            }

            // Unpause the game (if paused)
            GameManager.Instance.PauseGame(false);

            fadeFromBlack = false;

            // Terminate loading screen
            window.Enabled = false;
            if (fadeFromBlack)
            {
                DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack(0.5f);
                yield return new WaitForSeconds(0.5f);
                fadeFromBlack = false;
            }
        }

        /// <summary>
        /// Show a death screen on user death. 
        /// </summary>
        /// <remarks>
        /// This was originally present in Daggerfall protoype demo, but was replaced by a video in the final version of the game.
        /// The screen will be shown after the video or instead of, according to user settings.
        /// The image is taken from the game assets as Bethesda left it there in the released game.
        /// </remarks>
        private IEnumerator DoDeathScreen()
        {
            // Wait for user death
            var playerDeath = GameManager.Instance.PlayerDeath;
            while (playerDeath.DeathInProgress)
                yield return null;

            // Death video
            if (!disableVideo)
            {
                // Let the video starts
                yield return new WaitForSecondsRealtime(1);

                // Get video
                var topWindow = DaggerfallUI.Instance.UserInterfaceManager.TopWindow;
                var player = topWindow as DaggerfallVidPlayerWindow;
                if (player == null)
                {
                    LogError("Current top window is not a videoplayer ({0})", topWindow);
                    yield break;
                }

                // Wait for end of video
                while (player.IsPlaying)
                    yield return null;
            }

            // Disable background audio
            AudioListener.pause = true;

            // Show death screen
            window.Panel.OnDeathScreen();
            window.Enabled = true;

            // Wait for imput
            while (!Input.anyKey)
                yield return null;

            // Remove death screen
            window.Enabled = false;
            window.Panel.OnEndDeathScreen();
            AudioListener.pause = false;
        }

        /// <summary>
        /// Simulate a loading screen.
        /// </summary>
        /// <param name="seconds">Time in seconds for a single loading screen.</param>
        /// <param name="times">Number of loading screens.</param>
        private IEnumerator DoSimulation(float seconds, int times)
        {
            GameManager.Instance.PauseGame(true, true);

            for (int i = 0; i < times; i++)
            {
                window.Panel.OnLoadingScreen(new PlayerEnterExit.TransitionEventArgs());
                window.Enabled = true;

                float time = seconds;
                while ((time -= Time.unscaledDeltaTime) > 0)
                {
                    window.Panel.UpdateScreen();
                    yield return null;
                }

                if (pressAnyKey)
                {
                    window.Panel.OnEndScreen();
                    while (!Input.anyKey)
                        yield return null;
                }

                window.Enabled = false;
            }

            GameManager.Instance.PauseGame(false);
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
        /// Exchange messages with other mods.
        /// </summary>
        private void MessageReceiver(string message, object data = null, DFModMessageCallback callback = null)
        {
            try
            {
                switch (message)
                {
                    case "ShowLoadingScreen":
                        window.Enabled = (bool)data;
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

        #region Event Handlers

        // Start of save loading
        private void SaveLoadManager_OnStartLoad(SaveData_v1 saveData)
        {
            window.Panel.OnLoadingScreen(saveData);
            StartLoadingScreen();
        }

        // End of save loading
        private void SaveLoadManager_OnLoad(SaveData_v1 saveData)
        {
            isLoading = false;
        }

        // Start of transition
        private void PlayerEnterExit_OnPreTransition(PlayerEnterExit.TransitionEventArgs args)
        {
            if (ShowOnTransitionType(args.TransitionType))
            {
                window.Panel.OnLoadingScreen(args);
                StartLoadingScreen();
            }
        }

        // End of transition
        private void PlayerEnterExit_OnTransition(PlayerEnterExit.TransitionEventArgs args)
        {
            isLoading = false;
        }

        private void PlayerDeath_OnPlayerDeath(object sender, System.EventArgs e)
        {
            StartCoroutine(DoDeathScreen());
        }

        #endregion
    }
}