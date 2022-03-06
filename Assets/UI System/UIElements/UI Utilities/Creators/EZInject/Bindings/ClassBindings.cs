﻿using EZ.Inject;
using UIElements;
using UIElements.Input_System;

public class ClassBindings : EZInjectBindingBase
{
    public ClassBindings()
    {
        _injectMaster = new EZInjectMaster();
        BindAllObjects();
    }

    protected sealed override void BindAllObjects()
    {
        if(CheckIfAlreadyBound()) return;
        //Base
        _injectMaster.Bind<EZInject>().To<IEZInject>();
        
        //Hub Classes
        _injectMaster.Bind<UIAudioManager>().To<IAudioService>().WithParameters();
        _injectMaster.Bind<SwitchTrunkGroup>().To<ISwitchTrunkGroup>().WithParameters();
        _injectMaster.Bind<HistoryTracker>().To<IHistoryTrack>();
        _injectMaster.Bind<UICancel>().To<ICancel>();
        _injectMaster.Bind<ReturnControlFromEditor>().To<IReturnFromEditor>();
        _injectMaster.Bind<SwitchGroups>().To<ISwitchGroup>();

        //Tweens
        _injectMaster.Bind<PositionTween>().To<IPositionTween>();
        _injectMaster.Bind<RotateTween>().To<IRotationTween>();
        _injectMaster.Bind<ScaleTween>().To<IScaleTween>();
        _injectMaster.Bind<FadeTween>().To<IFadeTween>();
        _injectMaster.Bind<PunchTween>().To<IPunchTween>();
        _injectMaster.Bind<ShakeTween>().To<IShakeTween>();
        _injectMaster.Bind<TweenInspector>().To<ITweenInspector>();
        
        //ToolTips
        _injectMaster.Bind<ToolTipFade>().To<IToolTipFade>().WithParameters();
        _injectMaster.Bind<GetScreenPosition>().To<IGetScreenPosition>().WithParameters();
        _injectMaster.Bind<ToolTipsCalcs>().To<IToolTipCalcs>().WithParameters();
        
        //NodeBase Types
        _injectMaster.Bind<Standard>().To<IStandard>().WithParameters();
        _injectMaster.Bind<CancelOrBackButton>().To<ICancelOrBack>().WithParameters();
        _injectMaster.Bind<GroupedToggles>().To<IGroupedToggles>().WithParameters();
        _injectMaster.Bind<ToggleNotLinked>().To<IToggleNotLinked>().WithParameters();
        _injectMaster.Bind<NodeDisabling>().To<IDisabledNode>().WithParameters();
        _injectMaster.Bind<InGameNode>().To<IInGameNode>().WithParameters();
        
        //Branch Types
        //_injectMaster.Bind<HomeScreenBranch>().To<IHomeScreenBranch>().WithParameters();
        _injectMaster.Bind<StandardBranch>().To<IStandardBranch>().WithParameters();
        _injectMaster.Bind<ResolvePopUp>().To<IResolvePopUpBranch>().WithParameters();
        _injectMaster.Bind<OptionalPopUpPopUp>().To<IOptionalPopUpBranch>().WithParameters();
        _injectMaster.Bind<TimedPopUp>().To<ITimedPopUpBranch>().WithParameters();
        _injectMaster.Bind<PauseMenu>().To<IPauseBranch>().WithParameters();
        _injectMaster.Bind<GOUIBranch>().To<IGOUIBranch>().WithParameters();
        _injectMaster.Bind<SetScreenPosition>().To<ISetScreenPosition>().WithParameters();
        
        //NodeTweens
        _injectMaster.Bind<Position>().To<IPosition>().WithParameters();
        _injectMaster.Bind<Scale>().To<IScale>().WithParameters();
        _injectMaster.Bind<Punch>().To<IPunch>().WithParameters();
        _injectMaster.Bind<Shake>().To<IShake>().WithParameters();
        
        // //NodeSearch
        // _injectMaster.Bind<NodeSearch>().To<INodeSearch>();
        
        //ScreenData
        _injectMaster.Bind<ScreenData>().To<IScreenData>().WithParameters();
        
        //Input Classes
        _injectMaster.Bind<MenuAndGameSwitching>().To<IMenuAndGameSwitching>();
        _injectMaster.Bind<ChangeControl>().To<IChangeControl>().WithParameters();
        
        //InGameControl
        _injectMaster.Bind<_2DRaycast>().To<I2DRaycast>().WithParameters();
        _injectMaster.Bind<_3DRaycast>().To<I3DRaycast>().WithParameters();
        _injectMaster.Bind<GOUISwitcher>().To<IGOUISwitcher>();
        _injectMaster.Bind<VcInteractionUI>().To<IInteractWithUi>();
        _injectMaster.Bind<MoveVirtualCursor>().To<IMoveVirtualCursor>();
        _injectMaster.Bind<VirtualCursor>().To<IVirtualCursor>().WithParameters();
        
        //AutoOpenClose
        _injectMaster.Bind<AutoOpenCloseController>().To<IAutoOpenClose>().WithParameters();
        _injectMaster.Bind<DelayTimer>().To<IDelayTimer>();
        
        //CanvasCalculator
        _injectMaster.Bind<CanvasOrderCalculator>().To<ICanvasOrderCalculator>().WithParameters();
        
    }
}