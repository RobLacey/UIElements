using System;
using UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

public class RuntimeCreateBranch
{
    private CreateNewObjects _binGetter = new CreateNewObjects();

    public IBranch CreateGOUI(Branch template)
    {
        var parent = _binGetter.MakeGOUIBin(HubTransform());
        return CreateBranch(template, parent);
    }
        
    public IBranch CreatePopUp(Branch template, Transform parent)
    {
        return CreateBranch(template, parent);
    }
    
    public IBranch CreatePopUp(Branch template)
    {
        var parent = _binGetter.MakePopUpBin(HubTransform());
        return CreateBranch(template, parent);
    }
    
    private Transform HubTransform()
    {
        //TODO Change This as wont work in new system
        return Object.FindObjectOfType<Hub>().transform;
    }

    //Main
    private IBranch CreateBranch(Branch template, Transform createParent)
    {
        IBranch newBranch = template;
            
        if(template.ThisBranchesGameObject.GetIsAPrefab())
        {
            newBranch = Object.Instantiate(template, createParent);
        }
        else
        {
            newBranch.ThisBranchesGameObject.transform.SetParent(createParent, false);
        }
        
        return newBranch;
    }

}