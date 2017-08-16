// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using System.IO;
using UnityEngine;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using LoadingScreen.Plugins;

namespace LoadingScreen
{
    /// <summary>
    /// Load settings for Loading Screen mod and its optional plugins.
    /// </summary>
    public class LoadingScreenSetup
    {
        readonly ModSettings settings;

        #region Public Methods

        /// <summary>
        /// Constructor for LoadingScreenSetup.
        /// Use user settings for plugins to create GUI controls.
        /// </summary>
        /// <param name="settings"></param>
        public LoadingScreenSetup (ModSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Gets Loading Counter with user settings.
        /// </summary>
        /// <param name="labelText">Text shown while loading.</param>
        /// <param name="labelTextFinish">Text shown after loading.</param>
        public Plugins.LoadingLabel InitLabel()
        {
            const string loadingLabelSection = "LoadingLabel";
            if (!settings.GetBool(loadingLabelSection, "Enable"))
                return null;

            string labelText = settings.GetString(loadingLabelSection, "LoadingText");
            string labelTextFinish = settings.GetString(loadingLabelSection, "EndText");
            string deathLabel = settings.GetString("DeathScreen", "DeathText");

            var position = settings.GetTupleFloat(loadingLabelSection, "Position");
            Rect rect = new Rect(Screen.width - position.First, Screen.height - position.Second, 50, 10);
            var style = new GUIStyle()
            {
                alignment = TextAnchor.LowerRight,
                font = GetFont(settings.GetInt(loadingLabelSection, "Font")),
                fontSize = settings.GetInt(loadingLabelSection, "FontSize"),
                fontStyle = (FontStyle)settings.GetInt(loadingLabelSection, "FontStyle", 0, 3)
            };
            style.normal.textColor = settings.GetColor(loadingLabelSection, "FontColor");

            return new Plugins.LoadingLabel(rect, style, labelText, labelTextFinish, ".", deathLabel);
        }

        /// <summary>
        /// Gets Daggerfall Tips with user settings.
        /// </summary>
        public DfTips InitTips()
        {
            const string tipsSection = "Tips";
            if (!settings.GetBool(tipsSection, "Enable"))
                return null;

            TextAnchor alignment;
            var position = settings.GetTupleFloat(tipsSection, "Position");
            var size = settings.GetTupleFloat(tipsSection, "Size");
            Rect rect = GetRect(position, size.First, size.Second, out alignment);
            string language = settings.GetString(tipsSection, "Language");
            string path = Path.Combine(LoadingScreen.Mod.DirPath, "Tips");

            var style = new GUIStyle()
            {
                alignment = alignment,
                font = GetFont(settings.GetInt(tipsSection, "Font")),
                fontSize = settings.GetInt(tipsSection, "FontSize"),
                fontStyle = (FontStyle)settings.GetInt(tipsSection, "FontStyle", 0, 3),
                wordWrap = true,
            };
            style.normal.textColor = settings.GetColor(tipsSection, "FontColor");

            return new DfTips(rect, style, path, language);
        }

        /// <summary>
        /// Gets Quests Messages with user settings.
        /// </summary>
        public QuestsMessages InitQuestMessages()
        {
            const string questsSection = "Quests";
            if (!settings.GetBool(questsSection, "Enable"))
                return null;

            TextAnchor alignment;
            var position = settings.GetTupleFloat(questsSection, "Position");
            Rect rect = GetRect(position, 1000, 100, out alignment);

            GUIStyle style = new GUIStyle()
            {
                alignment = alignment,
                font = GetFont(settings.GetInt(questsSection, "Font")),
                fontSize = settings.GetInt(questsSection, "FontSize"),
                fontStyle = (FontStyle)settings.GetInt(questsSection, "FontStyle", 0, 3),
            };
            style.normal.textColor = settings.GetColor(questsSection, "FontColor");

            return new QuestsMessages(rect, style);
        }

        /// <summary>
        /// Gets Level Counter with user settings.
        /// </summary>
        public LevelCounter InitLevelCounter()
        {
            const string levelProgressSection = "LevelProgress";
            if (!settings.GetBool(levelProgressSection, "Enable"))
                return null;

            TextAnchor alignment;
            var position = settings.GetTupleFloat(levelProgressSection, "TextPosition");
            Rect rect = GetRect(position, 50, 10, out alignment);
            var style = new GUIStyle()
            {
                font = GetFont(settings.GetInt(levelProgressSection, "Font")),
                fontSize = 35,
                fontStyle = FontStyle.Bold,
                alignment = alignment,
            };
            style.normal.textColor = settings.GetColor(levelProgressSection, "FontColor");

            var barPosition = settings.GetTupleFloat(levelProgressSection, "BarPosition");
            var barSize = settings.GetTupleFloat(levelProgressSection, "BarSize");

            var constructor = new LevelCounter.LevelBar()
            {
                label = settings.GetString(levelProgressSection, "Text"),
                style = style,
                labelRect = rect,
                barRect = GetRect(barPosition, barSize.First, barSize.Second),
                background = LoadingScreen.Mod.GetAsset<Texture2D>("BarBackground"),
                progressBar = LoadingScreen.Mod.GetAsset<Texture2D>("BarProgress")
            };
            return new LevelCounter(constructor);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get Rect.
        /// Positives values starts from the left/up side of the screen,
        /// Negatives values starts from the right/bottom side.
        /// </summary>
        private static Rect GetRect(Tuple<float, float> position, float width, float height)
        {
            float rectX = position.First > 0 ? position.First : Screen.width + position.First;
            float rectY = position.Second > 0 ? position.Second : Screen.height + position.Second;
            return new Rect(rectX, rectY, width, height);
        }

        /// <summary>
        /// Get Rect and text alignment.
        /// Positives values starts from the left/up side of the screen,
        /// Negatives values starts from the right/bottom side.
        /// </summary>
        private static Rect GetRect(Tuple<float, float> position, float width, float height, out TextAnchor textAlignment)
        {
            float rectX, rectY;

            if (position.First > 0)
            {
                rectX = position.First;
                if (position.Second > 0)
                {
                    rectY = position.Second;
                    textAlignment = TextAnchor.UpperLeft;
                }
                else
                {
                    rectY = Screen.height + position.Second;
                    textAlignment = TextAnchor.LowerLeft;
                }
            }
            else
            {
                rectX = Screen.width + position.First;
                if (position.Second > 0)
                {
                    rectY = position.Second;
                    textAlignment = TextAnchor.UpperRight;
                }
                else
                {
                    rectY = Screen.height + position.Second;
                    textAlignment = TextAnchor.LowerRight;
                }
            }

            return new Rect(rectX, rectY, width, height);
        }

        /// <summary>
        /// Import a font from resources.
        /// </summary>
        private static Font GetFont(int fontID)
        {
            string name = GetFontName(fontID);
            if (name != null)
                return (Font)Resources.Load(name);

            return null; // Use unity default
        }

        /// <summary>
        /// Convert font id to font path.
        /// </summary>
        private static string GetFontName(int fontID)
        {
            switch (fontID)
            {
                case 1:
                    return "Fonts/OpenSans/OpenSans-ExtraBold";
                case 2:
                    return "Fonts/OpenSans/OpenSansBold";
                case 3:
                    return "Fonts/OpenSans/OpenSansSemibold";
                case 4:
                    return "Fonts/OpenSans/OpenSansRegular";
                case 5:
                    return "Fonts/OpenSans/OpenSansLight";
                case 6:
                    return "Fonts/TESFonts/Kingthings Exeter";
                case 7:
                    return "Fonts/TESFonts/Kingthings Petrock";
                case 8:
                    return "Fonts/TESFonts/Kingthings Petrock light";
                case 9:
                    return "Fonts/TESFonts/MorrisRomanBlack";
                case 10:
                    return "Fonts/TESFonts/oblivion-font";
                case 11:
                    return "Fonts/TESFonts/Planewalker";
                default:
                    return null;
            }
        }
        
        #endregion

    }
}
