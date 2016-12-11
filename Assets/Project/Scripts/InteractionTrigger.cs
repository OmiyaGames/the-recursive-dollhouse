using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractionTrigger : IGazed
{
    [SerializeField]
    IGazed interactWith;

    BoxCollider gazeBounds = null;
    bool isGazed = false;

    #region Properties
    BoxCollider GazeBounds
    {
        get
        {
            if(gazeBounds == null)
            {
                gazeBounds = GetComponent<BoxCollider>();
            }
            return gazeBounds;
        }
    }

    public bool IsEnabled
    {
        get
        {
            return GazeBounds.enabled;
        }
        set
        {
            GazeBounds.enabled = value;
            if((value == false) && (isGazed == true))
            {
                OnGazeExit(null);
            }
        }
    }
    #endregion

    public override void OnGazeEnter(Gazer gazer)
    {
        isGazed = true;
        interactWith.OnGazeEnter(gazer);
    }

    public override void OnGazeExit(Gazer gazer)
    {
        isGazed = false;
        interactWith.OnGazeExit(gazer);
    }

    public override void OnInteract(Gazer gazer)
    {
        interactWith.OnInteract(gazer);
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        // Do absolutely nothing
    }
}
