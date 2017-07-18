// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using UnityEngine;
using System.IO;
using System.Collections.Generic;
using IniParser;
using IniParser.Model;
using IniParser.Exceptions;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Serialization;

/*
 * TODO:
 * - Add more dungeonTips, basicTips and advancedTips.
 * - Improve GenderTip().
 * - Seek informations from quests.
 */

namespace LoadingScreen.Plugins
{
    /// <summary>
    /// Provide a string to be shown on the loading screen,
    /// taking in consideration informations obtained from the save game
    /// with the purpose of providing useful tips.
    /// </summary>
    public class DfTips : LoadingScreenPlugin
    {
        #region Tips

        /// <summary>
        /// All tips from language-specific file.
        /// </summary>
        IniData tips;

        /// <summary>
        /// Generic tips.
        /// </summary>
        List<string> genericTips { get { return tips.Sections.GetSectionData("Generic").Comments; } }

        /// <summary>
        /// Tips for dungeons.
        /// </summary>
        List<string> dungeonTips { get { return tips.Sections.GetSectionData("Dungeon").Comments; } }

        /// <summary>
        /// Tips for exteriors, towns and buildings.
        /// </summary>
        List<string> exteriorTips { get { return tips.Sections.GetSectionData("Exterior").Comments; } }

        /// <summary>
        /// Save-specific tips.
        /// These strings are used when "requested" by the save.
        /// </summary>
        const string careerTips = "Career";
        const string NULL = "";
        string LOWHELTH { get { return tips[careerTips]["LOWHELTH"]; } }
        string LOWGOLD0 { get { return tips[careerTips]["LOWGOLD0"]; } }
        string LOWGOLD1 { get { return tips[careerTips]["LOWGOLD1"]; } }
        string HIGHGOLD0 { get { return tips[careerTips]["HIGHGOLD0"]; } }
        string HIGHGOLD1 { get { return tips[careerTips]["HIGHGOLD1"]; } }
        string LOWLEVEL0 { get { return tips[careerTips]["LOWLEVEL0"]; } }
        string LOWLEVEL1 { get { return tips[careerTips]["LOWLEVEL1"]; } }
        string WAGON { get { return tips[careerTips]["WAGON"]; } }

        /// <summary>
        /// Race-specific tips
        /// </summary>
        /// <param name="race">Race of player charachter.</param>
        /// <returns>Tip for race</returns>
        private string RaceTip(Races race)
        {
            const string raceSection = "Race";
            string tip = tips[raceSection][race.ToString()];
            return tip != null ? tip : (EmptyItem(raceSection, race.ToString(), race!=Races.None ? RaceTip(Races.None) : ""));
        }

        /// <summary>
        /// Gender-specific tips
        /// </summary>
        /// <param name="gender">Gender of charachter.</param>
        /// <returns>Tip for gender</returns>
        private string GenderTip(Genders gender)
        {
            const string genderSection = "Gender";
            string tip = tips[genderSection][gender.ToString()];
            return tip != null ? tip : EmptyItem(genderSection, gender.ToString());
        }

        /// <summary>
        /// These are shown with more frequency at lower levels.
        /// </summary>
        List<string> basicTips { get { return tips.Sections.GetSectionData("Basic").Comments; } }

        /// <summary>
        /// These are shown with more frequency at higher levels.
        /// </summary>
        List<string> advancedTips { get { return tips.Sections.GetSectionData("Advanced").Comments; } }

        /// <summary>
        /// Tips for player death.
        /// </summary>
        List<string> deathTips { get { return tips.Sections.GetSectionData("Death").Comments; } }

        /// <summary>
        /// Fallback tip.
        /// </summary>
        const string fallbackTip = "Something wrong with your tips file...";

        #endregion

        #region Public Methods

        Rect rect;
        GUIStyle style;
        string tip = string.Empty;

        /// <summary>
        /// Constructor for Daggefall Tips.
        /// </summary>
        /// <param name="path">Folder with language files.</param>
        /// <param name="language">Name of language file without extension.</param>
        public DfTips(Rect rect, GUIStyle style, string path, string language)
        {
            this.rect = rect;
            this.style = style;
            enabled = Init(path, language);
        }

        public override void Draw()
        {
            GUI.Label(rect, tip, style);
        }

        public override void OnLoadingScreen(SaveData_v1 saveData)
        {
            tip = GetTip(saveData);
        }

        public override void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            tip = GetTip(args.TransitionType);
        }

        public override void OnDeathScreen()
        {
            tip = GetTip();
        }

        #endregion

        #region Algorithm

        /// <summary>
        /// Get a tip to show on screen for save loading.
        /// </summary>
        /// <param name="saveData">Save being loaded.</param>
        /// <returns>Tip</returns>
        public string GetTip(SaveData_v1 saveData)
        {
            SetSeed();
            switch (Random.Range(0, 6))
            {
                case 0:
                    // Save specific
                    return RandomTip(SaveTips(saveData));
                case 1:
                case 2:
                    // Scaled on level
                    int playerLevel = saveData.playerData.playerEntity.level;
                    return RandomTip(ScaledTips(playerLevel));
                case 3:
                case 4:
                    // Location
                    return RandomTip(LocationTips(saveData.playerData.playerPosition.insideDungeon));
                default:
                    // Generic tips
                    return RandomTip(genericTips);
            }
        }

        /// <summary>
        /// Get a tip to show on screen for entering/exiting.
        /// </summary>
        /// <param name="transitionType">Transition in action.</param>
        /// <returns>Tip</returns>
        public string GetTip(PlayerEnterExit.TransitionType transitionType)
        {
            SetSeed();
            const int maxValue = 5;
            switch (Random.Range(0, maxValue))
            {
                case 0:
                    // Generic tips
                    return RandomTip(genericTips);
                case 1:
                case 2:
                    // Scaled on level
                    int playerLevel = GameManager.Instance.PlayerEntity.Level;
                    return RandomTip(ScaledTips(playerLevel));
                default:
                    // Location
                    bool inDungeon = (transitionType == PlayerEnterExit.TransitionType.ToDungeonInterior);
                    return RandomTip(LocationTips(inDungeon));
            }
        }

        /// <summary>
        /// Get a tip to show on screen for Death Screen.
        /// </summary>
        /// <returns>Tip</returns>
        public string GetTip()
        {
            SetSeed();
            switch (Random.Range(0, 6))
            {
                case 0:
                    // Generic tips
                    return RandomTip(genericTips);
                case 1:
                case 2:
                    // Location
                    bool inDungeon = GameManager.Instance.IsPlayerInsideDungeon;
                    return RandomTip(LocationTips(inDungeon));
                default:
                    // Death
                    return RandomTip(deathTips);
            }
        }

        /// <summary>
        /// Get tips seeking information from the savegame.
        /// </summary>
        /// <param name="saveData">Save.</param>
        /// <returns>List of tips.</returns>
        private List<string> SaveTips(SaveData_v1 saveData)
        {
            // Fields
            var tips = new List<string>();
            PlayerEntityData_v1 playerEntityData = saveData.playerData.playerEntity;

            // Health
            if (playerEntityData.currentHealth < (playerEntityData.maxHealth / 4))
                tips.Add(LOWHELTH);

            // Gold
            const int lowGold = 2000, highGold = 5000;
            if (playerEntityData.goldPieces < lowGold)
            {
                tips.Add(LOWGOLD0);
                tips.Add(LOWGOLD1);
            }
            else if (playerEntityData.goldPieces > highGold)
            {
                tips.Add(HIGHGOLD0);
                tips.Add(HIGHGOLD1);
            }

            // Level
            const int lowLevel = 11, highLevel = 29;
            if (playerEntityData.level < lowLevel)
            {
                tips.Add(LOWLEVEL0);
                tips.Add(LOWLEVEL1);
            }
            else if (playerEntityData.level > highLevel)
                tips.Add(string.Format("They say great things about {0}", playerEntityData.name));

            // Race
            tips.Add(RaceTip((Races)playerEntityData.raceTemplate.ID));

            // Gender
            tips.Add(GenderTip(playerEntityData.gender));

            // Wagon
            if (playerEntityData.wagonItems.Length == 0)
                tips.Add(WAGON);

            return tips;
        }

        /// <summary>
        /// Get tip specific to location
        /// </summary>
        /// <param name="inDungeon">Dungeon or exteriors?</param>
        /// <returns>List of tips</returns>
        private List<string> LocationTips(bool inDungeon)
        {
            const int maxValue = 6; // the higher, the more probable it will be specific
            switch (Random.Range(0, maxValue))
            {
                case 0:
                    return dungeonTips;
                case 1:
                    return exteriorTips;
                default:
                    return inDungeon ? dungeonTips : exteriorTips;
            }
        }

        /// <summary>
        /// Choose tips according to player level.
        /// </summary>
        /// <param name="playerLevel">Level of player in game.</param>
        /// <returns>List of tips</returns>
        private List<string> ScaledTips(int playerLevel)
        {
            return Random.Range(0, 33) > playerLevel ? basicTips : advancedTips;
        }

        /// <summary>
        /// Get one tip from a list.
        /// </summary>
        /// <param name="tips">List of tips.</param>
        /// <returns>One tip.</returns>
        private static string RandomTip(List<string> tips)
        {
            try
            {
                int index = Random.Range(0, tips.Count);
                return tips[index];
            }
            catch (System.Exception e)
            {
                Debug.LogError("LoadingScreen: Failed to get a tip string\n" + e);
                return string.Format("{0}({1})", fallbackTip, e.Message);
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Load tips from file on disk.
        /// English file is used as fallback.
        /// </summary>
        /// <param name="path">Folder with language files.</param>
        /// <param name="language">Name of language file without extension.</param>
        /// <returns>True if tips can be used.</returns>
        public bool Init(string path, string language)
        {
            try
            {
                var parser = new FileIniDataParser();
                tips = parser.ReadFile(Path.Combine(path, language + ".ini"));
                return true;
            }
            catch (ParsingException e)
            {
                string message = e.InnerException != null ? e.InnerException.Message : e.Message;
                Debug.LogError(string.Format("Loading Screen: Failed to parse file with '{0}' tips, " +
                    "cannot display tips without this file!\n{1}", language, message));

                return language != "en" ? Init(path, "en") : false;
            }
        }

        /// <summary>
        /// Init seed for random methods.
        /// </summary>
        private static void SetSeed()
        {
            Random.InitState((int)Time.time);
        }

        /// <summary>
        /// Print error to log and return fallback string.
        /// </summary>
        /// <param name="itemClass">Item description</param>
        /// <param name="item">Item.ToString()</param>
        static private string EmptyItem (string itemClass, string item, string fallback = fallbackTip)
        {
            Debug.LogError(string.Format("LoadingScreen: Failed to read {0} ({1})", itemClass, item));
            return fallback;
        }
        
        #endregion
    }
}
