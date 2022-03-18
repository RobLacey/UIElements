
using System;
using System.Collections.Generic;

namespace UIElements.Hub_Sub_Classes.HistoryTracker
{
    public static class TrunkTracker
    {
        public static bool MovingToNewTrunk(SelectData data)
        {
            if (data.DestinationTrunk.IsNull()) return false;
            
            MoveToNewTrunkProcess(data);
            return true;
        }

        private static void MoveToNewTrunkProcess(SelectData data)
        {        
            if(data.ScreenTypeOfDestinationTrunk == ScreenType.FullScreen)
            {
                data.NewNode.ExitNodeByType(); //Stops MovingBack Node Flash
            }

            data.CurrentTrunk.OnMoveToNewTrunk(EndOfAction, data.ScreenTypeOfDestinationTrunk);
             
            void EndOfAction() => CloseOtherOpenTrunksAndMoveToNewTrunk(data);
        }

        private static void CloseOtherOpenTrunksAndMoveToNewTrunk(SelectData data)
        {
            var otherTrunks = data.MyDataHub.ActiveTrunks;

            if(IfNoOtherOpenTrunks(otherTrunks, MoveToNextTrunk)) return;
            
            CloseOtherOpenTrunks(data, otherTrunks, MoveToNextTrunk);

            void MoveToNextTrunk() => data.DestinationTrunk.OnStartTrunk(data.NewNodesBranch);
        }

        private static bool IfNoOtherOpenTrunks(List<Trunk> otherTrunks, Action moveToNewTrunk)
        {
            if (otherTrunks.Count != 0) return false;
            
            moveToNewTrunk.Invoke();
            return true;
        }

        private static void CloseOtherOpenTrunks(SelectData data, List<Trunk> otherTrunks, Action moveToNextTrunk)
        {
            for (var i = otherTrunks.Count - 1; i >= 1; i--)
            {
                otherTrunks[i].OnMoveToNewTrunk(null, data.ScreenTypeOfDestinationTrunk);
            }
            otherTrunks[0].OnMoveToNewTrunk(moveToNextTrunk, data.ScreenTypeOfDestinationTrunk);
        }

        public static bool MoveBackATrunk(SelectData data, INode currentNode)
        {
            if (NoTrunkToGoBackTo(data, currentNode)) return false;

            if (CheckIfGoingBackAnThenOnToANewTrunk(data)) return true;
            
            BackCloseProcess(data, currentNode);
            return true;
        }

        private static bool NoTrunkToGoBackTo(SelectData data, INode currentNode)
        {
            return currentNode.MyBranch.ParentTrunk == data.CurrentTrunk
                   || currentNode.MyBranch.ParentTrunk.IsNull();
        }

        //Lets MoveToNewTrunkManage the exit of the trunks as it was getting double called
        private static bool CheckIfGoingBackAnThenOnToANewTrunk(SelectData data)
        {
            var hasFullscreenDestination = data.DestinationTrunk.IsNotNull() &&
                                          data.ScreenTypeOfDestinationTrunk == ScreenType.FullScreen;
            
            return data.TweenType == OutTweenType.MoveToChild & hasFullscreenDestination;
        }

        private static void BackCloseProcess(SelectData data, INode currentNode)
        {
            data.CurrentTrunk.OnExitTrunk(EndOfAction);

            void EndOfAction() => currentNode.MyBranch.ParentTrunk.OnStartTrunk();
        }
    }
}