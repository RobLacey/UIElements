using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITweenInspector
{
    void UpdateInspector();
    ITweenInspector CurrentScheme(TweenScheme scheme);
    ITweenInspector CurrentBuildList(List<BuildTweenData> newBuildList);
}

public class TweenInspector : ITweenInspector
{
    private TweenScheme _lastTweenScheme;
    private List<BuildTweenData> _buildList = new List<BuildTweenData>();
    private TweenScheme _scheme;

    private bool HasBuildList { get; set; }
    private bool HasScheme => _scheme.IsNotNull();

    public ITweenInspector CurrentScheme(TweenScheme scheme)
    {
        _scheme = scheme;
        return this;
    }
    
    public ITweenInspector CurrentBuildList(List<BuildTweenData> newBuildList)
    {
        HasBuildList = false;

        foreach (var buildTweenData in newBuildList)
        {
            if (buildTweenData.Element.IsNotNull())
                HasBuildList = true;
        }

        _buildList = newBuildList;
        return this;
    }
    
    public void UpdateInspector()
    {
        if(!HasBuildList) return;
        
        PassInSchemeToBuildObjects();
        
        if(!HasScheme)
        {
            SchemeHasBeenDeleted();
            return;
        }

        if (HasScheme)
        {
            SchemeHasBeenAdded();
            ConfigureSettings();
        }
    }

    private void PassInSchemeToBuildObjects()
    {
        foreach (var buildObject in _buildList)
        {
            if(buildObject.Element.IsNull()) continue;
            buildObject.SetElement();
        }
    }

    private void SchemeHasBeenDeleted()
    {
        if (_lastTweenScheme.IsNotNull())
            _lastTweenScheme.Unsubscribe(ConfigureSettings);
        _lastTweenScheme = null;
        ClearTweenSettings();
    }

    private void SchemeHasBeenAdded()
    {
        _lastTweenScheme = _scheme;
        _scheme.Subscribe(ConfigureSettings);
    }

    private void ConfigureSettings()
    {
        if(_scheme.IsNull()) return;
        foreach (var buildElement in _buildList)
        {
            if(buildElement.Element.IsNull()) continue;
            buildElement.ActivateTweenSettings(_scheme);
        }
    }
    
    private void ClearTweenSettings()
    {
        if(!HasBuildList) return;
        foreach (var buildObject in _buildList)
        {
            buildObject.ClearSettings(TweenStyle.NoTween);
        }
    }

}
