// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:    

using DaggerfallWorkshop.Game.Serialization;
using TransitionType = DaggerfallWorkshop.Game.PlayerEnterExit.TransitionType;

namespace LoadingScreen
{
    struct LoadingType
    {
        public const int

            Default = 0,
            Building = 1,
            Dungeon = 2;

        public static int Get(PlayerPositionData_v1 playerPosition)
        {
            if (playerPosition.insideBuilding)
                return Building;

            if (playerPosition.insideDungeon)
                return Dungeon;

            return Default;
        }

        public static int Get(TransitionType transitionType)
        {
            switch (transitionType)
            {
                case TransitionType.ToBuildingInterior:
                    return Building;

                case TransitionType.ToDungeonInterior:
                    return Dungeon;

                default:
                    return Default;
            }
        }
    }
}