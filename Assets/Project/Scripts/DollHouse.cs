﻿using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using System;

public class DollHouse : TierObject
{
    readonly List<EnterTrigger> growTriggers = new List<EnterTrigger>();
    readonly List<EnterTrigger> shrinkTriggers = new List<EnterTrigger>();

    [SerializeField]
    Transform bottom;
    [SerializeField]
    Transform growPoint;
    [SerializeField]
    float offsetOnShrink = 1.5f;

    // Use this for initialization
    void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    public void AssociateWith(EnterTrigger triggerInfo)
    {
        if (triggerInfo.Action == EnterTrigger.Change.Grow)
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
        if (triggerInfo.Action == EnterTrigger.Change.Grow)
        {
            ResizeParent.Instance.Grow(growPoint);
        }
        else
        {
            ResizeParent.Instance.Shrink(growPoint.position + FirstPersonController.Instance.transform.forward * offsetOnShrink);
        }
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        foreach (EnterTrigger trigger in growTriggers)
        {
            trigger.IsEnabled = false;
        }
        foreach (EnterTrigger trigger in shrinkTriggers)
        {
            trigger.IsEnabled = false;
        }
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if (obj.CurrentTier == ThisTier)
        {
            foreach (EnterTrigger trigger in shrinkTriggers)
            {
                trigger.IsEnabled = true;
            }
        }
        else if ((obj.CurrentTier + 1) == ThisTier)
        {
            foreach (EnterTrigger trigger in growTriggers)
            {
                trigger.IsEnabled = true;
            }
        }
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        Instance_OnAfterResize(ResizeParent.Instance);
    }
}
