// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    
// Version:         1.4 for Daggerfall Unity 0.4.12

using System.IO;
using System.Collections;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;

namespace LoadingScreen
{
    /// <summary>
    /// Implement a loading screen in Daggerfall Unity.
    /// Use settings and image files from disk for customization.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        #region Fields

        // This mod
        public static Mod LoadingScreenMod;

        // Draw the splash screen on GUI
        static bool DrawLoadingScreen = false;

        bool isLoading = false;

        // Fade from black to clear after the splash screen
        bool fadeFromBlack = false;

        // Gui elements
        static Texture2D screenTexture;

        string LoadingLabel;
        static Rect LoadingCounterRect;
        static GUIStyle style;

        string tipLabel;
        static Rect tipRect;
        static GUIStyle tipStyle;

        #endregion

        #region Settings

        // Internal
        static int guiDepth;

        // Splash screen
        static bool dungeons;
        static bool buildings;
        static bool UseSeason;
        static float MinimumWait;
		static bool PressAnyKey;

        // Label
        static bool LoadingCounter;
        static int LoadingfontSize;
        static Color GuiColor;
        static string LabelText;
        static string LabelTextFinish;

        // Death screen
        static bool showDeathScreen;
        static bool DisableVideo;

        // Experimental
        static bool tips;
        static int tipsFontSize;
        static Color tipsFontColor;

        #endregion

        #region Properties

        /// <summary>
        /// Is the loading screen on GUI.
        /// </summary>
        public bool ShowLoadingScreen
        {
            get { return DrawLoadingScreen; }
            set { DrawLoadingScreen = value; }
        }

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
            // Load settings
            LoadSettings();

            // Initialize Gui Elements
            InitGUiElements();

            // ModMessages
            LoadingScreenMod.MessageReciver = MessageReceiver;

            // Suscribe to Loading
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

            // Suscribe to Death
            if (showDeathScreen)
                PlayerDeath.OnPlayerDeath += ShowDeathScreen;
        }

        /// <summary>
        /// Draw on GUI.
        /// </summary>
        void OnGUI()
        {
            // Place on top of Daggerfall Unity panels.
            GUI.depth = guiDepth;

            // Draw on GUI.
            if (DrawLoadingScreen)
            {
                // Background image
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), screenTexture, ScaleMode.StretchToFill);

                // Loading label
                if (LoadingCounter)
                    GUI.Box(LoadingCounterRect, LoadingLabel, style);

                // Tips
                if (tips)
                    GUI.Box(tipRect, tipLabel, tipStyle);
            }
        }

        #endregion

        #region Loading Screen

        /// <summary>
        /// Start showing splash screen (LoadSave).
        /// </summary>
        /// <param name="saveData">Save game being loaded.</param>
        private void StartLoadingScreen(SaveData_v1 saveData)
        {
            LoadImage();
            if (tips)
                tipLabel = DfTips.GetTip(saveData);
            DrawLoadingScreen = true;
            isLoading = true;
            StartCoroutine(ShowLoadingScreenOnGui());
        }

        /// <summary>
        /// Start showing splash screen (enter/exit transition).
        /// </summary>
        private void StartLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            if (((dungeons) && (args.TransitionType == PlayerEnterExit.TransitionType.ToDungeonInterior 
                || args.TransitionType == PlayerEnterExit.TransitionType.ToDungeonExterior)) ||
                    ((buildings) && (args.TransitionType == PlayerEnterExit.TransitionType.ToBuildingInterior 
                    || args.TransitionType == PlayerEnterExit.TransitionType.ToBuildingExterior)))
            {
                LoadImage();
                if (tips)
                    tipLabel = DfTips.GetTip(args.TransitionType);
                DrawLoadingScreen = true;
                isLoading = true;
                StartCoroutine(ShowLoadingScreenOnGui());
            }
        }

        /// <summary>
        /// Check end of loading.
        /// </summary>
        /// <param name="saveData">Save game loaded.</param>
        private void StopLoadingScreen(SaveData_v1 saveData)
        {
            isLoading = false;
        }

        /// <summary>
        /// Check end of loading.
        /// </summary>
        private void StopLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
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
                LoadingLabel += ".";

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
                LoadingLabel = LabelTextFinish;
                GameManager.Instance.PauseGame(true, true);

                // Wait for imput
                while (!Input.anyKey)
                    yield return null;
            }

            // Unpause the game (if paused)
            GameManager.Instance.PauseGame(false);

            // Terminate loading screen
            DrawLoadingScreen = false;
            LoadingLabel = LabelText;
            if (fadeFromBlack)
            {
                DaggerfallUI.Instance.FadeHUDFromBlack(0.5f);
                yield return new WaitForSeconds(0.5f);
                fadeFromBlack = false;
            }
        }

        #endregion

        #region Death Screen

        /// <summary>
        /// Show death screen.
        /// </summary>
        private void ShowDeathScreen(object sender, System.EventArgs e)
        {
            StartCoroutine(ShowDeathScreenOnGUI());
        }

        /// <summary>
        /// Show death screen.
        /// </summary>
        private IEnumerator ShowDeathScreenOnGUI()
        {
            // Wait for user death
            var playerDeath = GameManager.Instance.PlayerDeath;
            while (playerDeath.DeathInProgress)
                yield return null;

            // Death video
            if (!DisableVideo)
            {
                // Let the video starts
                float timeCounter = 0;
                while (timeCounter < 1)
                {
                    timeCounter += Time.unscaledDeltaTime;
                    yield return null;
                }

                // Get video
                DaggerfallVidPlayerWindow vidWindow;
                try
                {
                    vidWindow = (DaggerfallVidPlayerWindow)DaggerfallUI.Instance.UserInterfaceManager.TopWindow;
                }
                catch
                {
                    Debug.LogError("Loading Screen: current top window is not a videoplayer, failed to get death video.\n" +
                        "Expecting: DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallVidPlayerWindow\nGot: " +
                        DaggerfallUI.Instance.UserInterfaceManager.TopWindow.ToString());
                    yield break;
                }

                // Wait for end of video
                if (vidWindow.CustomVideo)
                {
                    CustomVideoPlayer video = vidWindow.CustomVideo;
                    while (video.Playing)
                        yield return null;
                }
                else
                {
                    DaggerfallVideo video = vidWindow.Video;
                    while (video.Playing)
                        yield return null;
                }
            }

            // Disable background audio
            AudioListener.pause = true;

            // Show death screen
            screenTexture = ImageReader.GetImageData("DIE_00I0.IMG").texture;
            LoadingLabel = LabelTextFinish;
            tipLabel = "";
            DrawLoadingScreen = true;

            // Wait for imput
            while (!Input.anyKey)
                yield return null;

            // Remove death screen
            DrawLoadingScreen = false;
            LoadingLabel = LabelText;
            AudioListener.pause = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load settings from ModSettings.
        /// </summary>
        private void LoadSettings()
        {
            ModSettings settings = new ModSettings(LoadingScreenMod);
            guiDepth = settings.GetInt(ModSettingsReader.internalSection, "GuiDepth");

            // Splash screen
            const string SplashScreenSection = "SplashScreen";
            dungeons = settings.GetBool(SplashScreenSection, "Dungeons");
            buildings = settings.GetBool(SplashScreenSection, "Buildings");
            UseSeason = settings.GetBool(SplashScreenSection, "UseSeason");
            MinimumWait = settings.GetFloat(SplashScreenSection, "ShowForMinimum");
            PressAnyKey = settings.GetBool(SplashScreenSection, "PressAnyKey");

            // Loading label
            const string LoadingLabelSection = "LoadingLabel";
            LoadingCounter = settings.GetBool(LoadingLabelSection, "LoadingCounter");
            LoadingfontSize = settings.GetInt(LoadingLabelSection, "LoadingfontSize");
            GuiColor = settings.GetColor(LoadingLabelSection, "GuiColor");
            LabelText = settings.GetString(LoadingLabelSection, "LabelText");
            LabelTextFinish = settings.GetString(LoadingLabelSection, "LabelTextFinish");

            // Death Screen
            const string DeathScreenSection = "DeathScreen";
            showDeathScreen = settings.GetBool(DeathScreenSection, "ShowDeathScreen");
            DisableVideo = settings.GetBool(DeathScreenSection, "DisableVideo");

            // Tips
            const string Experimental =  "Experimental";
            tips = settings.GetBool(Experimental, "Tips");
            tipsFontSize = settings.GetInt(Experimental, "TipsSize");
            tipsFontColor = settings.GetColor(Experimental, "TipsColor");
        }

        /// <summary>
        /// Initialize GUI elements.
        /// </summary>
        private void InitGUiElements()
        {
            // Label
            LoadingLabel = LabelText;

            // Label rect
            LoadingCounterRect = new Rect(Screen.width - 100, Screen.height - 30, 50, 10);

            // Label style
            style = new GUIStyle();
            style.alignment = TextAnchor.LowerRight;
            style.fontSize = LoadingfontSize;
            style.normal.textColor = GuiColor;

            // Tips rect
            const int tipX = 180, tipY = 100;
            tipRect = new Rect(tipX, Screen.height - tipY, 100, 20);

            // Tips style
            tipStyle = new GUIStyle();
            tipStyle.alignment = TextAnchor.UpperLeft;
            tipStyle.fontSize = tipsFontSize;
            tipStyle.normal.textColor = tipsFontColor;
        }

        /// <summary>
        /// Import image from disk.
        /// </summary>
        private void LoadImage()
        {
            // Get path
            string path = Path.Combine(TextureReplacement.TexturesPath, "LoadingScreens");
            if (UseSeason)
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
                    screenTexture = new Texture2D(2, 2);
                    screenTexture.LoadImage(File.ReadAllBytes(Path.Combine(path, images[index])));
                    if (screenTexture != null)
                        return;

                    Debug.LogError("Loading Screen: Failed to import " + images[index] + " from " + path + imageException);
                }

                Debug.LogError("Loading Screen: Failed to get any image from " + path + imageException);

            }
            catch (DirectoryNotFoundException)
            {
                Debug.LogError("Loading Screen: directory " + path + " is missing." + imageException);
            }

            // Use a black texture as fallback
            screenTexture = LoadingScreenMod.GetAsset<Texture2D>("defaultBackground");
        }

        #endregion

        #region Mod Messages

        // Uses DFModMessageReceiver delegate to share info between mods.
        private void MessageReceiver(string message, object data = null, DFModMessageCallback callback = null)
        {
            try
            {
                switch (message)
                {
                    case "ShowLoadingScreen":
                        DrawLoadingScreen = (bool)data;
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
