// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

// #define DRAW_RECT_BACKGROUND

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using LoadingScreen.Plugins;

namespace LoadingScreen
{
    public interface ILoadingScreenPanel
    {
        List<LoadingScreenComponent> Components { get; }
        Texture2D Background { get; set; }
        void Draw();
        void OnLoadingScreen(SaveData_v1 saveData);
        void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args);
        void UpdateScreen();
        void OnEndScreen();
        void OnDeathScreen();
        void OnEndDeathScreen();
    }

    /// <summary>
    /// Manages and draws components of Loading Screen.
    /// </summary>
    public class LoadingScreenPanel : ILoadingScreenPanel
    {
        #region Fields & Properties

        readonly static string imagesPath = Path.Combine(LoadingScreen.Mod.DirPath, "Images");

        readonly List<LoadingScreenComponent> components = new List<LoadingScreenComponent>();
        Rect rect = new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height));

        /// <summary>
        /// Background image for the loading screen.
        /// </summary>
        public Texture2D Background { get; set; }

        /// <summary>
        /// Components of the loading screen.
        /// </summary>
        public List<LoadingScreenComponent> Components
        {
            get { return components; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Draws the loading screen on GUI.
        /// </summary>
        public virtual void Draw()
        {
            GUI.DrawTexture(rect, Background, ScaleMode.StretchToFill);

            foreach (LoadingScreenComponent component in components)
            {
                if (component.Enabled)
                {
#if DRAW_RECT_BACKGROUND
                    GUI.DrawTexture(component.Rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
                    component.FontColor = Color.green;
#endif
                    component.Draw();

                }
            }
        }

        /// <summary>
        /// Called when the user load a save.
        /// </summary>
        /// <param name="saveData">Save being loaded.</param>
        public virtual void OnLoadingScreen(SaveData_v1 saveData)
        {
            RefreshRect();

            foreach (LoadingScreenComponent component in components)
            {
                if (component.Enabled)
                    component.OnLoadingScreen(saveData);
            }
        }

        /// <summary>
        /// Called during transition (entering/exiting building).
        /// </summary>
        /// <param name="args">Transition parameters.</param>
        public virtual void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            RefreshRect();

            foreach (LoadingScreenComponent component in components)
            {
                if (component.Enabled)
                    component.OnLoadingScreen(args);
            }
        }

        /// <summary>
        /// Called once per frame during loading.
        /// </summary>
        public virtual void UpdateScreen()
        {
            foreach (LoadingScreenComponent component in components)
            {
                if (component.Enabled)
                    component.UpdateScreen();
            }
        }

        /// <summary>
        /// Called on press-any-key screen.
        /// </summary>
        public virtual void OnEndScreen()
        {
            foreach (LoadingScreenComponent component in components)
            {
                if (component.Enabled)
                    component.OnEndScreen();
            }
        }

        /// <summary>
        /// Called on player death.
        /// </summary>
        public virtual void OnDeathScreen()
        {
            RefreshRect();

            foreach (LoadingScreenComponent component in components)
            {
                if (component.Enabled)
                    component.OnDeathScreen();
            }
        }

        /// <summary>
        /// Unhide components that don't support death screen.
        /// </summary>
        public virtual void OnEndDeathScreen()
        {
            foreach (LoadingScreenComponent component in components)
                component.Enabled = true;
        }


        public void AddValidComponent(LoadingScreenComponent component)
        {
            if (component)
                components.Add(component);
        }

        public void SetBackground(int loadingType, bool useSeason)
        {
            string folder;
            switch (loadingType)
            {
                case LoadingType.Building:
                    folder = "Building";
                    break;
                case LoadingType.Dungeon:
                    folder = "Dungeon";
                    break;
                default:
                    if (useSeason)
                    {
                        if (GameManager.Instance.PlayerGPS.ClimateSettings.ClimateType == DaggerfallConnect.DFLocation.ClimateBaseType.Desert)
                            folder = "Desert";
                        else if (DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Winter)
                            folder = "Winter";
                        else
                            folder = "Summer";
                        break;
                    }
                    folder = string.Empty;
                    break;
            }

            Background = LoadTexture(Path.Combine(imagesPath, folder));
        }

        #endregion

        #region Private Methods

        private void RefreshRect()
        {
            if (Screen.width != rect.width || Screen.height != rect.width)
            {
                rect.size = new Vector2(Screen.width, Screen.height);
                foreach (LoadingScreenComponent component in components)
                    component.RefreshRect();
            }
        }

        private static Texture2D LoadTexture(string path)
        {
            const string imageNotFound = "\nPlease place one or more images in png format inside this folder to be used as a background\n" +
                "for the loading screen. As a fallback, a black image is being used.";

            try
            {
                string[] images = Directory.GetFiles(path, "*.png");
                if (images.Length != 0)
                {
                    // Get random index
                    int index = Random.Range(0, images.Length);

                    // Import image
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(File.ReadAllBytes(Path.Combine(path, images[index])));
                    if (tex != null)
                        return tex;

                    Debug.LogError("Loading Screen: Failed to import " + images[index] + " from " + path + imageNotFound);
                }

                Debug.LogError("Loading Screen: Failed to get any image from " + path + imageNotFound);

            }
            catch (DirectoryNotFoundException)
            {
                Debug.LogError("Loading Screen: directory " + path + " is missing." + imageNotFound);
            }

            // Use a black texture as fallback
            return LoadingScreen.Mod.GetAsset<Texture2D>("defaultBackground");
        }

        #endregion
    }
}