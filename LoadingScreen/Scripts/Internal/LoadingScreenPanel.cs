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
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace LoadingScreen
{
    /// <summary>
    /// Implements loading screen events.
    /// </summary>
    public interface ILoadingEventsHandler
    {
        void Draw();
        void OnLoadingScreen(SaveData_v1 saveData);
        void OnLoadingScreen(DaggerfallTravelPopUp source);
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

        internal readonly static string ImagesPath = Path.Combine(TextureReplacement.TexturesPath, "Splash");
        internal readonly static string ResourcesPath = Path.Combine(ImagesPath, "Resources");

        readonly List<LoadingScreenComponent> components = new List<LoadingScreenComponent>();

        Rect rect = new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height));
        Texture2D background;

        #endregion

        #region Properties

        /// <summary>
        /// Components of the loading screen.
        /// </summary>
        public List<LoadingScreenComponent> Components
        {
            get { return components; }
        }

        public int CurrentLoadingType { get; private set; }

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
            CurrentLoadingType = LoadingType.Get(saveData.playerData.playerPosition);
            RefreshRect();
            RefreshBackground();

            foreach (LoadingScreenComponent component in components)
            {
                if (component.Enabled)
                    component.OnLoadingScreen(saveData);
            }
        }

        /// <summary>
        /// Called during a fast travel.
        /// </summary>
        /// <param name="sender">Travel popup.</param>
        public virtual void OnLoadingScreen(DaggerfallTravelPopUp sender)
        {
            CurrentLoadingType = LoadingType.Default;
            RefreshRect();
            RefreshBackground();

            foreach (LoadingScreenComponent component in components)
            {
                if (component.Enabled)
                    component.OnLoadingScreen(sender);
            }
        }

        /// <summary>
        /// Called during transition (entering/exiting building).
        /// </summary>
        /// <param name="args">Transition parameters.</param>
        public virtual void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            CurrentLoadingType = LoadingType.Get(args.TransitionType);
            RefreshRect();
            RefreshBackground();

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
            {
                component.Parent = this;
                components.Add(component);
            }
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

        /// <summary>
        /// Sets a custom background from loose files or silently fallbacks to default black screen.
        /// </summary>
        private void RefreshBackground()
        {
            if (!TryLoadSplash(GetSplashFolder(), out background) && !TryLoadSplash("", out background))
                background = LoadingScreen.Mod.GetAsset<Texture2D>("defaultBackground");
        }

        /// <summary>
        /// Gets the name of the folder with the background textures for current state.
        /// </summary>
        private string GetSplashFolder()
        {
            if (CurrentLoadingType == LoadingType.Building)
                return "Building";

            if (CurrentLoadingType == LoadingType.Dungeon)
                return "Dungeon";

            if (GameManager.Instance.PlayerGPS.ClimateSettings.ClimateType == DFLocation.ClimateBaseType.Desert)
                return "Desert";

            if (DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Winter)
                return "Winter";

            return "Summer";
        }

        /// <summary>
        /// Tries to load a random custom background from loose files.
        /// </summary>
        /// <param name="folder">Name of folder or empty string for root.</param>
        /// <param name="tex">Imported texture or null.</param>
        /// <returns>True if a texture has been found.</returns>
        private static bool TryLoadSplash(string folder, out Texture2D tex)
        {
            string directory = Path.Combine(ImagesPath, folder);
            if (Directory.Exists(directory))
            {
                string[] images = Directory.GetFiles(directory, "*.png");
                if (images.Length > 0)
                {
                    string path = Path.Combine(directory, images[Random.Range(0, images.Length)]);
                    return TextureReplacement.TryImportTextureFromLooseFiles(path, false, false, true, out tex);
                }
            }

            tex = null;
            return false;
        }

        #endregion
    }
}