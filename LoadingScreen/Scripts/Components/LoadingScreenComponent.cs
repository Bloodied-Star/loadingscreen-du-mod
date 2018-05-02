// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using System;
using UnityEngine;
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
        #region Fields & Properties

        private readonly Rect relRect;

        protected bool enabled = true;
        protected Rect rect;
        protected GUIStyle style;

        int referenceFontSize = -1;

        /// <summary>
        /// Show or hide the component.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public Rect Rect
        {
            get { return rect; }
        }

        public int Font
        {
            set { SetFont(value); }
        }

        public int FontSize
        {
            get { return style.fontSize; }
            set { style.fontSize = value; }
        }

        public FontStyle FontStyle
        {
            get { return style.fontStyle; }
            set { style.fontStyle = value; }
        }

        public Color FontColor
        {
            get { return style.normal.textColor; }
            set { style.normal.textColor = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Set component rect and style.
        /// </summary>
        /// <param name="virtualRect">Rect in a 100x100 resolution.</param>
        protected LoadingScreenComponent(Rect virtualRect)
        {
            Rect relRect = new Rect(
                virtualRect.position / 100f,
                virtualRect.size / 100f);

            var style = new GUIStyle();
            style.alignment = GetTextAnchor(relRect);

            if (relRect.x < 0)
                relRect.x += 1 - relRect.width;
            if (relRect.y < 0)
                relRect.y += 1 - relRect.height;
            relRect.x = Mathf.Clamp(relRect.x, 0, 1 - relRect.width);
            relRect.y = Mathf.Clamp(relRect.y, 0, 1 - relRect.height);

            this.relRect = relRect;
            this.rect = new Rect(RelToScreen(relRect.position), RelToScreen(relRect.size));
            this.style = style;
        }

        /// <summary>
        /// Set component rect and style.
        /// Font size will be automatically scaled on resolution and rect size.
        /// </summary>
        /// <param name="virtualRect">Rect in a 100x100 resolution.</param>
        /// <param name="referenceFontSize">A font size that fits the rect.</param>
        protected LoadingScreenComponent(Rect virtualRect, int referenceFontSize)
            :this(virtualRect)
        {
            this.referenceFontSize = referenceFontSize;
            this.style.fontSize = CalcFontSize(referenceFontSize);
        }

        #endregion

        #region Public Methods

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

        public virtual void RefreshRect()
        {
            rect.position = RelToScreen(relRect.position);
            rect.size = RelToScreen(relRect.size);

            if (referenceFontSize != -1)
                style.fontSize = CalcFontSize(referenceFontSize);
        }

        public static implicit operator bool(LoadingScreenComponent plugin)
        {
            return !object.ReferenceEquals(plugin, null);
        }

        #endregion

        /// <summary>
        /// Gets font size scaled on resolution and rect size.
        /// </summary>
        /// <param name="referenceSize">A font size that fits the rect.</param>
        /// <returns>Font size for the guistyle</returns>
        protected int CalcFontSize(int referenceSize)
        {
            Vector2 scale = new Vector2(rect.width / 100, rect.height / 100);
            return Mathf.RoundToInt(referenceSize * (scale.x + scale.y) / 2);
        }

        #region Private Methods

        private void SetFont(int fontID)
        {
            string name = GetFontName(fontID);
            style.font = name != null ? Resources.Load<Font>(name) : null;
        }

        /// <summary>
        /// Get text alignment for this relative rect.
        /// </summary>
        /// <remarks>
        /// From 0 to 1 is left(up) alignment, from 0 to -1 is right(bottom) aligment.
        /// Center and Middle are used according to center point.
        /// </remarks>
        private static TextAnchor GetTextAnchor(Rect relRect)
        {
            bool isCenter = Mathf.Approximately(Mathf.Abs(relRect.center.x), 0.5f);
            bool isMiddle = Mathf.Approximately(Mathf.Abs(relRect.center.y), 0.5f);

            // Center
            if (isCenter)
            {
                if (isMiddle)
                    return TextAnchor.MiddleCenter;

                if (relRect.y > 0)
                    return TextAnchor.UpperCenter;
                return TextAnchor.LowerCenter;
            }

            // Left
            if (relRect.x > 0)
            {
                if (isMiddle)
                    return TextAnchor.MiddleLeft;

                if (relRect.y > 0)
                    return TextAnchor.UpperLeft;
                return TextAnchor.LowerLeft;
            }

            // Right
            if (isMiddle)
                return TextAnchor.MiddleRight;

            if (relRect.y > 0)
                return TextAnchor.UpperRight;
            return TextAnchor.LowerRight;
        }

        private static Vector2 RelToScreen(Vector2 rel)
        {
            return new Vector2(rel.x * Screen.width, rel.y * Screen.height);
        }

        /// <summary>
        /// Convert font id to font path.
        /// </summary>
        private static string GetFontName(int fontID)
        {
            switch (fontID)
            {
                case 0:     return null; // Arial or Liberation Sans
                case 1:     return "Fonts/OpenSans/OpenSans-ExtraBold";
                case 2:     return "Fonts/OpenSans/OpenSansBold";
                case 3:     return "Fonts/OpenSans/OpenSansSemibold";
                case 4:     return "Fonts/OpenSans/OpenSansRegular";
                case 5:     return "Fonts/OpenSans/OpenSansLight";
                case 6:     return "Fonts/TESFonts/Kingthings Exeter";
                case 7:     return "Fonts/TESFonts/Kingthings Petrock";
                case 8:     return "Fonts/TESFonts/Kingthings Petrock light";
                case 9:     return "Fonts/TESFonts/MorrisRomanBlack";
                case 10:    return "Fonts/TESFonts/oblivion-font";
                case 11:    return "Fonts/TESFonts/Planewalker";
            }

            throw new ArgumentException(string.Format("Undefined fontID: {0}", fontID));
        }

        #endregion
    }
}
