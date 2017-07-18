// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;

namespace LoadingScreen.Plugins
{
    /// <summary>
    /// Create a level counter.
    /// </summary>
    public class LevelCounter : LoadingScreenPlugin
    {
        /// <summary>
        /// Constructor of level indicator and bar.
        /// </summary>
        public class LevelBar
        {
            public string label;                  // Label for level counter
            public GUIStyle style;                // Style for level counter
            public Rect labelRect;                // Rect of level counter
            public Rect barRect;                  // Rect of bar
            public Texture2D background;          // Texture for bar background
            public Texture2D progressBar;         // Texture for bar foreground; this is partially shown
        }

        // Fields
        private LevelBar levelBar;               // Gui elements and settings
        private string label;                    // Label for level counter
        private Rect rect;                       // Rect for group (affects foreground texture)
        private Rect barInGroupRect;             // Rect for foreground texture inside group
        private float n;                         // Size of 1% (1/100 of total lenght)

        #region Public Methods

        /// <summary>
        /// Constructor for Level Counter/Bar
        /// </summary>
        /// <param name="label">Label to show on indicator.</param>
        /// <param name="rect">Rect of Begingroup, equals to background rect on full bar.</param>
        public LevelCounter(LevelBar levelBar)
        {
            // Rect for foreground texture
            this.barInGroupRect = new Rect(0, 0, levelBar.barRect.width, levelBar.barRect.height);

            // Initial rect for group
            this.rect = new Rect(levelBar.barRect);
            this.n = rect.width / 100;

            // Other components
            this.levelBar = levelBar;
        }

        public override void Draw()
        {
            // Counter
            GUI.Label(levelBar.labelRect, label, levelBar.style);

            // Background texture
            GUI.DrawTexture(levelBar.barRect, levelBar.background, ScaleMode.StretchToFill);

            // Progress bar
            GUI.BeginGroup(rect);
                GUI.DrawTexture(barInGroupRect, levelBar.progressBar, ScaleMode.StretchToFill);
            GUI.EndGroup();
        }

        public override void OnLoadingScreen(SaveData_v1 saveData)
        {
            UpdateLevelCounter(saveData.playerData.playerEntity.level);
        }

        public override void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            UpdateLevelCounter(GameManager.Instance.PlayerEntity.Level);
        }

        public void UpdateLevelCounter(int level)
        {
            UpdateRect();
            label = string.Format("{0} {1}", levelBar.label, level);
        }
        
        #endregion

        #region Private Methods

        private void UpdateRect()
        {
            int levelProgress = CalculateLevelPercent();
            rect.width = n * levelProgress;
        }

        private int CalculateLevelPercent()
        {
            return (int)(CalculateLevelDecimal() * 100);
        }

        private float CalculateLevelDecimal()
        {
            var playerEntity = GameManager.Instance.PlayerEntity;
            float currentLevel = (float)(playerEntity.CurrentLevelUpSkillSum - playerEntity.StartingLevelUpSkillSum + 28f) / 15f;
            return currentLevel % 1;
        }
        
        #endregion
    }
}
