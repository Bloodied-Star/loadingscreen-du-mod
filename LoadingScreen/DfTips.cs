// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using UnityEngine;
using System.Collections.Generic;
using IniParser.Model;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Serialization;

/*
 * TODO:
 * - Improve dungeonTips.
 * - Improve GenderTip().
 * - Seek informations from quests.
 */

namespace LoadingScreen
{
    /// <summary>
    /// Provide a string to be shown on the loading screen,
    /// taking in consideration informations obtained from the save game
    /// with the purpose of providing useful tips.
    /// </summary>
    public static class DfTips
    {
        #region Tips

        /// <summary>
        /// All tips from language-specific file.
        /// </summary>
        public static IniData tips { get; set; }

        /// <summary>
        /// Generic tips.
        /// </summary>
        static List<string> genericTips { get { return tips.Sections.GetSectionData("Generic").Comments; } }

        /// <summary>
        /// Tips for dungeons.
        /// TODO: expand with more tips.
        /// </summary>
        static List<string> dungeonTips { get { return tips.Sections.GetSectionData("Dungeon").Comments; } }

        /// <summary>
        /// Tips for exteriors, towns and buildings.
        /// </summary>
        static List<string> exteriorTips { get { return tips.Sections.GetSectionData("Exterior").Comments; } }

        /// <summary>
        /// Save-specific tips.
        /// These strings are used when "requested" by the save.
        /// </summary>
        const string careerTips = "Career";
        static string NULL { get { return tips[careerTips]["NULL"]; } }
        static string LOWHELTH { get { return tips[careerTips]["LOWHELTH"]; } }
        static string LOWGOLD0 { get { return tips[careerTips]["LOWGOLD0"]; } }
        static string LOWGOLD1 { get { return tips[careerTips]["LOWGOLD1"]; } }
        static string HIGHGOLD0 { get { return tips[careerTips]["HIGHGOLD0"]; } }
        static string HIGHGOLD1 { get { return tips[careerTips]["HIGHGOLD1"]; } }
        static string LOWLEVEL0 { get { return tips[careerTips]["LOWLEVEL0"]; } }
        static string LOWLEVEL1 { get { return tips[careerTips]["LOWLEVEL1"]; } }
        static string WAGON { get { return tips[careerTips]["WAGON"]; } }

        /// <summary>
        /// Race-specific tips
        /// </summary>
        /// <param name="race">Race of player charachter.</param>
        /// <returns>Tip for race</returns>
        private static string RaceTip(Races race)
        {
            const string raceSection = "Race";
            try
            {
                string tip = tips[raceSection][race.ToString()];
                if (tip != null)
                    return tip;
                else
                    throw new System.Exception();
            }
            catch
            {
                UnknownItem(raceSection, race.ToString());
                return tips[raceSection]["Unknown"];
            }
        }

        /// <summary>
        /// Gender-specific tips
        /// TODO: something more useful?
        /// </summary>
        /// <param name="gender">Gender of charachter.</param>
        /// <returns>Tip for gender</returns>
        private static string GenderTip(Genders gender)
        {
            const string genderSection = "Gender";
            try
            {
                string tip = tips[genderSection][gender.ToString()];
                if (tip != null)
                    return tip;
                else
                    throw new System.Exception();
            }
            catch
            {
                return UnknownItem(genderSection, gender.ToString());
            }
        }

        #endregion

        #region Algorithm

        /// <summary>
        /// Get a tip to show on screen for save loading.
        /// </summary>
        /// <param name="saveData">Save being loaded.</param>
        /// <returns>Tip</returns>
        public static string GetTip(SaveData_v1 saveData)
        {
            SetSeed();
            switch (Random.Range(0, 3))
            {
                case 0:
                    // Save specific
                    return RandomTip(SaveTips(saveData));
                case 1:
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
        public static string GetTip(PlayerEnterExit.TransitionType transitionType)
        {
            // Get a generic tip or one specific to transitioning
            SetSeed();
            const int maxValue = 3; // the higher, the more probable it will be specific
            switch (Random.Range(0, maxValue))
            {
                case 0:
                    return RandomTip(genericTips);
                default:
                    bool inDungeon = (transitionType == PlayerEnterExit.TransitionType.ToDungeonInterior);
                    return RandomTip(LocationTips(inDungeon));
            }
        }

        /// <summary>
        /// Get tips seeking information from the savegame.
        /// </summary>
        /// <param name="saveData">Save.</param>
        /// <returns>List of tips.</returns>
        private static List<string> SaveTips(SaveData_v1 saveData)
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
        private static List<string> LocationTips(bool inDungeon)
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
                return NULL;
            }
        }

        #endregion

        #region Utilities

        private static void SetSeed()
        {
            Random.InitState((int)Time.time);
        }

        /// <summary>
        /// Print error to log and return default string.
        /// </summary>
        /// <param name="itemClass">Item description</param>
        /// <param name="item">Item.ToString()</param>
        static private string UnknownItem (string itemClass, string item)
        {
            Debug.LogError(string.Format("LoadingScreen: Failed to get {0} of player\nGot: {1}", itemClass, item));
            return NULL;
        }
        
        #endregion
    }
}
