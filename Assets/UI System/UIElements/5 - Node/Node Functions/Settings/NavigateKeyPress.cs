using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class NavigateKeyPress
{
    [SerializeField] private NavPressMoveType _moveType = NavPressMoveType.None;
    [SerializeField] [AllowNesting] [HideIf("Hide")] private Node _navigate = default;

    private bool Hide() => _moveType != NavPressMoveType.Navigate;

    public NavPressMoveType MoveType => _moveType;
    public Node Navigate => _navigate;
}