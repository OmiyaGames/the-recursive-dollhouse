using UnityEngine;
using System.Collections.Generic;

public class DollHouse : TierObject
{
    readonly List<EnterTrigger> growTriggers = new List<EnterTrigger>();
    readonly List<EnterTrigger> shrinkTriggers = new List<EnterTrigger>();

    [SerializeField]
    Transform bottom;
    [SerializeField]
    Transform growPoint;

    // Use this for initialization
    void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;
        Setup(ResizeParent.Instance.CurrentTier);
    }

    public void AssociateWith(EnterTrigger triggerInfo)
    {
        if(triggerInfo.Action == EnterTrigger.Change.Grow)
        {
            growTriggers.Add(triggerInfo);
        }
        else
        {
            shrinkTriggers.Add(triggerInfo);
        }
    }

    public void OnTrigger(EnterTrigger triggerInfo)
    {

    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        throw new System.NotImplementedException();
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        throw new System.NotImplementedException();
    }

    void Setup(int currentTier)
    {

    }
}
