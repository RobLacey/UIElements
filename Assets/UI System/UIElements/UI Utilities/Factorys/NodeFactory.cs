﻿using System;
using EZ.Inject;
using UnityEngine;

public static class NodeFactory
{
    private static IEZInject EJect { get; } = new EZInject();
    
    public static INodeBase Factory(ButtonFunction functions, INode node)
    {
        switch (functions)
        {
            case ButtonFunction.Standard:
            {
                return CreateStandard(node);
            }
            case ButtonFunction.ToggleGroup:
            {
                return CreateLinkedToggles(node);
            }
            case ButtonFunction.ToggleNotLinked:
            {
                return CreateTogglesNotLinked(node);
            }
            case ButtonFunction.CancelOrBack:
            {
                return CreateCancelOrBack(node);
            }
            case ButtonFunction.InGameUi:
            {
                return CreateInGameUi(node);
            }            
            default:
                throw new Exception("No Node Type Found");
        }
    }

    private static INodeBase CreateStandard(INode node)
    {
        return EJect.WithParams<IStandard>(node);
    }
    private static INodeBase CreateLinkedToggles(INode node)
    {
        node.HasChildBranch = null;
        return EJect.WithParams<IGroupedToggles>(node);
    }
    private static INodeBase CreateTogglesNotLinked(INode node)
    {
        node.HasChildBranch = null;
        return EJect.WithParams<IToggleNotLinked>(node);
    }
    private static INodeBase CreateCancelOrBack(INode node)
    {
        node.HasChildBranch = null;
        return EJect.WithParams<ICancelOrBack>(node);
    }
    
    private static INodeBase CreateInGameUi(INode node)
    {
        return EJect.WithParams<IInGameNode>(node);
    }
}
