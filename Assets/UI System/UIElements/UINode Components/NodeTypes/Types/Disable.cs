using System;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;


[Serializable]
public class NodeDisabling : IDisabledNode, IEZEventDispatcher, IServiceUser, IMonoEnable, IMonoDisable
{

    [SerializeField] private IsActive _passOverIfDisabled = IsActive.Yes;
    private Disabled _isDisabled = Disabled.No;
    private IDataHub _myDataHub;
    private INode[] _thisBranchesNodes;

    //Events
    private Action<IDisabledNode> ThisIsDisabled { get; set; }

    //Properties
    public INode ThisNode { get; private set; }
    public bool PassOver() => _passOverIfDisabled == IsActive.Yes;
    public Disabled IsDisabled
    {
        get => _isDisabled;
        set
        {
            _isDisabled = value;
            DisableProcess();
        }
    }

    //Main

    public void OnAwake(INode parentNode)
    {
        ThisNode = parentNode;
        _thisBranchesNodes = ThisNode.MyBranch.ThisBranchesNodes;
    }

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
    }

    public void OnDisable()
    {
        _isDisabled = Disabled.No;
        ThisIsDisabled = null;
    }

    public void FetchEvents() => ThisIsDisabled = HistoryEvents.Do.Fetch<IDisabledNode>();

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);


    private void DisableProcess()
    {
        if (_isDisabled == Disabled.No) return;

        ThisIsDisabled?.Invoke(this);
        if (_myDataHub.Highlighted == ThisNode && PassOver())
            FindNextFreeNode();
    }

    public void FindNextFreeNode()
    {
        Debug.Log(_passOverIfDisabled);
        INode freeNode = null;
        
        foreach (var node in _thisBranchesNodes)
        {
            if(node.IsNodeDisabled()) continue;
            freeNode = node;
            break;
        }

        if (freeNode.IsNotNull())
        {
            freeNode.SetNodeAsActive();
        }
        else
        {
            //Switch HomeGroup or better message
            Debug.Log("Nothing is free use disable pass through instead");
        }
    }

    // private INode NextFree()
    // {
    //     int count;
    //     int startIndex = Array.IndexOf(_thisBranchesNodes, ThisNode);
    //
    //     if (_thisBranchesNodes[startIndex] == ThisNode || _thisBranchesNodes[startIndex].IsNodeDisabled())
    //     {
    //         count++;
    //         
    //     }
    // }
}


