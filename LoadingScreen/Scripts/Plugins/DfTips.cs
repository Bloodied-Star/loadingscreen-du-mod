// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using FullSerializer;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Serialization;

/*
 * TODO:
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
        #region Tips Definition

        class DaggerfallTips
        {
            public Header header;
            public List<string> generic;
            public Location location;
            public Career career;
            public Character character;
            public Progress progress;
            public List<string> death;
        }

        class Header
        {
            public string
                project, webSite, license, sourceCode, contributors, translators;
        }

        class Location
        {
            public List<string> exterior, dungeon;
        }

        class Career
        {
            public List<string> LOWHEALT, LOWGOLD, HIGHGOLD, LOWLEVEL;
            public string HIGHLEVEL;
            public List<string> WAGON;
        }

        class Character
        {
            public Dictionary<string, List<string>> race;
            public Gender gender;
        }

        class Gender
        {
            public List<string> male, female;
        }

        class Progress
        {
            public List<string> basic, advanced;
        }

        #endregion

        #region Fields

        /// <summary>
        /// All tips from language-specific file.
        /// </summary>
        DaggerfallTips tips = new DaggerfallTips();

        /// <summary>
        /// Fallback tip.
        /// </summary>
        const string fallbackTip = "Something wrong with your tips file...";

        /// <summary>
        /// Fallback tip.
        /// </summary>
        readonly List<string> fallbackTips = new List<string>() { fallbackTip };

        // UI fields
        Rect rect;
        GUIStyle style;
        string tip = string.Empty;

        #endregion

        #region Public Methods

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
                    return RandomTip(tips.generic);
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
                    return RandomTip(tips.generic);
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
                    return RandomTip(tips.generic);
                case 1:
                case 2:
                    // Location
                    bool inDungeon = GameManager.Instance.IsPlayerInsideDungeon;
                    return RandomTip(LocationTips(inDungeon));
                default:
                    // Death
                    return RandomTip(tips.death);
            }
        }

        /// <summary>
        /// Get tips seeking information from the savegame.
        /// </summary>
        /// <param name="saveData">Save.</param>
        /// <returns>List of tips.</returns>
        private List<string> SaveTips(SaveData_v1 saveData)
        {
            var c = this.tips.career;

            // Fields
            var tips = new List<string>();
            PlayerEntityData_v1 playerEntityData = saveData.playerData.playerEntity;

            // Health
            if (playerEntityData.currentHealth < (playerEntityData.maxHealth / 4))
                tips.AddRange(c.LOWHEALT);

            // Gold
            const int lowGold = 2000, highGold = 5000;
            if (playerEntityData.goldPieces < lowGold)
                tips.AddRange(c.LOWGOLD);
            else if (playerEntityData.goldPieces > highGold)
                tips.AddRange(c.HIGHGOLD);

            // Level
            const int lowLevel = 11, highLevel = 29;
            if (playerEntityData.level < lowLevel)
                tips.AddRange(c.LOWLEVEL);
            else if (playerEntityData.level > highLevel)
                tips.Add(string.Format(c.HIGHLEVEL, playerEntityData.name));

            // Race
            tips.AddRange(RaceTip((Races)playerEntityData.raceTemplate.ID));

            // Gender
            tips.AddRange(GenderTip(playerEntityData.gender));

            // Wagon
            if (playerEntityData.wagonItems.Length == 0)
                tips.AddRange(c.WAGON);

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
            Location l = tips.location;
            switch (Random.Range(0, maxValue))
            {
                case 0:
                    return l.dungeon;
                case 1:
                    return l.exterior;
                default:
                    return inDungeon ? l.dungeon : l.exterior;
            }
        }

        /// <summary>
        /// Race-specific tips
        /// </summary>
        /// <param name="race">Race of player charachter.</param>
        /// <returns>Tips for race</returns>
        private List<string> RaceTip(Races race)
        {
            List<string> raceTips;
            if (tips.character.race.TryGetValue(race.ToString(), out raceTips))
                return raceTips;

            Debug.LogErrorFormat("Failed to get tip for race {0}", race.ToString());

            if (race != Races.None)
                return RaceTip(Races.None);

            return fallbackTips;
        }

        /// <summary>
        /// Gender-specific tips
        /// </summary>
        /// <param name="gender">Gender of charachter.</param>
        /// <returns>Tip for gender</returns>
        private List<string> GenderTip(Genders gender)
        {
            switch (gender)
            {
                case Genders.Male:
                default:
                    return tips.character.gender.male;

                case Genders.Female:
                    return tips.character.gender.female;
            }
        }

        /// <summary>
        /// Choose tips according to player level.
        /// </summary>
        /// <param name="playerLevel">Level of player in game.</param>
        /// <returns>List of tips</returns>
        private List<string> ScaledTips(int playerLevel)
        {
            return Random.Range(0, 33) > playerLevel ? tips.progress.basic : tips.progress.advanced;
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

        #region Private Methods

        /// <summary>
        /// Load tips from file on disk.
        /// English file is used as fallback.
        /// </summary>
        /// <param name="path">Folder with language files.</param>
        /// <param name="language">Name of language file without extension.</param>
        /// <returns>True if tips can be used.</returns>
        private bool Init(string path, string language)
        {
            var serializedData = File.ReadAllText(Path.Combine(path, language + ".json"));
            fsData data = fsJsonParser.Parse(serializedData);

            if (new fsSerializer().TryDeserialize<DaggerfallTips>(data, ref tips).Succeeded)
                return true;

            Debug.LogErrorFormat("Loading Screen: Failed to parse file for {0} tips, " +
                "cannot display tips without this file!", language);

            if (language != "en")
                return Init(path, "en");

            return false;
        }

        /// <summary>
        /// Init seed for random methods.
        /// </summary>
        private static void SetSeed()
        {
            Random.InitState((int)Time.time);
        }
        
        #endregion
    }
}
