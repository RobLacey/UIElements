using System;
using UIElements;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BranchEvents : IBranchEvents, IMonoDisable
{
    [SerializeField] private UnityEvent _onBranchMouseEnter;
    [SerializeField] private UnityEvent _onBranchMouseExit;
    [SerializeField] private UnityEvent _onOpenBranch;
    [SerializeField] private UnityEvent _onExitBranch;
    [SerializeField] private UnityEvent _onOpenBranchEnd;
    [SerializeField] private UnityEvent _onExitBranchEnd;

    private IBranch _myParentBranch;
    public void SetUpClass(IBranch parentBranch)
    {
        parentBranch.OpenBranchStartEvent += OnBranchEnter;
        parentBranch.OpenBranchEndEvent += OnBranchExit;
        parentBranch.ExitBranchStartEvent += OnBranchEnterEnd;
        parentBranch.ExitBranchEndEvent += OnBranchExitEnd;
        parentBranch.OnMouseEnterEvent += OnMouseEnter;
        parentBranch.OnMouseExitEvent += OnMouseExit;
        _myParentBranch = parentBranch;
    }
    
    public void OnDisable()
    {
        _myParentBranch.OpenBranchStartEvent -= OnBranchEnter;
        _myParentBranch.OpenBranchEndEvent -= OnBranchExit;
        _myParentBranch.ExitBranchStartEvent -= OnBranchEnterEnd;
        _myParentBranch.ExitBranchEndEvent -= OnBranchExitEnd;
        _myParentBranch.OnMouseEnterEvent -= OnMouseEnter;
        _myParentBranch.OnMouseExitEvent -= OnMouseExit;
    }

    public void OnMouseEnter() => _onBranchMouseEnter?.Invoke();
    public void OnMouseExit() => _onBranchMouseExit?.Invoke();
    public void OnBranchEnter() => _onOpenBranch?.Invoke();

    public void OnBranchExit() => _onExitBranch?.Invoke();
    public void OnBranchEnterEnd() => _onOpenBranchEnd?.Invoke();

    public void OnBranchExitEnd() => _onExitBranchEnd?.Invoke();
}

public interface IBranchEvents
{
    void SetUpClass(IBranch parentBranch);
    void OnMouseEnter();
    void OnMouseExit();
    void OnBranchEnter();
    void OnBranchEnterEnd();
    void OnBranchExit();
    void OnBranchExitEnd();
}