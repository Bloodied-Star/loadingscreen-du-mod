// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors: 

using System.IO;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;

namespace LoadingScreen
{
    public static class BackgroundImage
    {
        const string imageException = "\nPlease place one or more images in png format inside this folder to be used as a background\n" +
                "for the loading screen. As a fallback, a black image is being used.";

        readonly static string rootPath = Path.Combine(LoadingScreen.Mod.DirPath, "Images");

        /// <summary>
        /// Import image from disk.
        /// </summary>
        public static Texture2D Get(int loadingType, bool useSeason)
        {
            string folder = GetFolder(loadingType, useSeason);
            return LoadTexture(Path.Combine(rootPath, folder));
        }

        private static string GetFolder(int loadingType, bool useSeason)
        {
            switch (loadingType)
            {
                case LoadingType.Building:
                    return "Building";

                case LoadingType.Dungeon:
                    return "Dungeon";

                default:
                    if (useSeason)
                    {
                        if (GameManager.Instance.PlayerGPS.ClimateSettings.ClimateType == DaggerfallConnect.DFLocation.ClimateBaseType.Desert)
                            return "Desert";

                        if (DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Winter)
                            return "Winter";

                        return "Summer";
                    }
                    break;
            }

            return string.Empty;
        }

        private static Texture2D LoadTexture(string path)
        {
            try
            {
                string[] images = Directory.GetFiles(path, "*.png");
                if (images.Length != 0)
                {
                    // Get random index
                    int index = Random.Range(0, images.Length);

                    // Import image
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(File.ReadAllBytes(Path.Combine(path, images[index])));
                    if (tex != null)
                        return tex;

                    Debug.LogError("Loading Screen: Failed to import " + images[index] + " from " + path + imageException);
                }

                Debug.LogError("Loading Screen: Failed to get any image from " + path + imageException);

            }
            catch (DirectoryNotFoundException)
            {
                Debug.LogError("Loading Screen: directory " + path + " is missing." + imageException);
            }

            // Use a black texture as fallback
            return LoadingScreen.Mod.GetAsset<Texture2D>("defaultBackground");
        }
    }
}