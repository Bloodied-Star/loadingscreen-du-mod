// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using LoadingScreen.Plugins;

namespace LoadingScreen
{
    public interface ILoadingScreenPanel
    {
        List<LoadingScreenPlugin> Components { get; }
        Texture2D background { get; set; }
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
        readonly Rect backgroundRect = new Rect(0, 0, Screen.width, Screen.height);
        readonly List<LoadingScreenPlugin> components = new List<LoadingScreenPlugin>();

        /// <summary>
        /// Background image for the loading screen.
        /// </summary>
        public Texture2D background { get; set; }

        /// <summary>
        /// Components of the loading screen.
        /// </summary>
        public List<LoadingScreenPlugin> Components
        {
            get { return components; }
        }

        /// <summary>
        /// Draws the loading screen on GUI.
        /// </summary>
        public virtual void Draw()
        {
            GUI.DrawTexture(backgroundRect, background, ScaleMode.StretchToFill);

            foreach (LoadingScreenPlugin component in components)
            {
                if (component.Enabled)
                    component.Draw();
            }
        }

        /// <summary>
        /// Called when the user load a save.
        /// </summary>
        /// <param name="saveData">Save being loaded.</param>
        public virtual void OnLoadingScreen(SaveData_v1 saveData)
        {
            foreach (LoadingScreenPlugin component in components)
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
            foreach (LoadingScreenPlugin component in components)
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
            foreach (LoadingScreenPlugin component in components)
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
            foreach (LoadingScreenPlugin component in components)
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
            foreach (LoadingScreenPlugin component in components)
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
            foreach (LoadingScreenPlugin component in components)
                component.Enabled = true;
        }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Add a component only if is not null.
        /// </summary>
        public static void AddValid (this List<LoadingScreenPlugin> list, LoadingScreenPlugin c)
        {
            if (c != null)
                list.Add(c);
        }
    }
}