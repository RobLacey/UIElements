using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class NodeFunctionBase : IEZEventUser, IMono, IServiceUser
{
    protected NodeFunctionBase(IUiEvents uiEvents)
    {
        _uiEvents = uiEvents;
    }

    protected bool _pointerOver;
    protected bool _isSelected;
    protected bool _isDisabled;
    protected bool _passOver;
    protected IUiEvents _uiEvents;
    protected IDataHub _myDataHub;

    //Properties
    public virtual void AxisMoveDirection(MoveDirection moveDirection) { }
    protected abstract bool CanBeHighlighted();
    protected abstract bool CanBePressed();
    protected virtual bool FunctionNotActive() => _isDisabled && _passOver;
    protected bool MyHubDataIsNull => _myDataHub.IsNull();
    
    //Main
    public virtual void OnAwake() { }

    public virtual void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
        LateStartSetUp();
    }

    public virtual void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    protected virtual void LateStartSetUp()
    {
        if (MyHubDataIsNull) return;
        
        if(_myDataHub.SceneStarted)
        {
            _pointerOver = false;
            _isDisabled = false;
        }    
    }

    public virtual void OnDisable()
    {
        UnObserveEvents();
        _myDataHub = null;
    }

    public virtual void ObserveEvents()
    {
        _uiEvents.WhenPointerOver += SavePointerStatus;
        _uiEvents.IsSelected += SaveIsSelected;
        _uiEvents.IsPressed += ProcessPress;
        _uiEvents.IsDisabled += IsDisabled;
    }

    public virtual void UnObserveEvents()
    {
        if (_uiEvents is null) return;
        _uiEvents.WhenPointerOver -= SavePointerStatus;
        _uiEvents.IsSelected -= SaveIsSelected;
        _uiEvents.IsPressed -= ProcessPress;
        _uiEvents.IsDisabled -= IsDisabled;
    }

    public virtual void OnDestroy()
    {
        UnObserveEvents();
        _myDataHub = null;
    }

    public virtual void OnStart() { }

    protected abstract void SavePointerStatus(bool pointerOver);

    protected virtual void SaveIsSelected(bool isSelected) => _isSelected = isSelected;

    private void IsDisabled(IDisableData disableData)
    {
        _isDisabled = disableData.IsNodeDisabled();
        _passOver = disableData.PassOver();
        ProcessDisabled();
    }
    
     private protected abstract void ProcessPress();
     
     private protected virtual void ProcessDisabled() { }
}
