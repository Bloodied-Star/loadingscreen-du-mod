// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using System.IO;
using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using LoadingScreen.Components;

namespace LoadingScreen
{
    /// <summary>
    /// Implements a Loading Screen window.
    /// </summary>
    public class LoadingScreenWindow
    {
        #region Fields

        protected readonly LoadingScreenPanel panel = new LoadingScreenPanel();

        #endregion

        #region Properties

        /// <summary>
        /// Show or hide the loading screen.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Panel containing all components.
        /// </summary>
        public LoadingScreenPanel Panel
        {
            get { return panel; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Make a loading screen window with the given mod settings.
        /// </summary>
        public LoadingScreenWindow(ModSettings settings)
        {
            // Setup components. This is the draw order.
            panel.AddValidComponent(MakeModelViewer(settings));
            panel.AddValidComponent(MakeEnemyPreview(settings));
            panel.AddValidComponent(MakeLabel(settings));
            panel.AddValidComponent(MakeTips(settings));
            panel.AddValidComponent(MakeQuestMessages(settings));
            panel.AddValidComponent(MakeLevelCounter(settings));
        }

        #endregion

        #region Public Methods

        public virtual void Draw()
        {
            if (Enabled)
                panel.Draw();
        }

        #endregion

        #region Load Settings

        /// <summary>
        /// Gets Loading Counter with user settings.
        /// </summary>
        /// <param name="labelText">Text shown while loading.</param>
        /// <param name="labelTextFinish">Text shown after loading.</param>
        private LoadingLabel MakeLabel(ModSettings settings)
        {
            const string loadingLabelSection = "LoadingLabel";
            if (!settings.GetBool(loadingLabelSection, "Enable"))
                return null;

            string labelText = settings.GetString(loadingLabelSection, "LoadingText");
            bool isDynamic = settings.GetBool(loadingLabelSection, "IsDynamic");
            string labelTextFinish = settings.GetString(loadingLabelSection, "EndText");
            string deathLabel = settings.GetString(loadingLabelSection, "DeathText");

            Rect rect = GetRect(
                settings.GetTupleInt(loadingLabelSection, "Position"),
                new Tuple<int, int>(10, 3));

            return new LoadingLabel(rect, labelText, labelTextFinish, isDynamic, ".", deathLabel)
            {
                Font = settings.GetInt(loadingLabelSection, "Font"),
                FontSize = settings.GetInt(loadingLabelSection, "FontSize"),
                FontStyle = (FontStyle)settings.GetInt(loadingLabelSection, "FontStyle"),
                FontColor = settings.GetColor(loadingLabelSection, "FontColor")
            };
        }

        /// <summary>
        /// Gets Daggerfall Tips with user settings.
        /// </summary>
        private DfTips MakeTips(ModSettings settings)
        {
            const string tipsSection = "Tips";
            if (!settings.GetBool(tipsSection, "Enable"))
                return null;

            Rect rect = GetRect(
                settings.GetTupleInt(tipsSection, "Position"),
                settings.GetTupleInt(tipsSection, "Size"));

            return new DfTips(rect)
            {
                Font = settings.GetInt(tipsSection, "Font"),
                FontStyle = (FontStyle)settings.GetInt(tipsSection, "FontStyle"),
                FontColor = settings.GetColor(tipsSection, "FontColor")
            };
        }

        /// <summary>
        /// Gets Quests Messages with user settings.
        /// </summary>
        private QuestsMessages MakeQuestMessages(ModSettings settings)
        {
            const string questsSection = "Quests";
            if (!settings.GetBool(questsSection, "Enable"))
                return null;

            Rect rect = GetRect(
                settings.GetTupleInt(questsSection, "Position"),
                settings.GetTupleInt(questsSection, "Size"));

            return new QuestsMessages(rect)
            {
                Font = settings.GetInt(questsSection, "Font"),
                FontStyle = (FontStyle)settings.GetInt(questsSection, "FontStyle"),
                FontColor = settings.GetColor(questsSection, "FontColor")
            };
        }

        /// <summary>
        /// Gets Level Counter with user settings.
        /// </summary>
        private LevelCounter MakeLevelCounter(ModSettings settings)
        {
            const string levelProgressSection = "LevelProgress";
            if (!settings.GetBool(levelProgressSection, "Enable"))
                return null;

            Rect rect = GetRect(
                settings.GetTupleInt(levelProgressSection, "Position"),
                settings.GetTupleInt(levelProgressSection, "Size"));

            return new LevelCounter(rect)
            {
                LabelFormat = settings.GetString(levelProgressSection, "Text"),
                Font = settings.GetInt(levelProgressSection, "Font"),
                FontColor = settings.GetColor(levelProgressSection, "FontColor"),
            };
        }

        /// <summary>
        /// Gets Enemy Preview with user settings.
        /// </summary>
        private EnemyShowCase MakeEnemyPreview(ModSettings settings)
        {
            const string enemyPreviewSection = "EnemyPreview";
            if (!settings.GetBool(enemyPreviewSection, "Enable"))
                return null;

            Rect rect = GetRect(
                settings.GetTupleInt(enemyPreviewSection, "Position"),
                settings.GetTupleInt(enemyPreviewSection, "Size"));
            bool enableBackground = settings.GetBool(enemyPreviewSection, "Background");

            return new EnemyShowCase(rect, enableBackground)
            {
                Font = 8,
                FontColor = Color.yellow
            };
        }

        private ModelViewer MakeModelViewer(ModSettings settings)
        {
            const string modelViewerSection = "ModelViewer";
            if (!settings.GetBool(modelViewerSection, "Enable"))
                return null;

            return new ModelViewer(new Rect(0, 0, 100, 100))
            {
                HorizontalPosition = settings.GetFloat(modelViewerSection, "HorizontalPosition"),
                FilmicTonemapping = settings.GetBool(modelViewerSection, "FilmicTonemapping")
            };
        }

        #endregion

        #region Helpers

        private static Rect GetRect(Tuple<int, int> position, Tuple<int, int> size)
        {
            return new Rect(
                position.First, position.Second,
                size.First, size.Second
                );
        }

        #endregion
    }
}