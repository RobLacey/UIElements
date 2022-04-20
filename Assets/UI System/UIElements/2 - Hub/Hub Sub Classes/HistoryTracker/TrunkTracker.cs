
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
            
            MoveToNewTrunkProcess(data);
            return true;
        }

        private static void MoveToNewTrunkProcess(HistoryData data)
        {        
            if(data.ScreenTypeOfDestinationTrunk == ScreenType.FullScreen)
            {
                data.NewNode.ExitNodeByType(); //Stops MovingBack Node Flash
            }

           // data.CurrentTrunk.OnMoveToNewTrunk(EndOfAction, data.ScreenTypeOfDestinationTrunk);
             
            /*void EndOfAction() => */CloseOtherOpenTrunksAndMoveToNewTrunk(data);
        }

        private static void CloseOtherOpenTrunksAndMoveToNewTrunk(HistoryData data)
        {
            var otherTrunks = data.ActiveTrunks;
            void MoveToNextTrunk() => data.DestinationTrunk.OnStartTrunk(data.NewNodesBranch);

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
                otherTrunks[i].OnMoveToNewTrunk(null, data.ScreenTypeOfDestinationTrunk);
                // otherTrunks[i].OnMoveToNewTrunk(null, data.ScreenTypeOfDestinationTrunk);
            }

            // var CurrentTrunk = data.NewNode.MyBranch.ParentTrunk;
            // Debug.Log(data.NewNode.MyBranch);
            // if (CurrentTrunk.IsNull()) CurrentTrunk = data.CurrentTrunk;
            data.CurrentTrunk.OnMoveToNewTrunk(moveToNextTrunk, data.ScreenTypeOfDestinationTrunk);
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
                currentNode.MyBranch.ParentTrunk.OnStartTrunk();
            }
            data.CurrentTrunk.OnExitTrunk(EndOfBack);

            // Close();
            //
            // void Close()
            // {
            //     if (currentNode.MyBranch.ParentTrunk == data.ActiveTrunks.Last())
            //     {
            //             currentNode.MyBranch.ParentTrunk.OnStartTrunk();
            //             data.EndOfTrunkCloseAction?.Invoke();
            //     }
            //     else
            //     {
            //         // if(data.CurrentTrunk.TweenBackImmediately)
            //         // {
            //             data.CurrentTrunk.OnExitTrunk(Close);
            //            // Close();
            //         // }
            //         // else
            //         // {
            //         //     data.CurrentTrunk.OnExitTrunk(Close);
            //         // }
            //     }
            // }
        }
    }
}