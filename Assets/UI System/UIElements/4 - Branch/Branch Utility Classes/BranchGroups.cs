using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Service;
using UIElements;
using UnityEngine;

[Obsolete("Has been made obsolete by using Homegroup for everything", true)]
public class BranchGroups /*: ISwitch, IServiceUser, IMonoEnable*/
{
    public BranchGroups(IBranch branch)
    {
        _thisBranch = branch;
    }

    //Variables
    private int _index;
    private IDataHub _myDataHub;
    private readonly IBranch _thisBranch;

    //Properties
    /*
    public bool HasOnlyOneMember => ThisBranchesGroup.Count == 1;
    private List<GroupList> ThisBranchesGroup => _thisBranch.BranchGroupsList;
    private List<IBranch> HomeBranches => _myDataHub.ActiveTrunkGroup;
    private bool CanSwitch => ThisBranchesGroup.Count > 0;

    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        SetGroupIndex(_thisBranch.DefaultStartOnThisNode);
    }
    
    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void SetGroupIndex(INode defaultStartPosition)
    {
        if(_thisBranch.ScreenType != ScreenType.FullScreen) return;
        
        _index = 0;

        if(ThisBranchesGroup.Count == 0) return;
        
        foreach (var branchGroup in ThisBranchesGroup)
        {
            if (branchGroup.GroupNodes.Any(node => ReferenceEquals(node, defaultStartPosition)))
            {
                return;
            }
            _index++;
        }
    }
    
    //TODO Remove this class

    public void AddControlBarToBranchGroup()
    {
        if(_thisBranch.ScreenType != ScreenType.FullScreen) return;
        
        bool hasControlBar = HomeBranches.Any(homeBranch => homeBranch.IsControlBar());
        if(!hasControlBar) return;
        
        ThisBranchesGroupIfEmpty();
        AddControlBarAsNewGroup();
    }

    private void ThisBranchesGroupIfEmpty()
    {
        if (ThisBranchesGroup.Count == 0)
            ThisBranchesGroup.Add(GroupList(_thisBranch));
    }

    private void AddControlBarAsNewGroup()
    {
        foreach (var homeBranch in HomeBranches.Where(homeBranch => homeBranch.IsControlBar()))
        {
            ThisBranchesGroup.Add(GroupList(homeBranch));
            return;
        }
    }

    private static GroupList GroupList(IBranch branch)
    {
        var newGroup = new GroupList
        {
            StartNode = (UINode) branch.DefaultStartOnThisNode,
            GroupNodes = new UINode[branch.ThisBranchesNodes.Length]
        };
        newGroup.GroupNodes = branch.ThisBranchesNodes.Cast<UINode>().ToArray();
        return newGroup;
    }


    public void DoSwitch(SwitchInputType switchInputType)
    {
        if(_thisBranch.ScreenType != ScreenType.FullScreen) return;

        Debug.Log("Switch : " + switchInputType);
        
        if(!CanSwitch) return;
        
        switch (switchInputType)
        {
            case SwitchInputType.Positive:
                _index = _index.PositiveIterate(ThisBranchesGroup.Count);
                break;
            case SwitchInputType.Negative:
                _index = _index.NegativeIterate(ThisBranchesGroup.Count);
                break;
            case SwitchInputType.Activate:
                Debug.Log("Here");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(switchInputType), switchInputType, null);
        }
        Debug.Log(ThisBranchesGroup[_index].StartNode);
        ThisBranchesGroup[_index].StartNode.SetNodeAsActive();
    }
    */

}
