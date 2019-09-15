// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace LoadingScreen.Components
{
    /// <summary>
    /// Create a level counter.
    /// </summary>
    public class LevelCounter : LoadingScreenComponent
    {
        #region Fields

        readonly Texture2D barBackground;
        readonly Texture2D barForeGround;

        Rect labelRect;
        Rect groupRect;
        Rect backgroundRect;
        Rect foregroundRect;

        float progress;
        string label;
        string labelFormat;

        #endregion

        #region Properties

        public string LabelFormat
        {
            get { return labelFormat; }
            set { SetLabelFormat(value); }
        }

        #endregion

        #region Public Methods

        public LevelCounter(Rect rect)
            :base(rect, 24)
        {
            this.style.alignment = TextAnchor.MiddleRight;
            this.style.fontStyle = FontStyle.Bold;
            this.barBackground = ImportTexture("BarBackground");
            this.barForeGround = ImportTexture("BarProgress");

            SetRects();
        }

        public override void Draw()
        {
            // Label
            GUI.Label(labelRect, label, style);

            // Progress bar
            GUI.DrawTexture(backgroundRect, barBackground, ScaleMode.StretchToFill);
            GUI.BeginGroup(groupRect);
                GUI.DrawTexture(foregroundRect, barForeGround, ScaleMode.StretchToFill);
            GUI.EndGroup();
        }

        public override void OnLoadingScreen(SaveData_v1 saveData)
        {
            var playerEntity = saveData.playerData.playerEntity;
            UpdateLevelCounter(playerEntity.level, playerEntity.startingLevelUpSkillSum, playerEntity.currentLevelUpSkillSum);
        }

        public override void OnLoadingScreen(DaggerfallTravelPopUp sender)
        {
            UpdateLevelCounter();
        }

        public override void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            UpdateLevelCounter();
        }

        public override void RefreshRect()
        {
            base.RefreshRect();

            SetRects();
        }

        /// <summary>
        /// Updates level counter from current player entity.
        /// </summary>
        public void UpdateLevelCounter()
        {
            var playerEntity = GameManager.Instance.PlayerEntity;
            UpdateLevelCounter(playerEntity.Level, playerEntity.StartingLevelUpSkillSum, playerEntity.CurrentLevelUpSkillSum);
        }

        /// <summary>
        /// Updates level counter from given stats.
        /// </summary>
        public void UpdateLevelCounter(int level, int startingLevelUpSkillSum, int currentLevelUpSkillSum = 0)
        {
            progress = currentLevelUpSkillSum != 0 ? (currentLevelUpSkillSum - startingLevelUpSkillSum + 28f) / 15f % 1 : 0;
            label = string.Format(LabelFormat, level);
            groupRect = new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.5f * progress, rect.height);
        }

        #endregion

        #region Private Methods

        private void SetRects()
        {
            labelRect = new Rect(rect.x, rect.y, rect.width * 0.45f, rect.height);
            groupRect = new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.5f * progress, rect.height);
            backgroundRect = new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.5f, rect.height);
            foregroundRect = new Rect(0, 0, rect.width * 0.5f, rect.height);
        }

        private void SetLabelFormat(string format)
        {
            if (format.Contains("%"))
                labelFormat = format.Replace("%", "{0}");
            else
                labelFormat = string.Format("{0} {1}", format, "{0}");
        }

        #endregion
    }
}
