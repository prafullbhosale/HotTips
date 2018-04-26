﻿using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;

namespace HotTips
{
    public class VSTipHistoryManager : ITipHistoryManager
    {
        private static readonly string SVsSettingsPersistenceManagerGuid = "9B164E40-C3A2-4363-9BC5-EB4039DEF653";
        private static readonly string TIP_OF_THE_DAY_SETTINGS = "TipOfTheDay";
        private static readonly string TIP_HISTORY = "TipHistory";
        private static readonly bool RoamSettings = true;

        private static VSTipHistoryManager _instance;

        private ISettingsManager SettingsManager;
        private List<string> _tipsSeen;

        public static ITipHistoryManager Instance()
        {
            return _instance ?? (_instance = new VSTipHistoryManager());
        }

        public bool HasTipBeenSeen(string globalTipId)
        {
            List<string> tipsSeen = GetTipHistory();
            return tipsSeen != null && tipsSeen.Contains(globalTipId);
        }

        public List<string> GetTipHistory()
        {
            if (_tipsSeen == null) InitialiseTipHistory();
            return _tipsSeen;
        }

        private void InitialiseTipHistory()
        {
            // Note: Must instaniate on UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();
            SettingsManager = (ISettingsManager)ServiceProvider.GlobalProvider.GetService(new Guid(SVsSettingsPersistenceManagerGuid));

            // Pull tip history from VS settings store (Comma separated string of TipIDs)
            string tipHistory = GetTipHistoryFromSettingsStore();
            _tipsSeen = (!String.IsNullOrEmpty(tipHistory)) ? new List<string>(tipHistory.Split(',')) : new List<string>();
        }

        private string GetTipHistoryFromSettingsStore()
        {
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            SettingsManager.TryGetValue(collectionPath, out string tipHistoryRaw);
            return tipHistoryRaw;
        }

        public void MarkTipAsSeen(string globalTipId)
        {
            // Add tip ID to the tip history and persist to the VS settings store
            List<string> tipsSeen = GetTipHistory();
            tipsSeen.Add(globalTipId);

            // Update the VS settings store with the latest tip history
            string tipHistoryRaw = String.Join(",", tipsSeen);
            // Get Writable settings store and update value
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            //ISettingsList settingsList = SettingsManager.GetOrCreateList(collectionPath, isMachineLocal: !RoamSettings);
            SettingsManager.SetValueAsync(collectionPath, tipHistoryRaw, isMachineLocal: !RoamSettings);
        }

        public void ClearTipHistory()
        {
            // Clear the local tips object
            _tipsSeen = null;

            // Delete the entries from the Settings Store by setting to Empty string
            string collectionPath = TIP_OF_THE_DAY_SETTINGS;
            SettingsManager.SetValueAsync(collectionPath, string.Empty, isMachineLocal: !RoamSettings);

        }
    }
}