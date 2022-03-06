using System;
using UnityEngine;
using UnityEngine.UI;

public class UISwapImageText : NodeFunctionBase
{
    public UISwapImageText(ISwapImageOrTextSettings settings, IUiEvents uiEvents) : base(uiEvents)
    {
        _changeWhen = settings.ChangeWhen;
        _toggleIsOff = settings.ToggleOff;
        _toggleIsOn = settings.ToggleOn;
        _textToSwap = settings.TextToSwap;
        _changeTextToo = settings.ChangeTextToo;
        _startingText = GetStartingText();
    }

    //variables
    private readonly string _startingText;
    private readonly ChangeWhen _changeWhen;
    private readonly Image _toggleIsOff;
    private readonly Image _toggleIsOn;
    private readonly Text _textToSwap;
    private readonly string _changeTextToo;
    
    //Properties, Getters & Setters
    protected override bool CanBeHighlighted() => _changeWhen == ChangeWhen.OnHighlight;
    protected override bool CanBePressed() => _changeWhen == ChangeWhen.OnPressed;
    private bool ToggleOnNewControls => _changeWhen == ChangeWhen.OnControlChanged;
    private string GetStartingText() => _textToSwap ? _textToSwap.text : String.Empty;
    protected override bool FunctionNotActive() => _isDisabled && _passOver;


    //Main
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        InputEvents.Do.Subscribe<IAllowKeys>(OnControlsChanged);
    }

    public override void UnObserveEvents()
    {
        base.UnObserveEvents();
        InputEvents.Do.Unsubscribe<IAllowKeys>(OnControlsChanged);
    }

    protected override void LateStartSetUp()
    {
        base.LateStartSetUp();
        if (MyHubDataIsNull) return;
        
        SetUp();
    }

    public override void OnStart()
    {
        base.OnStart();
        SetUp();
    }

    private void SetUp()
    {
        if (ToggleOnNewControls)
        {
            CycleToggle(_myDataHub.AllowKeys);
        }
        else
        {
            CycleToggle(_isSelected);
        }
    }

    private void CycleToggle(bool isOn)
    {
        if (isOn)
        {
            ToggleImages(true);
            ToggleText(true);
        }
        else
        {
            ToggleImages(false);  
            ToggleText(false);
        }
    }

    private void OnControlsChanged(IAllowKeys args)
    {
        if (!ToggleOnNewControls || !_uiEvents.ReturnMasterNode.MyBranch.CanvasIsEnabled) return;
        CycleToggle(args.CanAllowKeys);
    }

    private void ToggleText(bool isOn)
    {
        if(_textToSwap is null) return;

        _textToSwap.text = isOn ? _changeTextToo : _startingText;
    }

    private void ToggleImages(bool isOn)
    {
        if(_toggleIsOff is null) return;
        _toggleIsOff.enabled = !isOn;
        _toggleIsOn.enabled = isOn;
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive() || !CanBeHighlighted() || CanBePressed() && _isSelected) return;
        
        if (pointerOver)
        {
            PointerOver();
        }
        else
        {
            PointerNotOver();
        }
    }

    private void PointerOver() => CycleToggle(true);
    private void PointerNotOver() => CycleToggle(false);

    private protected override void ProcessPress()
    {
        if(FunctionNotActive() || !CanBePressed()) return;
        CycleToggle(_isSelected);
    }

    private protected override void ProcessDisabled() { }
}
