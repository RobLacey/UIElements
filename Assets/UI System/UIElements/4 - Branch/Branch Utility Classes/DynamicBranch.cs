using System.Linq;
using EZ.Service;

public static class DynamicBranch
{
    public static void AddNodeToBranch(IDynamicBranch branch)
    {
        var noNodes = branch.ThisBranchesNodes.Length == 0;
        
        branch.SetThisGroupsNode(BranchChildNodeUtil.GetChildNodes(branch.ThisBranch));
        
        if (noNodes)
        {
            branch.DefaultStartOnThisNode = branch.ThisBranchesNodes.First();
            branch.LastHighlighted = branch.DefaultStartOnThisNode;
            branch.LastSelected = branch.DefaultStartOnThisNode;
        }
    }

    public static void RemoveNodeFromBranch(IDynamicBranch branch, INode nodeToRemove)
    {
        branch.SetThisGroupsNode(BranchChildNodeUtil.RemoveNode(branch.ThisBranchesNodes, nodeToRemove));

        if (branch.ThisBranchesNodes.Length == 0 && !branch.IsInGameBranch())
        {
            //TODO Add Functionality to move the current active switcher to the next object as this group is empty. Need to check switcher is active and focused
            //EZService.Locator.LateGet<IHistoryTrack>().ReturnToNextHomeGroup();
            return;
        }

        SetValuesToNextNode(branch, nodeToRemove);
    }

    private static void SetValuesToNextNode(IDynamicBranch branch, INode nodeToRemove)
    {
        if(branch.ThisBranchesNodes.Length == 0) return;
        
        if (branch.LastHighlighted == nodeToRemove && branch.GetSaveOnExit == IsActive.Yes)
        {
            branch.ThisBranchesNodes.Last().SetNodeAsActive();
        }

        if (branch.LastSelected == nodeToRemove)
        {
            branch.LastSelected = branch.ThisBranchesNodes.Last();
        }
    }

}