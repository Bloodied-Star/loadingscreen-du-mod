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

namespace LoadingScreen.Plugins
{
    /// <summary>
    /// Create a level counter.
    /// </summary>
    public class LevelCounter : LoadingScreenComponent
    {
        Rect labelRect;
        Rect groupRect;
        Rect backgroundRect;
        Rect foregroundRect;

        float progress;
        string label;
        string labelFormat;

        public Texture2D BarBackground { get; set; }
        public Texture2D BarForeGround { get; set; }

        public string LabelFormat
        {
            get { return labelFormat; }
            set { SetLabelFormat(value); }
        }

        #region Public Methods

        public LevelCounter(Rect rect)
            :base(rect, 24)
        {
            this.style.alignment = TextAnchor.MiddleRight;
            this.style.fontStyle = FontStyle.Bold;

            SetRects();
        }

        public override void Draw()
        {
            // Label
            GUI.Label(labelRect, label, style);

            // Progress bar
            GUI.DrawTexture(backgroundRect, BarBackground, ScaleMode.StretchToFill);
            GUI.BeginGroup(groupRect);
                GUI.DrawTexture(foregroundRect, BarForeGround, ScaleMode.StretchToFill);
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

        public override void RefreshRect()
        {
            base.RefreshRect();

            SetRects();
        }

        public void UpdateLevelCounter(int level)
        {
            progress = CalcProgress();
            label = string.Format(LabelFormat, level);
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

        private float CalcProgress()
        {
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            float currentLevel = (playerEntity.CurrentLevelUpSkillSum - playerEntity.StartingLevelUpSkillSum + 28f) / 15f;
            return currentLevel % 1;
        }
        
        #endregion
    }
}
