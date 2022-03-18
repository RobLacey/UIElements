using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BranchEvents : IBranchEvents
{
    [SerializeField] private UnityEvent _onBranchEnter;
    [SerializeField] private UnityEvent _onBranchExit;
    [SerializeField] private UnityEvent _onBranchEnterEnd;
    [SerializeField] private UnityEvent _onBranchExitEnd;
    
    public void OnBranchEnter() => _onBranchEnter?.Invoke();

    public void OnBranchExit() => _onBranchExit?.Invoke();
    public void OnBranchEnterEnd() => _onBranchEnterEnd?.Invoke();

    public void OnBranchExitEnd() => _onBranchExitEnd?.Invoke();
}

public interface IBranchEvents
{
    void OnBranchEnter();
    void OnBranchEnterEnd();
    void OnBranchExit();
    void OnBranchExitEnd();
}