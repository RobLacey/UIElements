using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] 
    [AllowNesting] [OnValueChanged(Check)]
    private Node _presetParent  = default;
    
    //Variables
    private bool _hasParentNode;
    private bool _hasPresetParentNode;
    private INode _parentNode;
    private bool _isPopUp;
    private List<INode> _historicHierarchy = new List<INode>();
    private const string SetName = "Set My Name";

    public void ResetOnNewHotKey()
    {
        _name = SetName;
        _hotKeyInput = HotKey.None;
        _myBranch = null;
    }

    //Properties, Getters / Setters
    public HotKey HotKeyInput => _hotKeyInput;
    public void SetBranchRunTime(Branch branch) => _myBranch = branch;

    //Main
    public void OnEnable() => IsAllowedType();

    public void HotKeyActivation()
    {    
        if(_myBranch.IsNull() || _myBranch.CanvasIsEnabled) return;
        
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
        
        //CheckForInvalidBranch();
        GetImmediateParent();
        
        var currentBranch = (IBranch)_myBranch;
        var parentBranch = _myBranch.MyParentBranch;
        
        while (parentBranch.IsNotNull())
        {
            INode parentNode;
            
            parentNode = ReturnNextParentNode(parentBranch, currentBranch);
            _historicHierarchy.Add(parentNode);
            currentBranch = parentBranch;
            parentBranch = parentBranch.MyParentBranch;
        }
    }

    private INode ReturnNextParentNode(IBranch parentBranch, IBranch currentBranch)
    {
        INode parentNode;
        if (_hasPresetParentNode)
        {
            parentNode = _presetParent;
            _hasPresetParentNode = false;
        }
        else
        {
            parentNode = parentBranch.ThisBranchesNodes
                                     .First(node => ReferenceEquals(currentBranch, node.HasChildBranch));
        }
        return parentNode;
    }

    private bool SetIfIsAPopUpBranch() => _isPopUp = _myBranch.IsAPopUpBranch();

    // private void CheckForInvalidBranch()
    // {
    //     if (_myBranch.MyParentBranch.IsNull())
    //     {
    //         throw new Exception($"{_myBranch} :Hot Key has no parent and isn't a PopUp so is invalid. Make into a POPUP or REMOVE");
    //     }
    //
    //     // if (_presetParent.IsNotNull() && _presetParent.HasChildBranch != (IBranch)_myBranch)
    //     // {
    //     //     throw new Exception($"{_myBranch} HotKey has a preset that isn't linked to it. Set it's child to this hotkey");
    //     // }
    // }

    private void GetImmediateParent()
    {
        if (_presetParent.IsNotNull())
        {
            _parentNode = _presetParent;
            _hasPresetParentNode = true;
            return;
        }
        
        var branchesNodes = _myBranch.MyParentBranch.ThisBranchesNodes;
        _parentNode = branchesNodes.First(node => ReferenceEquals(_myBranch, node.HasChildBranch));
    }

    private void StartThisHotKeyBranch()
    {
        foreach (var node in _historicHierarchy)
        {
            if(node == _parentNode) continue;
            node.SetAsHotKeyParent(false);
        }
        
        if(!_isPopUp)
        {
            _parentNode.SetAsHotKeyParent(true);
        }
        else
        {
            _myBranch.OpenThisBranch();
        }
    }
}
