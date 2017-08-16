using System.Collections;
using UnityEngine;
using DaggerfallWorkshop.Game;

namespace LoadingScreen
{
    public class LoadingLogic
    {
        readonly float minimumWait;
        readonly bool pressAnyKey;

        bool isLoading = false;
        bool fadeFromBlack = false;

        public LoadingLogic(float minimumWait, bool pressAnyKey)
        {
            this.minimumWait = minimumWait;
            this.pressAnyKey = pressAnyKey;
        }

        public void StartLogic()
        {
            isLoading = true;
            LoadingScreen.Instance.StartCoroutine(Logic());
        }

        public void StopLogic()
        {
            isLoading = false;
        }

        private IEnumerator Logic()
        {
            var ls = LoadingScreen.Instance.Window;

            // Time spent on the loading screen
            float timeCounter = 0;

            // Wait for end of loading
            while (isLoading)
            {
                ls.Panel.UpdateScreen();
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
                ls.Panel.OnEndScreen();
                GameManager.Instance.PauseGame(true, true);

                // Wait for imput
                while (!Input.anyKey)
                    yield return null;
            }

            // Unpause the game (if paused)
            GameManager.Instance.PauseGame(false);

            fadeFromBlack = false;

            // Terminate loading screen
            ls.Enabled = false;
            if (fadeFromBlack)
            {
                DaggerfallUI.Instance.FadeHUDFromBlack(0.5f);
                yield return new WaitForSeconds(0.5f);
                fadeFromBlack = false;
            }
        }
    }
}
