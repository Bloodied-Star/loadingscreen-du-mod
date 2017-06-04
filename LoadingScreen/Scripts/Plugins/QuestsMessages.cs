// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Serialization;

/*
 * TODO
 * - Serialized quests. 
 */

namespace LoadingScreen.Plugins
{
    /// <summary>
    /// Retrieves active quest messages.
    /// </summary>
    public static class QuestsMessages
    {
        /// <summary>
        /// Get one quest message from serialized quests.
        /// </summary>
        public static string GetQuestMessage(SaveData_v1 saveData)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get one quest message from active quests.
        /// </summary>
        public static string GetQuestMessage()
        {
            // Get quest messages
            List<Message> questMessages = QuestMachine.Instance.GetAllQuestLogMessages();
            if (questMessages.Count == 0)
                return string.Empty;

            // Choose one quest message
            Random.InitState((int)Time.time);
            Message message = questMessages[Random.Range(0, questMessages.Count)];

            // Get text
            return GetTextFromMessage(message);
        }

        /// <summary>
        /// Get a readable string from message tokens.
        /// </summary>
        public static string GetTextFromMessage(Message message)
        {
            var text = message.GetTextTokens().Where(x => x.formatting == TextFile.Formatting.Text).Select(x => x.text);
            return string.Join("\n", text.ToArray());
        }
    }
}
