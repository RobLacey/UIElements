using System.Collections.Generic;
using System.Linq;
using EZ.Service;
using UnityEngine;

namespace UIElements.Input_System
{
    public static class MultiSelectSystem
    {
        public static void MultiSelectPressed(SelectData data)
        {
            IsInAnotherGroup(data);
            AlreadyInMultiSelect(data);
            AddToMultiSelect(data);
        }

        private static void IsInAnotherGroup(SelectData data)
        {
            if (Check_InSameGroup(data)) return;
            HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
            data.HistoryTracker.UpdateHistoryData(null);
            ClearMultiSelect(data);
        }

        private static bool Check_InSameGroup(SelectData data)
        {
            return data.History.Any(node => node.MultiSelectSettings.AllowMultiSelect == IsActive.Yes & 
                                            node.MultiSelectSettings.MultiSelectGroup == data.NewNode.MultiSelectSettings.MultiSelectGroup);
        }

        private static bool AlreadyInMultiSelect(SelectData data)
        {
            var newNode = data.NewNode;
            if (data.History.Contains(newNode))
            {
                RemoveFromMultiSelectHistory(data);
                return true;
            }
            return false;
        }
        
        private static void AddToMultiSelect(SelectData data)
        {
            data.SetCurrentGroup(data.NewNode.MultiSelectSettings.MultiSelectGroup);
            data.History.Add(data.NewNode);
            data.HistoryTracker.UpdateHistoryData(data.NewNode);
            data.MultiSelectOn();
        }

        public static void RemoveFromMultiSelectHistory(SelectData data)
        {
            HistoryListManagement.CloseThisLevel(data, data.NewNode);
            if (data.History.Count == 0)
            {
                ClearMultiSelect(data);
            }
        }

        public static void ClearMultiSelect(SelectData selectData)
        {
            selectData.MultiSelectOff();
            selectData.SetCurrentGroup(MultiSelectGroup.None);
        }
    }
}