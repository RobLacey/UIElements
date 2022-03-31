using System.Linq;

namespace UIElements.Input_System
{
    public static class MultiSelectSystem
    {
        public static void MultiSelectPressed(HistoryData data)
        {
            IsInAnotherGroup(data);
            AlreadyInMultiSelect(data);
            AddToMultiSelect(data);
        }

        private static void IsInAnotherGroup(HistoryData data)
        {
            if (Check_InSameGroup(data)) return;
            data.SetToThisTrunkWhenFinished(data.CurrentTrunk);
            HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
            //data.ClearHistory();
            ClearMultiSelect(data);
        }

        private static bool Check_InSameGroup(HistoryData data)
        {
            return data.History.Any(node => node.MultiSelectSettings.AllowMultiSelect == IsActive.Yes & 
                                            node.MultiSelectSettings.MultiSelectGroup == data.NewNode.MultiSelectSettings.MultiSelectGroup);
        }

        private static bool AlreadyInMultiSelect(HistoryData data)
        {
            var newNode = data.NewNode;
            if (data.History.Contains(newNode))
            {
                RemoveFromMultiSelectHistory(data);
                return true;
            }
            return false;
        }
        
        private static void AddToMultiSelect(HistoryData data)
        {
            //data.History.Add(data.NewNode);
            data.AddToHistory(data.NewNode);
            data.MultiSelectOn();
        }

        public static void RemoveFromMultiSelectHistory(HistoryData data)
        {
            if (data.History.Count == 0)
            {
                ClearMultiSelect(data);
            }
        }

        public static void ClearMultiSelect(HistoryData historyData) => historyData.MultiSelectOff();
    }
}