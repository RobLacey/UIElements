using System;
using UnityEngine;

public interface ISizeAndPosition : IPositionScaleTween, IPunchShakeTween { }

public class UISizeAndPosition : NodeFunctionBase, ISizeAndPosition
{
    public UISizeAndPosition(ISizeAndPositionSettings settings, IUiEvents uiEvents): base(uiEvents)
    {
        Scheme = settings.Scheme;
        if(settings.RectTransform != null)
            MyRect = settings.RectTransform;
    }

    //Variables
    private INodeTween _tween;

    //Properties & Set/Getters
    public SizeAndPositionScheme Scheme { get; }
    public bool IsPressed { get; private set; }
    public RectTransform MyRect { get; }
    public Transform MyTransform { get; private set; }
    public Vector3 StartPosition { get; private set; }
    public Vector3 StartSize { get; private set; }
    public string GameObjectID { get; private set; }
    protected override bool CanBeHighlighted() => Scheme.CanBeHighlighted || Scheme.CanBeSelectedAndHighlight;
    protected override bool CanBePressed()  => !Scheme.NotSet && !Scheme.CanBeSelectedAndHighlight;
    protected override bool FunctionNotActive() => _isDisabled || Scheme.NotSet;

    //Main
    public override void OnAwake()
    {
        if(!Scheme || MyRect is null) return;
        base.OnAwake();
        Scheme.OnAwake();
        GameObjectID = _uiEvents.MasterNodeID.ToString();
        SetVariables();
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        IsPressed = false;
    }

    private void SetVariables()
    {
        MyTransform = MyRect.transform;
        StartSize = MyRect.localScale;
        StartPosition = MyRect.anchoredPosition3D;
        _tween = SizeAndPositionFactory.AssignType(Scheme.TweenEffect, this);
    }

    protected override void SaveIsSelected(bool isSelected)
    {
        base.SaveIsSelected(isSelected);
        ProcessPress();
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive()|| !CanBeHighlighted()) return;
        
        if(pointerOver)
        {
            PointerOver();
        }
        else
        {
            PointerNotOver();
        }    
    }

    private void PointerOver()
    {
        if (_isSelected && Scheme.CanBeSelectedAndHighlight || _pointerOver) return;
        _tween.DoTween(IsActive.Yes);
        _pointerOver = true;
    }

    private void PointerNotOver()
    {
        if (_isSelected && Scheme.CanBeSelectedAndHighlight || !_pointerOver) return;
        _tween.DoTween(IsActive.No);
        _pointerOver = false;
    }

    private protected override void ProcessPress()
    { 
        if(FunctionNotActive() || !CanBePressed()) return;
        
        if(Scheme.IsPressed)
        {
            IsPressed = true;
            _tween.DoTween(IsActive.Yes);
            IsPressed = false;
        }
        else if(Scheme.CanBeSelected || Scheme.CanBeSelectedAndHighlight)
        {
            if(_pointerOver) return;
            _tween.DoTween(_isSelected ? IsActive.Yes : IsActive.No);
        }
    }

    private protected override void ProcessDisabled() => _tween.DoTween(IsActive.No);
}
