using EZ.Service;
using UIElements;
using UnityEngine;

public interface IStandardBranch : IBranchBase { }

public class StandardBranch : BranchBase, IStandardBranch
{
    public StandardBranch(IBranch branch) : base(branch) { }
    
    //Variables
    private ICanvasOrderData _canvasOrderData;
    
    private bool IsControlBar => ThisBranch.IsControlBar();
    
    public override void UseEZServiceLocator()
    {
        base.UseEZServiceLocator();
        _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        HistoryEvents.Do.Subscribe<IOnStart>(SetUpOnStart);
    }

    public override void UnObserveEvents()
    {
        base.UnObserveEvents();
        HistoryEvents.Do.Unsubscribe<IOnStart>(SetUpOnStart);
    }

    public override void OnStart()
    {
        base.OnStart();
        SetCanvas(ActiveCanvas.No);
        SetControlBarCanvasOrder();
    }

    private void SetUpOnStart(IOnStart args)
    {
        if(_myCanvas.enabled)
            SetBlockRaycast(BlockRaycast.Yes);
    }

    private void SetControlBarCanvasOrder()
    {
        if(!IsControlBar) return;
        
        ThisBranch.MyCanvas.overrideSorting = true;
        ThisBranch.MyCanvas.sortingOrder = _canvasOrderData.ReturnControlBarCanvasOrder();
    }

    public override void SetUpBranch(/*IBranch newParentController = null*/)
    {
        base.SetUpBranch(/*newParentController*/);
        
        if(!IsControlBar)
            _canvasOrderCalculator.SetCanvasOrder();
        
        if( IsControlBar || ThisBranch.IsAlreadyActive)
        {
            ThisBranch.DoNotTween();
        }        
        SetCanvas(ActiveCanvas.Yes);
        
       // SetNewParentBranch(newParentController);
    }

    // private void SetNewParentBranch(IBranch newParentController) 
    // {
    //     if(newParentController is null) return;
    //     ThisBranch.MyParentBranch = newParentController;
    // }
    
    public override void SetBlockRaycast(BlockRaycast active) 
        => base.SetBlockRaycast(IsControlBar ? BlockRaycast.Yes: active);

    public override void SetCanvas(ActiveCanvas active) => base.SetCanvas(IsControlBar ? ActiveCanvas.Yes: active);
}
