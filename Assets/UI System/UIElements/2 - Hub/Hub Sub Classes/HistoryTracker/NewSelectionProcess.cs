
using System.Linq;
using UIElements.Hub_Sub_Classes.HistoryTracker;
using UnityEngine;

public static class NewSelectionProcess 
{
    public static void AddNewSelection(SelectData data)
    {
        if (data.History.Contains(data.NewNode))
        {
            ContainsNewNode(data);
            return;
           // return data.History.Count == 0 ? null : data.History.Last();
        }
        DoesntContainNewNode(data);
      //  return data.NewNode;
    }

    private static void ContainsNewNode(SelectData data)
    {
        data.AddStopPoint(data.NewNode);
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
    }

    private static void DoesntContainNewNode(SelectData data)
    {
        
        if(data.History.Count > 0)
        {
            NodeInDifferentBranchButSameTrunk(data);
            //data.NewNode.HasChildBranch.MoveToThisBranch(data.MyDataHub.ActiveBranch);
        }
        
        data.HistoryTracker.UpdateHistoryData(data.NewNode);
        data.History.Add(data.NewNode);
        NavigateToChildBranch(data);
        TrunkTracker.MovingToNewTrunk(data);

        // TrunkTracker.MovingToNewTrunk(data);
        
        // void MoveToChildProcess()
        // {
        //
        //     // Debug.Log(data.LastSelected());
        //     // Debug.Log(data.CurrentTrunk.ActiveBranch.LastSelected);
        //     // data.AddStopPoint(data.CurrentTrunk.ActiveBranch.LastSelected);
        //     // NodeInDifferentBranchButSameTrunk(data);
        //
        //     NavigateToChildBranch(data);
        //     // var newBranch = data.NewNode.MyBranch;
        //     // data.NewNode.HasChildBranch.MoveToThisBranch(newBranch);
        //     //NavigateToChildBranch(data);
        //     data.HistoryTracker.UpdateHistoryData(data.NewNode);
        //     data.History.Add(data.NewNode);
        // }    
    }

    private static void NavigateToChildBranch(SelectData data)
    {
        IBranch thisBranch;
        
        if(data.LastSelected().IsNotNull())
        {
            thisBranch = data.LastSelected().MyBranch;
            thisBranch.StartBranchExitProcess(OutTweenType.MoveToChild, ToChildBranchProcess);
            return;
        }

        thisBranch = data.NewNode.MyBranch;
        ToChildBranchProcess();
        
        void ToChildBranchProcess()
        {
            data.NewNode.HasChildBranch.MoveToThisBranch(thisBranch);
        }
    }

    private static void NodeInDifferentBranchButSameTrunk(SelectData data)
    {
        bool NodeIsInSameBranch() => data.LastSelected().HasChildBranch == data.NewNode.MyBranch;
        bool NodeSameTrunkDifferentBranch() => data.CurrentTrunk == data.NewNodesTrunk;
        bool NodeDifferentTrunk() => data.CurrentTrunk != data.NewNode.MyBranch.ParentTrunk;

        if (NodeIsInSameBranch() /*|| NodeSameTrunk()*/) return;
        
        if (NodeDifferentTrunk())
        {
            data.AddStopPoint(data.NewNode);
        }
        else
        {
            if(NodeSameTrunkDifferentBranch())
            {
                data.SameTrunkButNothingSelected = true;
               // return;
            }
            
            data.AddStopPoint(data.CurrentTrunk.ActiveBranch.LastSelected);
        }
        //  Debug.Log(NodeIsInSameBranch() + " : "    + NodeDifferentTrunk());
        // Debug.Log(data.CurrentTrunk + " : " + data.CurrentTrunk.ActiveBranch + " : " + data.CurrentTrunk.ActiveBranch.LastSelected );
        // Debug.Log(data.LastSelected() + " : " + data.LastSelected().MyBranch + " : " + data.LastSelected().MyBranch.ParentTrunk);
       //data.AddStopPoint(!NodeDifferentTrunk() ? data.CurrentTrunk.ActiveBranch.LastSelected : data.NewNode);
        
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
        data.SameTrunkButNothingSelected = false;
    }
}