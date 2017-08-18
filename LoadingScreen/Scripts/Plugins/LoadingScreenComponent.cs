// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;

namespace LoadingScreen.Plugins
{
    public interface ILoadingScreenComponent
    {
        bool Enabled { get; set; }
        void Draw();
        void OnLoadingScreen(SaveData_v1 saveData);
        void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args);
        void UpdateScreen();
        void OnEndScreen();
        void OnDeathScreen();
    }

    /// <summary>
    /// Implements a component for the loading screen.
    /// </summary>
    public abstract class LoadingScreenComponent : ILoadingScreenComponent
    {
        protected bool enabled = true;

        /// <summary>
        /// Show or hide the component.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Draws on OnGui().
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Called when the user load a save.
        /// </summary>
        /// <param name="saveData">Save being loaded.</param>
        public abstract void OnLoadingScreen(SaveData_v1 saveData);

        /// <summary>
        /// Called during transition (entering/exiting building).
        /// </summary>
        /// <param name="args">Transition parameters.</param>
        public abstract void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args);

        /// <summary>
        /// Called once per frame.
        /// </summary>
        public virtual void UpdateScreen()
        {
        }

        /// <summary>
        /// Called on press-any-key screen.
        /// </summary>
        public virtual void OnEndScreen()
        {
        }

        /// <summary>
        /// Hide components that don't support death screen.
        /// Override to show and do something.
        /// </summary>
        public virtual void OnDeathScreen()
        {
            enabled = false;
        }

        public static implicit operator bool(LoadingScreenComponent plugin)
        {
            return !object.ReferenceEquals(plugin, null);
        }
    }
}
