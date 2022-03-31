
using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class GroupList
{
    public string _name = DefaultName;
    [SerializeField] 
    [AllowNesting] [ValidateInput(SetName)]
    private Node _startNode;
    [SerializeField] private Node[] _nodes;
    
    private const string DefaultName = "Set Me";
    private const string SetName = nameof(NameGroup);

    private bool NameGroup()
    {
        if (_startNode.IsNotNull())
        {
            _nodes = new[] {_startNode};
        }
        else
        {
            _nodes = new Node[0];
        }
        _name = _startNode.IsNotNull() ? "Group Staring with : " + _startNode.name : DefaultName;
        return true;
    }

    public Node StartNode
    {
        get => _startNode;
        set => _startNode = value;
    }

    public Node[] GroupNodes
    {
        get => _nodes;
        set => _nodes = value;
    }
}
