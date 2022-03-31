using System;
using System.Collections.Generic;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;


[Serializable]
public class WhenAllowed : IServiceUser, IMonoEnable, IMonoDisable
{
    [SerializeField] private AllowBuffer _notOnRootScreen = AllowBuffer.Allow;
    
    [SerializeField] 
    [InfoBox("Applies to Optional & Timed Only")] [AllowNesting] [Label("If Same Type Of PopUps Are Open")]
    private Allow _withSamePopUpType = Allow.Buffer;
    
    [SerializeField] 
    [InfoBox("Resolve Pop Ups disable blocking raycast by default. This stops activation and hides open branches")]
    [AllowNesting] [Label("If Other Resolve PopUps Are Open")]
    private Allow _withActiveResolvePopUps = Allow.Allow;


    private enum Allow
    {
        Allow, Buffer
    }
    private enum AllowBuffer
    {
        Allow, CloseAndBuffer, Close
    }

    private IDataHub _myDataHub;

    public bool RestoreBranch { get; private set; }


    public void OnEnable()
    {
        UseEZServiceLocator();
    }

    public void OnDisable()
    {
        RestoreBranch = false;
    }

    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }


    public bool IsAllowed(Action exitBranch = null)
    {
        if (!_myDataHub.IsAtRoot)
        {
            if (_notOnRootScreen == AllowBuffer.CloseAndBuffer)
            {
                RestoreBranch = true;
                return false;
            }

            if (_notOnRootScreen == AllowBuffer.Close)
            {
                exitBranch?.Invoke();
                return false;
            }
        }
        
        if (_myDataHub.ActiveResolvePopUps.IsNotEmpty() & _withActiveResolvePopUps == Allow.Buffer)
        {
            RestoreBranch = true;
            return false;
        }
        return true;
    }
    
    public bool IsAllowed(List<IBranch> sameType, IBranch thisBranch, Action exitBranch = null)
    {
        if (!IsAllowed(exitBranch)) return false;
        if (CheckForSameType(sameType, thisBranch)) return false;
        return true;
    }
    
    private bool CheckForSameType(List<IBranch> sameType, IBranch thisBranch)
    {
        if (sameType.IsNull() || sameType.Contains(thisBranch)) return false;

        if (sameType.Count > 0 & _withSamePopUpType == Allow.Buffer)
        {
            RestoreBranch = true;
            return true;
        }
        return false;
    }

    public void Restored() => RestoreBranch = false;
}