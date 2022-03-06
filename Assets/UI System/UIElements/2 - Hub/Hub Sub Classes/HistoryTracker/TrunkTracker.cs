using System;
using UnityEngine;

namespace UIElements.Hub_Sub_Classes.HistoryTracker
{
    public static class TrunkTracker
    {
        public static void MovingToNewTrunk(SelectData data)
        {
            if (data.NewNodesTrunk == data.CurrentTrunk) return;
            
            CloseProcess(data);
        }

        private static void CloseProcess(SelectData data)
        {
            var newTrunksScreenType = data.NewNodesTrunk.ScreenType;
            data.CurrentTrunk.OnExitTrunk(EndOfAction, newTrunksScreenType);

            void EndOfAction()
            {
                data.NewNodesTrunk.OnStartTrunk();
            }
        }

        public static bool MoveBackATrunk(SelectData data, Action endOfBackAction)
        {
            if (data.MoveBackToTrunk != data.CurrentTrunk)
            {
                BackCloseProcess(data, endOfBackAction);
                return true;
            }
            //endOfBackAction();
            return false;
        }

        private static void BackCloseProcess(SelectData data, Action endOfBackAction)
        {
            data.CurrentTrunk.OnExitTrunk(EndOfAction);

            void EndOfAction()
            {
                data.MoveBackToTrunk.OnStartTrunk();
                endOfBackAction.Invoke();
            }

        }
    }
}