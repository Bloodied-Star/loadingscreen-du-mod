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
    public class LoadingLabel : LoadingScreenComponent
    {
        Rect rect;
        GUIStyle style;
        string label, loadingLabel, endLabel, updateChar, deathLabel;
        bool deathScreen;

        public LoadingLabel(Rect rect, GUIStyle style, string loadingLabel, string endLabel, string updateChar, string deathLabel)
        {
            this.rect = rect;
            this.style = style;
            this.loadingLabel = loadingLabel;
            this.label = loadingLabel;
            this.endLabel = endLabel;
            this.updateChar = updateChar;
            this.deathLabel = deathLabel;
            this.deathScreen = label != null && label.Length != 0;
        }

        public override void Draw()
        {
            GUI.Box(rect, label, style);
        }

        public override void OnLoadingScreen(SaveData_v1 saveData)
        {
            label = loadingLabel;
        }

        public override void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            label = loadingLabel;
        }

        public override void UpdateScreen()
        {
            label += updateChar;
        }

        public override void OnEndScreen()
        {
            label = endLabel;
        }

        public override void OnDeathScreen()
        {
            if (deathScreen)
                this.label = deathLabel;
            else
                base.OnDeathScreen();
        }
    }
}
