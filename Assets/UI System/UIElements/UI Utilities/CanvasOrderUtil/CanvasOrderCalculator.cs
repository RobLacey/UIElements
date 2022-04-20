using System.Collections.Generic;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;


public interface ICanvasOrderCalculator : IMonoStart, IMonoEnable
{
    BranchType GetBranchType { get; }
    int GetManualCanvasOrder { get; }
    OrderInCanvas GetOrderInCanvas { get; }
    void SetCanvasOrder();
    void SetFocusCanvasOrder(int canvasOrder);
    void ResetFocus();
    void ResetCanvasOrder();
    void ProcessActiveCanvasses(List<Canvas> activeCanvasList);
}

public class CanvasOrderCalculator: IServiceUser, ICanvasOrderCalculator
{
    public CanvasOrderCalculator(ICanvasCalcParms data)
    {
        _myBranch = data.ThisBranch;
        _myCanvas = data.ThisBranch.MyCanvas;
        GetOrderInCanvas = data.ThisBranch.CanvasOrder;
        GetBranchType = data.ThisBranch.ReturnBranchType;
        if (GetOrderInCanvas == OrderInCanvas.Manual)
        {
            GetManualCanvasOrder = data.ThisBranch.ReturnManualCanvasOrder;
        }
    }
    
    //Variables
    private readonly Canvas _myCanvas;
    private int _startingOrder;
    private int _activeOrder;
    private ICanvasOrderData _canvasOrderData;
    private IDataHub _myDataHub;
    private IBranch _myBranch;
    private bool _overrideSorting;
    private bool _focusActive;

    //Properties
    private IBranch ActiveBranch => _myDataHub.ActiveBranch;
    public BranchType GetBranchType { get; }
    public int GetManualCanvasOrder { get; }
    public OrderInCanvas GetOrderInCanvas { get; }

    //Main

    public void OnEnable() => UseEZServiceLocator();

    public void UseEZServiceLocator()
    {
        _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void OnStart() => SetUpCanvasOrderAtStart();


    private void SetUpCanvasOrderAtStart()
    {
        if (CheckIfSetToDefaultOrder()) return;
        SetStartingSortingOrder(_canvasOrderData.ReturnPresetCanvasOrder(this));
    }

    private bool CheckIfSetToDefaultOrder()
    {
        if (GetOrderInCanvas != OrderInCanvas.Default) return false;

        _myCanvas.enabled = true;
        _startingOrder = _myCanvas.sortingOrder;
        _activeOrder = _startingOrder;
        _myCanvas.overrideSorting = false;
        _myCanvas.enabled = false;
        return true;
    }

    private void SetStartingSortingOrder(int canvasOrder)
    {
        _myCanvas.enabled = true;
        _startingOrder = canvasOrder;
        _activeOrder = _startingOrder;
        _myCanvas.overrideSorting = true;
        _myCanvas.sortingOrder = _startingOrder;
        _myCanvas.enabled = false;
    }

    public void SetCanvasOrder()
    {
        if (_myDataHub.PausedOrEscapeTrunk(_myBranch.ParentTrunk) || _myDataHub.GamePaused)
        {
            SetStartingSortingOrder(_canvasOrderData.ReturnPauseCanvasOrder());
        }

        if(ActiveBranch.IsNull() || _myCanvas.sortingOrder > _startingOrder) return;

        switch (GetOrderInCanvas)
        {
            case OrderInCanvas.InFront:
                _myCanvas.sortingOrder++;
                break;
            case OrderInCanvas.Behind:
                _myCanvas.sortingOrder--;
                break;
            case OrderInCanvas.Manual:
                if (ActiveBranch.CanvasOrder == GetOrderInCanvas)
                {
                    _myCanvas.sortingOrder++;
                }
                break;
        }
    }

    public void SetFocusCanvasOrder(int canvasOrder)
    {
        if(_focusActive) return;
        
        _focusActive = true;
        _myCanvas.overrideSorting = true;
        _myCanvas.sortingOrder += canvasOrder;
    }

    public void ResetFocus()
    {
        if(!_focusActive) return;
        
        _myCanvas.sortingOrder = _activeOrder;
        _focusActive = false;
    }

    public void ResetCanvasOrder()
    {
        _myCanvas.sortingOrder = _startingOrder;
        _activeOrder = _startingOrder;
        _focusActive = false;
    }

    public void ProcessActiveCanvasses(List<Canvas> activeCanvasList)
    {
        for (var index = 0; index < activeCanvasList.Count; index++)
        {
            var canvasses = activeCanvasList[index];
            _activeOrder = _startingOrder + index;
            canvasses.sortingOrder = _activeOrder;
        }
    }
}
