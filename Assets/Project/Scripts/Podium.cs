using System;
using UnityEngine;

public class Podium : TierObject
{
    [SerializeField]
    Transform itemPlacement;
    [SerializeField]
    ResizingTier item;

    void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;

        Instance_OnBeforeResize(ResizeParent.Instance);
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if (item != null)
        {
            if ((obj.currentDirection != ResizeParent.ResizeDirection.Shrinking) && (ThisTier == obj.CurrentTier))
            {
                // Check if this object is only one step smaller than the current tier
                EmbedTier(obj);
            }
            else if ((obj.currentDirection != ResizeParent.ResizeDirection.Growing) && (ThisTier == (obj.CurrentTier - 1)))
            {
                // Check if this object is only one step larger than the current tier
                EmbedTier(obj);
            }
        }
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if (item != null)
        {
            // Un-embed
            item.transform.SetParent(null, true);
        }
    }

    void EmbedTier(ResizeParent obj)
    {
        // If so, embed this tier to itemPlacement first
        item.transform.SetParent(itemPlacement, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
        item.transform.localRotation = Quaternion.identity;

        // Update the tier of this object
        item.CurrentTier = ThisTier + 1;

        // Parent this to the resize parent
        item.transform.SetParent(obj.transform, true);
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        // Do nothing
        //throw new NotImplementedException();
    }
}
