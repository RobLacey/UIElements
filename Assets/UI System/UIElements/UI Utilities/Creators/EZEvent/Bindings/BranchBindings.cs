﻿
using EZ.Events;
using UIElements;
using UnityEngine;

public class BranchBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //Branch
        AutoRemove().CreateEvent<IClearScreen>();
        AutoRemove().CreateEvent<ISetUpStartBranches>();
        AutoRemove().CreateEvent<IEndTween>();
        AutoRemove().CreateEvent<ICanInteractWithBranch>();
        AutoRemove().CreateEvent<ICannotInteractWithBranch>();
        AutoRemove().CreateEvent<ICloseBranch>();
    }
}

public interface ISetUpStartBranches 
{
    IBranch StartBranch { get; }
} 

public interface IClearScreen 
{
    IBranch IgnoreThisBranch { get; }
}

public interface IEndTween
{
    RectTransform EndTweenRect { get; }
    TweenScheme Scheme { get; }
}

public interface ICanInteractWithBranch
{
    IBranch MyBranch { get; }
}
public interface ICannotInteractWithBranch
{
    IBranch MyBranch { get; }
}

public interface ICloseBranch
{
    IBranch TargetBranch { get; }
    GOUIModule ReturnGOUIModule { get; }
}



