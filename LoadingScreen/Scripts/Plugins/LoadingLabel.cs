// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:   

using UnityEngine;

namespace LoadingScreen.Plugins
{
    public class LoadingLabel
    {
        Rect rect;
        GUIStyle style;
        string label, loadingLabel, endLabel, updateChar;

        public LoadingLabel(Rect rect, GUIStyle style, string loadingLabel, string endLabel, string updateChar)
        {
            this.rect = rect;
            this.style = style;
            this.loadingLabel = loadingLabel;
            this.endLabel = endLabel;
            this.updateChar = updateChar;

            SetLoadingLabel();
        }

        public void DoGui()
        {
            GUI.Box(rect, label, style);
        }

        public void SetLoadingLabel()
        {
            label = loadingLabel;
        }

        public void UpdateLoadingLabel()
        {
            label += updateChar;
        }

        public void SetEndLabel()
        {
            label = endLabel;
        }

        public void SetLabel(string label)
        {
            this.label = label;
        }

        public void EmptyLabel()
        {
            label = string.Empty;
        }
    }
}
