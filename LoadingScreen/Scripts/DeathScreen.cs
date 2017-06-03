// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors: 

using System.Collections;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using LoadingScreen.Plugins;

namespace LoadingScreen
{
    /// <summary>
    /// Show a death screen on user death. 
    /// This was originally present in Daggerfall protoype demo, but was replaced by a video in the final version of the game.
    /// The screen will be shown after the video or instead of, according to user settings.
    /// The image is taken from the game assets as Bethesda left it there in the released game.
    /// </summary>
    public class DeathScreen
    {
        // Fields
        LoadingScreen loadingScreen;
        bool disableVideo;
        bool tips;
        string labelText;
        string labelTextFinish;

        #region Public Methods

        /// <summary>
        /// Constructor for Death Screen.
        /// </summary>
        /// <param name="loadingScreen">Main MonoBehaviour.</param>
        /// <param name="disableVideo">Replace video with screen, or show screen after video.</param>
        /// <param name="tips">Show tips on death screen?</param>
        /// <param name="labelText">Label which will be restored after death screen.</param>
        /// <param name="labelTextFinish">Label shown during loading screen.</param>
        public DeathScreen (LoadingScreen loadingScreen, bool disableVideo, bool tips, string labelText, string labelTextFinish)
        {
            this.loadingScreen = loadingScreen;
            this.disableVideo = disableVideo;
            this.tips = tips;
            this.labelText = labelText;
            this.labelTextFinish = labelTextFinish;
        }

        /// <summary>
        /// Show death screen.
        /// </summary>
        public void ShowDeathScreen(object sender, System.EventArgs e)
        {
            loadingScreen.StartCoroutine(ShowDeathScreenOnGUI());
        }

        #endregion

        #region Private Methods

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
            if (!disableVideo)
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

            // Disable unnecessary plugins
            PluginsStatus pluginsStatus = loadingScreen.SettingsPluginsStatus;
            pluginsStatus.questMessages = false;
            pluginsStatus.levelCounter = false;
            loadingScreen.SetPluginStatus(pluginsStatus);

            // Show death screen
            loadingScreen.screenTexture = ImageReader.GetImageData("DIE_00I0.IMG").texture;
            loadingScreen.LoadingLabel = labelTextFinish;
            if (tips)
                loadingScreen.tipLabel = DfTips.GetTip();
            loadingScreen.ShowLoadingScreen = true;

            // Wait for imput
            while (!Input.anyKey)
                yield return null;

            // Remove death screen
            loadingScreen.ShowLoadingScreen = false;
            loadingScreen.LoadingLabel = labelText;
            loadingScreen.RestorePluginsStatus();
            AudioListener.pause = false;
        }

        #endregion
    }
}
