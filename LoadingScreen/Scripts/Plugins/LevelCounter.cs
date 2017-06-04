// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;

/*
 * TODO
 * - Progress for next level. 
 */

namespace LoadingScreen.Plugins
{
    /// <summary>
    /// Create a level counter.
    /// </summary>
    public static class LevelCounter
    {
        /// <summary>
        /// Get a level counter.
        /// </summary>
        /// <param name="saveData">Game save.</param>
        /// <param name="upperCase">Upper case text.</param>
        public static string GetLevelCounter (SaveData_v1 saveData, bool upperCase = false)
        {
            return GetLevelCounter(saveData.playerData.playerEntity.level, upperCase);
        }

        /// <summary>
        /// Get a level counter.
        /// </summary>
        /// <param name="upperCase">Upper case text.</param>
        public static string GetLevelCounter(bool upperCase = false)
        {
            var playerEntity = GameManager.Instance.PlayerEntity;
            return GetLevelCounter(playerEntity.Level, upperCase);
        }

        /// <summary>
        /// Get a level counter.
        /// </summary>
        /// <param name="level">Current level.</param>
        /// <param name="upperCase">Upper case text.</param>
        public static string GetLevelCounter(int level, bool upperCase = false)
        {
            return string.Format("{0} {1}", upperCase ? "LEVEL" : "Level", level);
        }
    }
}
