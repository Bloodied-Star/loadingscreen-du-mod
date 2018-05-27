// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:     

using System;
using UnityEngine;
using Wenzil.Console;

namespace LoadingScreen
{
    /// <summary>
    /// Console commands for Vibrant Wind.
    /// </summary>
    public static class LoadingScreenConsoleCommands
    {
        const string noInstanceMessage = "Loading Screen instance not found.";

        public static void RegisterCommands()
        {
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(ToggleLoadingScreen.name, ToggleLoadingScreen.description, ToggleLoadingScreen.usage, ToggleLoadingScreen.Execute);
                ConsoleCommandsDatabase.RegisterCommand(SimulateLoadingScreen.name, SimulateLoadingScreen.description, SimulateLoadingScreen.usage, SimulateLoadingScreen.Execute);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error Registering Loading Screen Console commands: {0}", e.Message));
            }
        }

        private static class ToggleLoadingScreen
        {
            public static readonly string name = "loadingscreen_toggle";
            public static readonly string description = "Enable/Disable loading screen.";
            public static readonly string usage = "loadingscreen_toggle";

            public static string Execute(params string[] args)
            {
                var loadingScreen = LoadingScreen.Instance;
                if (loadingScreen == null)
                    return noInstanceMessage;

                loadingScreen.Toggle();
                return loadingScreen.ToString();
            }
        }

        private static class SimulateLoadingScreen
        {
            public static readonly string name = "loadingscreen_simulate";
            public static readonly string description = "Simulate a loading screen.";
            public static readonly string usage = "loadingscreen_simulate {seconds:10} {times:1}";

            public static string Execute(params string[] args)
            {
                var loadingScreen = LoadingScreen.Instance;
                if (loadingScreen == null)
                    return noInstanceMessage;

                float seconds;
                if (args.Length < 1 || !float.TryParse(args[0], out seconds))
                    seconds = 10;

                int times;
                if (args.Length < 2 || !int.TryParse(args[1], out times))
                    times = 1;

                loadingScreen.Simulate(seconds, times);
                return string.Format("Simulation started ({0} seconds x{1})", seconds, times);
            }
        }
    }
}