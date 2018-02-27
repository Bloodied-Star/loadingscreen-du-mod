// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace LoadingScreen
{
    /// <summary>
    /// Implement a loading screen in Daggerfall Unity.
    /// Use settings and image files from disk for customization.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        #region Fields

        static Mod mod;
        static LoadingScreen instance;
        readonly LoadingScreenWindow loadingScreen = new LoadingScreenWindow();

        int guiDepth;

        #endregion

        #region Properties

        /// <summary>
        /// Loading Screen mod.
        /// </summary>
        public static Mod Mod
        {
            get { return mod; }
        }

        /// <summary>
        /// Loading Screen instance.
        /// </summary>
        public static LoadingScreen Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<LoadingScreen>();
                return instance;
            }
        }

        /// <summary>
        /// Loading Screen window.
        /// </summary>
        public LoadingScreenWindow Window
        {
            get { return loadingScreen; }
        }

        /// <summary>
        /// Load and get mod settings.
        /// </summary>
        public ModSettings Settings
        {
            get { return LoadSettings(); }
        }

        /// <summary>
        /// The sorting depth of the loading screen.
        /// </summary>
        public int GuiDepth { get { return guiDepth; } }

        #endregion

        #region Unity

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            mod = initParams.Mod;

            // Add script to scene
            GameObject go = new GameObject("LoadingScreen");
            instance = go.AddComponent<LoadingScreen>();

            // Set mod as Ready
            mod.IsReady = true;
        }

        void Awake()
        {
            loadingScreen.Setup();
            mod.MessageReceiver = MessageReceiver;
        }

        void OnGUI()
        {
            // Place on top of Daggerfall Unity panels.
            GUI.depth = guiDepth;

            // Draw on GUI.
            loadingScreen.Draw();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load settings from ModSettings.
        /// </summary>
        private ModSettings LoadSettings()
        {
            ModSettings settings = new ModSettings(mod);
            guiDepth = settings.GetValue<int>("UiSettings", "GuiDepth");
            return settings;
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
                        loadingScreen.Enabled = (bool)data;
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