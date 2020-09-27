// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using System.IO;
using System.Linq;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace LoadingScreen.Components
{
    /// <summary>
    /// Shows enemy graphics with a label.
    /// </summary>
    public class EnemyShowCase : LoadingScreenComponent
    {
        #region Fields

        bool background;
        GUIStyle backgroundStyle;
        Rect innerRect;

        string enemyName;
        Texture2D enemyTexture;

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows enemy graphics with a label.
        /// </summary>
        /// <param name="rect">Rect on screen.</param>
        /// <param name="background">Shows a background with enemy name.</param>
        public EnemyShowCase(Rect rect, bool background)
            :base(rect)
        {
            if (this.background = background)
            {
                this.backgroundStyle = new GUIStyle()
                {
                    alignment = TextAnchor.LowerCenter,
                    fontSize = 50,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                };
                backgroundStyle.normal.background = GetBackgroundTexture();

                this.innerRect = GetInnerRect(this.rect, 0.8f, 0.4f);
            }
            else
            {
                this.innerRect = this.rect;
            }
        }

        public override void Draw()
        {
            if (background)
                GUI.Label(rect, enemyName, backgroundStyle);

            GUI.DrawTexture(innerRect, enemyTexture, ScaleMode.ScaleToFit);
        }

        public override void OnLoadingScreen(SaveData_v1 saveData)
        {
            SetEnemy();
        }

        public override void OnLoadingScreen(DaggerfallTravelPopUp sender)
        {
            SetEnemy();
        }

        public override void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            SetEnemy();
        }

        public override void OnDeathScreen()
        {
            SetEnemy();
        }

        public override void RefreshRect()
        {
            base.RefreshRect();

            innerRect = background ?
                GetInnerRect(rect, 0.8f, 0.4f) :
                rect;
        }

        #endregion

        #region Private Methods

        private void SetEnemy()
        {
            // Get random enemy
            int index = Random.Range(0, GameObjectHelper.EnemyDict.Count);
            MobileEnemy mobileEnemy = GameObjectHelper.EnemyDict.ElementAt(index).Value;

            // Get random texture 
            int archive = Random.value < 0.5f ? mobileEnemy.MaleTexture : mobileEnemy.FemaleTexture;
            string fileName = string.Format("TEXTURE.{0}", archive);
            TextureFile textureFile = new TextureFile(Path.Combine(DaggerfallUnity.Instance.Arena2Path, fileName), FileUsage.UseMemory, true);
            int record = Random.Range(0, textureFile.RecordCount);
            int frame = Random.Range(0, textureFile.GetFrameCount(record));

            // Set fields
            enemyTexture = ImageReader.GetTexture(fileName, record, frame, true);
            enemyName = TextManager.Instance.GetLocalizedEnemyName(mobileEnemy.ID);
        }

        private static Texture2D GetBackgroundTexture()
        {
            Color firstColor = new Color(0, 0, 0, 0.8f);
            Color secondColor = new Color(0, 0, 0, 0.9f);

            var background = new Texture2D(1024, 1024);
            var colors = background.GetPixels32();
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.Lerp(firstColor, secondColor, Random.value);
            background.SetPixels32(colors);
            background.Apply();
            background.filterMode = FilterMode.Bilinear;
            return background;
        }

        private static Rect GetInnerRect(Rect rect, float relSize, float relY)
        {
            return new Rect(
                rect.x + rect.width * (1 - relSize) / 2,
                rect.y + rect.height * (1 - relSize) / 2 * relY,
                rect.width * relSize,
                rect.height * relSize
                );
        }

        #endregion
    }
}
