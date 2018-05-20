// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:   

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;

namespace LoadingScreen.Components
{
    public class LoadingLabel : LoadingScreenComponent
    {
        string label, loadingLabel, endLabel, updateChar, deathLabel;
        bool deathScreen;
        bool isDynamic;

        public LoadingLabel(Rect rect, string loadingLabel, string endLabel, bool isDynamic, string updateChar, string deathLabel)
            :base(rect)
        {
            this.loadingLabel = loadingLabel;
            this.label = loadingLabel;
            this.endLabel = endLabel;
            this.isDynamic = isDynamic;
            this.updateChar = updateChar;
            this.deathLabel = deathLabel;
            this.deathScreen = label != null && label.Length != 0;
        }

        public override void Draw()
        {
            GUI.Label(rect, label, style);
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
            if (isDynamic)
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
