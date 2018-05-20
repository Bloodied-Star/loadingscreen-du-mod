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
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Serialization;

namespace LoadingScreen.Components
{
    /// <summary>
    /// Retrieves active quest messages.
    /// </summary>
    public class QuestsMessages : LoadingScreenComponent
    {
        const string defaultquestMessage = "Taverns are the best place to seek work.";
        string questMessage;
        bool loadingSave;

        #region Public Methods

        public QuestsMessages(Rect rect)
            :base(rect, 5)
        {
            this.style.wordWrap = true;
        }

        /// <summary>
        /// Show QuestMessage on screen with OnGUI
        /// </summary>
        public override void Draw()
        {
            GUI.Label(rect, questMessage, style);
        }

        /// <summary>
        /// Show one quest message from serialized quests.
        /// </summary>
        public override void OnLoadingScreen(SaveData_v1 saveData)
        {
            loadingSave = true;

            questMessage = GetQuestMessage();
        }

        /// <summary>
        /// Show one quest message from active quests.
        /// </summary>
        public override void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            loadingSave = false;

            if (string.IsNullOrEmpty(questMessage = GetQuestMessage()))
                questMessage = defaultquestMessage;
        }

        public override void UpdateScreen()
        {
            if (loadingSave && string.IsNullOrEmpty(questMessage))
                questMessage = GetQuestMessage();
        }

        public override void OnEndScreen()
        {
            if (loadingSave && string.IsNullOrEmpty(questMessage) &&
                string.IsNullOrEmpty(questMessage = GetQuestMessage()))
                questMessage = defaultquestMessage;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get one quest message from active quests.
        /// </summary>
        private static string GetQuestMessage()
        {
            if (QuestMachine.Instance.QuestCount < 1)
                return null;

            // Get quest messages
            List<Message> questMessages = QuestMachine.Instance.GetAllQuestLogMessages();
            if (questMessages.Count == 0)
                return null;

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
