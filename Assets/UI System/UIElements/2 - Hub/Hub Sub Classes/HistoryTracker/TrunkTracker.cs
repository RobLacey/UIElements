
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIElements.Hub_Sub_Classes.HistoryTracker
{
    public static class TrunkTracker
    {
        public static bool MovingToNewTrunk(HistoryData data)
        {
            if (data.DestinationTrunk == data.CurrentTrunk) return false;
            // if (data.DestinationTrunk.IsNull()) return false;
            
            CloseOtherOpenTrunksAndMoveToNewTrunk(data);
            return true;
        }

        // private static void MoveToNewTrunkProcess(HistoryData data)
        // {        
        //
        //    // data.CurrentTrunk.OnMoveToNewTrunk(EndOfAction, data.ScreenTypeOfDestinationTrunk);
        //      
        //     /*void EndOfAction() => */CloseOtherOpenTrunksAndMoveToNewTrunk(data);
        // }

        private static void CloseOtherOpenTrunksAndMoveToNewTrunk(HistoryData data)
        {
            var otherTrunks = data.ActiveTrunks;
            void MoveToNextTrunk()
            {
                if(data.ScreenTypeOfDestinationTrunk == ScreenType.FullScreen)
                {
                    data.NewNode.ExitNodeByType(); //Stops MovingBack Node Flash
                }
                data.DestinationTrunk.SetNewSwitcherIndex(data.NewNode.HasChildBranch.LastHighlighted);
                data.DestinationTrunk.OnStartTrunk(data.NewNodesBranch);
            }

            //if(IfNoOtherOpenTrunks(otherTrunks, MoveToNextTrunk)) return;
            
            CloseOtherOpenTrunks(data, otherTrunks, MoveToNextTrunk);
        }

        // private static bool IfNoOtherOpenTrunks(List<Trunk> otherTrunks, Action moveToNewTrunk)
        // {
        //     //TODO IS this reached or used
        //     // if (otherTrunks.Count != 0) return false;
        //     if (otherTrunks.Count != 0) return false;
        //     
        //     moveToNewTrunk.Invoke();
        //     return true;
        // }

        private static void CloseOtherOpenTrunks(HistoryData data, List<Trunk> otherTrunks, Action moveToNextTrunk)
        {
            for (var i = otherTrunks.Count - 1; i >= 1; i--)
            {
                if(otherTrunks[i] == data.NewNode.MyBranch.ParentTrunk) continue;
                OnMoveToNewTrunk(data.ScreenTypeOfDestinationTrunk, otherTrunks[i], null);
            }

            OnMoveToNewTrunk(data.ScreenTypeOfDestinationTrunk, data.CurrentTrunk, moveToNextTrunk);
        }

        private static void OnMoveToNewTrunk(ScreenType destinationScreenType, Trunk trunk, Action moveToNextTrunk)
        {
            bool toFullScreenTrunk = destinationScreenType == ScreenType.FullScreen || trunk.ScreenType == ScreenType.FullScreen;
            
            trunk.SetCurrentMoveTypeToMoveToNext();

            if(toFullScreenTrunk)
            {
                CloseTrunkForFullScreen(trunk, moveToNextTrunk);
            }
            else
            {
                moveToNextTrunk?.Invoke();
            }
        }
        
        private static void CloseTrunkForFullScreen(Trunk trunk, Action endOfMoveAction)
        {
            void CloseTrunk() => trunk.OnExitTrunk(endOfMoveAction, false);

            if (trunk.SwitcherHistory.IsNotEmpty())
            {
                CloseOpenBranchesInSwitchHistory(trunk, CloseTrunk);
            }
            else
            {
                CloseTrunk();
            }
        }

        private static void CloseOpenBranchesInSwitchHistory(Trunk trunk, Action closeTrunk)
        {
            OutTweenType outTweenType = OutTweenType.MoveToChild;
            var lastNodeInHistory = trunk.SwitcherHistory.Last();
            
            if (trunk.ForceClear)
                outTweenType = OutTweenType.Cancel;
            
            foreach (var node in trunk.SwitcherHistory)
            {
                if (node == lastNodeInHistory) continue;
                if (trunk.GroupsBranches.Contains(node.MyBranch)) continue;

                node.MyBranch.ExitThisBranch(outTweenType);
            }

            if (trunk.GroupsBranches.Contains(lastNodeInHistory.MyBranch))
            {
                closeTrunk?.Invoke();
                return;
            }
            lastNodeInHistory.MyBranch.ExitThisBranch(outTweenType, closeTrunk);
        }


        public static bool MoveBackATrunk(HistoryData data, INode currentNode)
        {
            //Debug.Log($"{currentNode} : {currentNode.MyBranch.ParentTrunk} : {data.CurrentTrunk}");
            if (NoTrunkToGoBackTo(data, currentNode)) return false;
           // if (CheckIfGoingBackAnThenOnToANewTrunk(data)) return true;
            
            BackCloseProcess(data, currentNode);
            return true;
        }

        private static bool NoTrunkToGoBackTo(HistoryData data, INode currentNode)
        {
            return currentNode.MyBranch.ParentTrunk == data.CurrentTrunk
                   || data.CurrentTrunk             == data.RootTrunk; /*currentNode.MyBranch.ParentTrunk.IsNull();*/
        }

        //Lets MoveToNewTrunkManage the exit of the trunks as it was getting double called
        // private static bool CheckIfGoingBackAnThenOnToANewTrunk(HistoryData data)
        // {
        //     var hasFullscreenDestination = data.DestinationTrunk.IsNotNull() &&
        //                                   data.ScreenTypeOfDestinationTrunk == ScreenType.FullScreen;
        //     
        //     return data.TweenType == OutTweenType.MoveToChild & hasFullscreenDestination;
        // }

        private static void BackCloseProcess(HistoryData data, INode currentNode)
        {
            void EndOfBack()
            {
                currentNode.ExitNodeByType();
                currentNode.MyBranch.ParentTrunk.OnStartTrunk();
            }
            data.CurrentTrunk.SetCurrentMoveTypeToMoveToBack();
            data.CurrentTrunk.OnExitTrunk(EndOfBack);
        }
    }
}