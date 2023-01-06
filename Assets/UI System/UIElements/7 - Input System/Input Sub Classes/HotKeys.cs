using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public partial class HotKeys : IMonoEnable, IServiceUser, IMonoStart
{
    [SerializeField]
    private string _name = SetName;
    [SerializeField]
    private HotKey _hotKeyInput  = HotKey.None;
    [SerializeField] 
    [AllowNesting] [OnValueChanged(IsAllowed)]
    private Branch _myBranch  = default;
    [FormerlySerializedAs("parentNode")]
    [FormerlySerializedAs("_presetParent")]
    [SerializeField] 
    
    [AllowNesting] [OnValueChanged(Check)] 
    private Node _parentNode  = default;
    
    //Variables
   // private bool _hasParentNode;
    private bool _hasPresetParentNode;
    //private INode _parentNode;
    private bool _isPopUp;
    private List<INode> _historicHierarchy = new List<INode>();
    private IDataHub _myDataHub;
    private const string SetName = "Set My Name";
    private List<IBranch> _branchesHierachy = new List<IBranch>();


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
    public void OnEnable()
    {
        UseEZServiceLocator();
        IsAllowedType();
    }
    
    public void OnStart()
    {
        FindParentNodes();
    }

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void HotKeyActivation()
    {
        if(_myBranch.IsNull() || ReferenceEquals(_myDataHub.ActiveBranch, _myBranch)) return;
        
        StartThisHotKeyBranch();
    }

    private void FindParentNodes()
    {
        if (SetIfIsAPopUpBranch()) return;
        
        var currentBranch = _parentNode.MyBranch;
        var parentBranch = _parentNode.MyBranch.MyParentBranch;
        INode parentNode = _parentNode;
        
        while (parentBranch.IsNotNull())
        {
            parentNode = ReturnNextParentNode(parentBranch, currentBranch);
            _historicHierarchy.Add(parentNode);
            Debug.Log($"{_myBranch} : Added to history {parentNode}");
            currentBranch = parentBranch;
            parentBranch = parentBranch.MyParentBranch;
        }
        
        SetUpHierchyBranchList();
    }

    private void SetUpHierchyBranchList()
    {
        foreach (var node in _historicHierarchy)
        {
            if(node.HasChildBranch.ParentTrunk != _myBranch.ParentTrunk)
            {
                Debug.Log($"Add : {node.HasChildBranch}");
                _branchesHierachy.Add(node.HasChildBranch);
            }        
        }
    }

    private INode ReturnNextParentNode(IBranch parentBranch, IBranch currentBranch)
    {
            return parentBranch.ThisBranchesNodes
                                     .First(node => ReferenceEquals(currentBranch, node.HasChildBranch));
    }

    private bool SetIfIsAPopUpBranch() => _isPopUp = _myBranch.IsAPopUpBranch();

    private void StartThisHotKeyBranch()
    {
        
        Debug.Log($"Hot Key : {_myBranch} : {_myBranch.MyParentBranch.LastSelected}");
        if(!_isPopUp)
        {
            if (!_myDataHub.NoHistory && _myDataHub.History.Last() == _myBranch.MyParentBranch.LastSelected)
            {
                //TODO Fix the Immediate parent issue when pressing the hotkey not from Root
                Debug.Log("Already Open");
               // _myBranch.OpenThisBranch();
                return;
            }
            //_parentNode.SetAsHotKeyParent(true);
            if (_myDataHub.History.Contains(_parentNode))
            {
                Debug.Log($"Contains : {_myDataHub.History[_myDataHub.History.IndexOf(_parentNode) +1]}");
                _myDataHub.History[_myDataHub.History.IndexOf(_parentNode)+1].SetAsHotKeyParent(true);
            }
            else
            {
                //TODO Can use keys after return from full screen. Fix first.
                //TODO Descide on a way to do this either : 
                //TODO a) As is below and add hieachy (Can be a little messy). Need an end event when hot key branch is
                //TODO    finished to activate nodes rather than when this is activated. Could assign to it here. Could have sound setting here too
                // TODO  so don't use activate in Node just SetAsSelectedNoEffect.  
                //TODO B) No hierachy just opens like a Pop Up and returns to current Trunk after close (would need a sound setting to be added)
                HotkeyPressed?.Invoke(_branchesHierachy);
                
                Debug.Log("Normal");
                for (var index = _historicHierarchy.Count - 1; index >= 0; index--)
                {
                    var node = _historicHierarchy[index];
                    if (node == _parentNode || _myDataHub.History.Contains(node)) continue;
                    node.SetAsHotKeyParent(false);
                }
                
               _parentNode.SetAsHotKeyParent(true);
               //_myBranch.OpenThisBranch();
            }
        }
        else
        {
            _myBranch.OpenThisBranch();
            //_parentNode.NodeSelected();
        }
    }
    
    public static Action<List<IBranch>> HotkeyPressed { get; set; }

}
