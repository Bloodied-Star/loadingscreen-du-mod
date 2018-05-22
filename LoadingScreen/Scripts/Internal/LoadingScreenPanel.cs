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
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using LoadingScreen.Components;

namespace LoadingScreen
{
    /// <summary>
    /// Implements loading screen events.
    /// </summary>
    public interface ILoadingEventsHandler
    {
        void Draw();
        void OnLoadingScreen(SaveData_v1 saveData);
        void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args);
        void UpdateScreen();
        void OnEndScreen();
        void OnDeathScreen();
    }

    /// <summary>
    /// Manages and draws components of Loading Screen.
    /// </summary>
    public class LoadingScreenPanel : ILoadingEventsHandler
    {
        #region Fields

        readonly static string imagesPath = Path.Combine(LoadingScreen.Mod.DirPath, "Images");

        readonly List<LoadingScreenComponent> components = new List<LoadingScreenComponent>();

        Rect rect = new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height));
        Texture2D background;

        #endregion

        #region Properties

        internal static string ImagesPath
        {
            get { return imagesPath; }
        }

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
            GUI.DrawTexture(rect, background, ScaleMode.StretchToFill);

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

            background = ImageReader.GetTexture("DIE_00I0.IMG");

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
            string folder = GetSplashFolder(loadingType, useSeason);
            background = LoadSplash(Path.Combine(imagesPath, folder));
        }

        #endregion

        #region Private Methods

        private void RefreshRect()
        {
            if (Screen.width != rect.width || Screen.height != rect.height)
            {
                rect.size = new Vector2(Screen.width, Screen.height);
                foreach (LoadingScreenComponent component in components)
                    component.RefreshRect();
            }
        }

        private static string GetSplashFolder(int loadingType, bool useSeason)
        {
            if (loadingType == LoadingType.Building)
                return "Building";

            if (loadingType == LoadingType.Dungeon)
                return "Dungeon";

            if (useSeason)
            {
                if (GameManager.Instance.PlayerGPS.ClimateSettings.ClimateType == DFLocation.ClimateBaseType.Desert)
                    return "Desert";

                if (DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Winter)
                    return "Winter";

                return "Summer";
            }

            return string.Empty;
        }

        private static Texture2D LoadSplash(string path)
        {
            if (!Directory.Exists(path))
                return ManageSplashError("Path {0} is not a valid directory.", path);

            string[] images = Directory.GetFiles(path, "*.png");
            if (images.Length == 0)
                return ManageSplashError("Failed to get any image from {0}.", path);

            // Get random image
            int index = Random.Range(0, images.Length);
            path = Path.Combine(path, images[index]);

            // Import image
            Texture2D tex;
            if (!TextureReplacement.TryImportTextureFromDisk(path, false, false, out tex))
                return ManageSplashError("Failed to import {0} from {1}.", images[index], path);

            return tex;
        }

        /// <summary>
        /// Print a log error and import fallback splash screen.
        /// </summary>
        private static Texture2D ManageSplashError(string format, params object[] args)
        {
            LoadingScreen.Instance.LogError("{0}\nPlease place one or more images in png format inside this folder to be used as a background" +
                "for the loading screen.\nAs a fallback, a black image is being used.", string.Format(format, args));;

            return LoadingScreen.Mod.GetAsset<Texture2D>("defaultBackground");
        }

        #endregion
    }
}