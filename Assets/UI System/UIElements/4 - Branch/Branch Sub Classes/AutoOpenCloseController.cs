
using System;
using System.Collections;
using EZ.Events;
using UnityEngine;


public class AutoOpenCloseController: IAutoOpenClose, IEZEventDispatcher, ICancelActivated
{
    public AutoOpenCloseController(IAutoOpenCloseData data)
    {
        _myBranch = data.ThisBranch;
    }
    
    //Variables
    private readonly IBranch _myBranch;

    private static Coroutine runningCoroutine;

    //Properties
    public bool PointerOverBranch { get; private set; }
    public EscapeKey EscapeKeyType => _myBranch.EscapeKeyType;
    public IBranch BranchToCancel => _myBranch;
    public IBranch ChildNodeHasOpenChild { private get; set; }

    //Events
    private  Action<ICancelActivated> CancelHooverOver { get; set; }

    //Main
    public void OnEnable()
    {
        FetchEvents();
       // ObserveEvents();
    }

    public void OnDisable()
    {
      //  UnObserveEvents();
        CancelHooverOver = null;
        StaticCoroutine.StopCoroutines(runningCoroutine);
    }
    
    public void FetchEvents() => CancelHooverOver = CancelEvents.Do.Fetch<ICancelActivated>();

    public void OnPointerEnter()
    {
        if (runningCoroutine.IsNotNull())
        {
            StaticCoroutine.StopCoroutines(runningCoroutine);
        }
        PointerOverBranch = true;
    }

    public void OnPointerExit()
    {
        PointerOverBranch = false;
        
        if(_myBranch.AutoClose == IsActive.No) return;
        
        if (ChildNodeHasOpenChild != null)
        {
            runningCoroutine = StaticCoroutine.StartCoroutine(WaitForPointer());
            return;
        }

        runningCoroutine = StaticCoroutine.StartCoroutine(WaitForPointerNoChild());
    }

    private IEnumerator WaitForPointer()
    {
        yield return new WaitForSeconds(_myBranch.AutoCloseDelay);
        if (!ChildNodeHasOpenChild.PointerOverBranch && !_myBranch.MyParentBranch.PointerOverBranch)
            CloseBranch();
    }

    private IEnumerator WaitForPointerNoChild()
    {
        yield return new WaitForEndOfFrame();
        if (!_myBranch.MyParentBranch.PointerOverBranch)
            CloseBranch();
    }

    private void CloseBranch()
    {
        ChildNodeHasOpenChild = null;
        CancelHooverOver?.Invoke(this);
    }

}

