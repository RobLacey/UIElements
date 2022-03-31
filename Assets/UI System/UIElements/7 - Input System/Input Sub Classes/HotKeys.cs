using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

[Serializable]
public partial class HotKeys : IMonoEnable
{
    [SerializeField]
    private string _name = SetName;
    [SerializeField]
    private HotKey _hotKeyInput  = HotKey.None;
    [SerializeField] 
    [AllowNesting] [OnValueChanged(IsAllowed)]
    private Branch _myBranch  = default;
    
    //Variables
    private bool _hasParentNode;
    private INode _parentNode;
    private bool _isPopUp;
    private List<INode> HistoricHierachy = new List<INode>();
    private const string SetName = "Set My Name";

    public void ResetOnNewHotKey()
    {
        _name = SetName;
        _hotKeyInput = HotKey.None;
        _myBranch = null;
    }

    //Properties, Getters / Setters
    public HotKey HotKeyInput => _hotKeyInput;

    //Main
    public void OnEnable() => IsAllowedType();

    public void HotKeyActivation()
    {    
        if(!_hasParentNode)
            GetParentNode();
        
        StartThisHotKeyBranch();
    }

    private void GetParentNode()
    {
        FindParentNodes();
        _hasParentNode = true;
    }

    private void FindParentNodes()
    {
        if (SetIfIsAPopUpBranch()) return;
        
        CheckForInvalidBranch();
        GetHistoricParents();
        
        var currentBranch = (IBranch)_myBranch;
        var parentBranch = _myBranch.MyParentBranch;
        
        while (parentBranch.IsNotNull())
        {
            var parentNode = parentBranch.ThisBranchesNodes
                                    .First(node => ReferenceEquals(currentBranch, node.HasChildBranch));
            HistoricHierachy.Add(parentNode);
            currentBranch = parentBranch;
            parentBranch = parentBranch.MyParentBranch;
        }
    }

    private bool SetIfIsAPopUpBranch() => _isPopUp = _myBranch.IsAPopUpBranch();

    private void CheckForInvalidBranch()
    {
        if (_myBranch.MyParentBranch.IsNull())
        {
            throw new Exception("Hot Key has no parent and isn't a PopUp so is invalid. Make into a POPUP or REMOVE");
        }
    }

    private void GetHistoricParents()
    {
        var branchesNodes = _myBranch.MyParentBranch.ThisBranchesNodes;
        _parentNode = branchesNodes.First(node => ReferenceEquals(_myBranch, node.HasChildBranch));
    }

    private void StartThisHotKeyBranch()
    {
        foreach (var node in HistoricHierachy)
        {
            if(node == _parentNode) continue;
            node.SetAsHotKeyParent(false);
        }
        
        if(!_isPopUp)
            _parentNode.SetAsHotKeyParent(true);
        _myBranch.OpenThisBranch();
    }
}
