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
using DaggerfallWorkshop.Game;

/*
 * TODO
 * - Serialized quests. 
 */

namespace LoadingScreen.Plugins
{
    /// <summary>
    /// Retrieves active quest messages.
    /// </summary>
    public class QuestsMessages : LoadingScreenPlugin
    {
        Rect rect;
        GUIStyle style;
        string questMessage;

        #region Public Methods

        public QuestsMessages(Rect rect, GUIStyle style)
        {
            this.rect = rect;
            this.style = style;
        }

        /// <summary>
        /// Show QuestMessage on screen with OnGUI
        /// </summary>
        public override void Draw()
        {
            GUI.Box(rect, questMessage, style);
        }

        /// <summary>
        /// Show one quest message from serialized quests.
        /// </summary>
        public override void OnLoadingScreen(SaveData_v1 saveData)
        {
            questMessage = GetQuestMessage(saveData);
        }

        /// <summary>
        /// Show one quest message from active quests.
        /// </summary>
        public override void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            questMessage = GetQuestMessage();
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Get one quest message from serialized quests.
        /// </summary>
        private static string GetQuestMessage(SaveData_v1 saveData)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get one quest message from active quests.
        /// </summary>
        private static string GetQuestMessage()
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
        private static string GetTextFromMessage(Message message)
        {
            var text = message.GetTextTokens().Where(x => x.formatting == TextFile.Formatting.Text).Select(x => x.text);
            return string.Join("\n", text.ToArray());
        }
        
        #endregion
    }
}
