// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using UnityEngine;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game;

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
        /// Generic tips.
        /// These strings are used for all saves.
        /// </summary>
        /// <returns></returns>
        private static List<string> GetGenericTips()
        {
            return new List<string>
            {
                // Travel
                "Try visiting a town in a different season, if you're not in the desert...",
                "Casting a Recall spell inside a dungeon is a great way for a quick exit",
                "The first cast of Recall sets the anchor, the second teleports caster back",
                "Daggerfall horses make travel shorter and faster, even more than a cart",
                "Ask people if you can't find your horse",
                "Carts are cheaper alternatives to horses, buy one in a shop",
                "Horses can be even faster with levitation...",
                "Unlike Fast Travel, Teleportation is instantaneous. It's magic after all",
                "Explore the Iliac Bay to find dungeons; or get a dungeon map",
                "Many want to send you in a dungeon, some consider it a recompensation",

                // NPCs
                "People will react differently according to your reputation",
                "Improve your reputation doing quests",
                "Shop owners and guild members will give you quests if you ask them",
                "If you're lost, ask people in town for directions",
                "Sometimes you get a Random Map from dead monsters...",
                "If you can't find you house, someone in town might be able to help",
                
                // Combat
                "Levitation allows a ranged attack from the air",
                "Diagonal Slashes, Horizontal Slashes, Vertical Chops and Thrust Forward",

                // Factions
                "If you're looking for work, there are various knightly orders around",
                "If you're interested in the job of mercenary, you can join the Fighters Guild",
                "The Mages Guild is the best place to study the world of magicka",
                "Burglars, prostitutes, professional criminals, you can find them all in the Thieves Guild",
                "If you are interested in thief, try something little and you will be rewarded eventually...",
                "The members of the Dark Brotherhood are said to be Daedra worshipper",
                "The Dark Brotherhood has been declared illegal by many, yet persists...",
                "Eight Divines, eight temples",
                "Templar Knightly Orders are associated with the worshipper of the Eight Divine",
                "To advance in a faction, you need good reputation and willing to work",
                "Legends talk about an Order of the Lamp, but nobody can prove its existence...",
                "Witch covens can be found in some dungeons and houses"
            };
        }

        /// <summary>
        /// Tips for dungeons.
        /// </summary>
        private static List<string> dungeonTips = new List<string>()
        {
            NULL
        };

        /// <summary>
        /// Tips for exteriors, towns and buildings.
        /// </summary>
        private static List<string> exteriorTips = new List<string>()
        {
            NULL
        };

        /// <summary>
        /// Save-specific tips.
        /// These strings are used when "requested" by the save.
        /// </summary>
        const string
            NULL = " ",
            LOWHELTH = "You can restore health using spells, potions or resting",
            LOWGOLD0 = "Gold can be obtained from enemies, dungeons or by selling items",
            LOWGOLD1 = "Banks can give you a loan if you're short on money",
            HIGHGOLD0 = "You can turn gold in Letters of Credit at banks",
            HIGHGOLD1 = "Coins have a weight, consider converting money into letters of credit",
            LOWLEVEL0 = "Practice your skills to increase level",
            LOWLEVEL1 = "Skill Trainers are available in guilds, every 9 hours",
            WAGON = "A wagon can carry a lot of items";

        /// <summary>
        /// Race-specific tips
        /// </summary>
        /// <param name="race">Race of player charachter.</param>
        /// <returns></returns>
        private static string RaceTip(Races race)
        {
            switch (race)
            {
                case Races.Argonian:
                    return "Argonians are known for their intelligence, agility and speed";
                case Races.Breton:
                    return "Bretons are excellent in magic arts";
                case Races.DarkElf:
                    return "Darkelves are very strong and quick";
                case Races.HighElf:
                    return "High elves are immune to Paralysing";
                case Races.Khajiit:
                    return "Khajiites are great climbers (and thieves)";
                case Races.Nord:
                    return "Nords are strong and resistant to cold temperature.";
                case Races.Redguard:
                    return "Redguards are famous for being excellent warriors";
                case Races.WoodElf:
                    return "Bow and arrors, these are the best weapons fot wood elves";
                default:
                    UnknownItem("race", race.ToString());
                    return "There are eight races in Daggerfall...";
            }
        }

        /// <summary>
        /// Gender-specific tips
        /// TODO: something more useful?
        /// </summary>
        /// <param name="gender">Gender of charachter.</param>
        private static string Gender(Genders gender)
        {
            switch (gender)
            {
                case Genders.Male:
                    return "If you're wearing it, it's probably a male armor";
                case Genders.Female:
                    return "If you're wearing it, it's probably a female armor";
                default:
                    return UnknownItem("gender", gender.ToString());
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
                    return RandomTip(GetGenericTips());
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
                    return RandomTip(GetGenericTips());
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
            tips.Add(Gender(playerEntityData.gender));

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

        #region Private Methods

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
