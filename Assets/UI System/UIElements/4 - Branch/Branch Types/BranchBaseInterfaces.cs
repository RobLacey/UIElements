using EZ.Inject;
using UIElements;

public interface IBranchBase : IParameters, IMono
{
    void SetUpGOUIBranch(IGOUIModule module);
    IGOUIModule ReturnGOUIModule();
    //void SetUpAsTabBranch();
    void SetUpBranch(IBranch newParentController = null);
    void EndOfBranchStart();
    void StartBranchExit();
    void EndOfBranchExit();
    void SetCanvas(ActiveCanvas active);
    void SetBlockRaycast(BlockRaycast active);
    bool CanStartBranch();
    bool DontExitBranch(OutTweenType outTweenType);
    void SetFocus(int focusSortingOrder);
    void ResetFocus();
}

public interface IBranchParams
{
    IBranch ThisBranch { get; }
}

public interface ICanvasCalcParms : IParameters
{
    IBranch ThisBranch { get; }
}

public interface ISetPositionParms : IParameters
{
    IBranch ThisBranch { get; }
}

