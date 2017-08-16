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
        readonly bool disableVideo;

        #region Public Methods

        /// <summary>
        /// Constructor for Death Screen.
        /// </summary>
        /// <param name="disableVideo">Replace video with screen, or show screen after video.</param>
        public DeathScreen (bool disableVideo)
        {
            this.disableVideo = disableVideo;
        }

        /// <summary>
        /// Start logic for Death Screen.
        /// </summary>
        public void StartLogic(object sender, System.EventArgs e)
        {
            LoadingScreen.Instance.StartCoroutine(Logic());
        }

        #endregion

        #region Private Methods

        private IEnumerator Logic()
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
                var player = DaggerfallUI.Instance.UserInterfaceManager.TopWindow as DaggerfallVidPlayerWindow;
                if (player == null)
                {
                    Debug.LogError("LoadingScreen: current top window is not a videoplayer, failed to get death video.");
                    yield break;
                }

                // Wait for end of video
                if (player.UseCustomVideo)
                {
                    CustomVideoPlayer video = player.CustomVideo;
                    while (video.Playing)
                        yield return null;
                }
                else
                {
                    DaggerfallVideo video = player.Video;
                    while (video.Playing)
                        yield return null;
                }
            }

            // Disable background audio
            AudioListener.pause = true;

            // Show death screen
            var ls = LoadingScreen.Instance.Window;
            ls.Panel.OnDeathScreen();
            ls.Panel.background = ImageReader.GetImageData("DIE_00I0.IMG").texture;
            ls.Enabled = true;

            // Wait for imput
            while (!Input.anyKey)
                yield return null;

            // Remove death screen
            ls.Enabled = false;
            ls.Panel.OnEndDeathScreen();
            AudioListener.pause = false; 
        }

        #endregion
    }
}
