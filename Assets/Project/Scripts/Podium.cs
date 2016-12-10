using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Podium : TierObject
{
    [SerializeField]
    ResizingTier parentItem;
    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("item")]
    ResizingTier embedItem;

    [Header("Required Components")]
    [SerializeField]
    Transform itemPlacement;

    void Start()
    {
        // Add path to embedded doll house
        HashSet<Podium> podiums;
        if(ResizeParent.Instance.AllPodiumsPerTier.TryGetValue(ParentTier, out podiums) == false)
        {
            podiums = new HashSet<Podium>();
            ResizeParent.Instance.AllPodiumsPerTier.Add(ParentTier, podiums);
        }
        podiums.Add(this);
        if (embedItem != null)
        {
            ResizeParent.Instance.PathToPodiumMap.Add(new ResizeParent.TierPath(ParentTier, embedItem), this);
        }

        // Bind to events
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;

        // Do embedded setup
        Instance_OnBeforeResize(ResizeParent.Instance);
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if (obj.currentDirection == ResizeParent.ResizeDirection.Shrinking)
        {
            if ((parentItem != null) && ((ThisTier - 1) == obj.CurrentTier))
            {
                // Check if this object is only one step larger than the current tier
                //StartCoroutine(UpdateParentItem());
                Podium parentPodium = UpdateParentItem(obj, ResizeParent.Instance.ShrinkScaleVector);

                // Update the parent podum's embedded elements
                foreach (Podium podium in ResizeParent.Instance.AllPodiumsPerTier[parentPodium.ParentTier])
                {
                    if (podium != parentPodium)
                    {
                        podium.UpdateEmbedItem(obj);
                    }
                }

                // Check if there is at least one more parent above this
                if ((parentPodium.parentItem != null) && (parentPodium.ThisTier > 0))
                {
                    // Setup this new parent as well
                    parentPodium.UpdateParentItem(obj, ResizeParent.Instance.GrowScaleVector);

                    // Parent this to the resize parent
                    parentPodium.parentItem.transform.SetParent(obj.transform, true);
                }

                // Parent this to the resize parent
                parentItem.transform.SetParent(obj.transform, true);
            }
        }
        else
        {
            if ((embedItem != null) && (ThisTier == obj.CurrentTier))
            {
                // Check if this object is only one step smaller than the current tier
                UpdateEmbedItem(obj);
            }
        }
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if (embedItem != null)
        {
            // Un-embed
            embedItem.transform.SetParent(null, true);
        }
    }

    Podium UpdateParentItem(ResizeParent obj, Vector3 finalScale)
    {
        // Grab the parent podium
        Podium parentPodium = ResizeParent.Instance.PathToPodiumMap[new ResizeParent.TierPath(parentItem, ParentTier)];

        // Reset the size of the parent item
        parentItem.transform.SetParent(null, true);
        parentItem.transform.localScale = Vector3.one;

        // Setup the resize helper to the proportions of the podium
        ResizeParent.Instance.ResizeHelper.transform.SetParent(parentPodium.itemPlacement, false);
        ResizeParent.Instance.ResizeHelper.transform.localPosition = Vector3.zero;
        ResizeParent.Instance.ResizeHelper.transform.localScale = Vector3.one;
        ResizeParent.Instance.ResizeHelper.transform.localRotation = Quaternion.identity;

        // Parent the parentTier to the resize helper
        ResizeParent.Instance.ResizeHelper.transform.SetParent(null, true);
        parentItem.transform.SetParent(ResizeParent.Instance.ResizeHelper.transform, true);

        // Position and resize the resize helper
        ResizeParent.Instance.ResizeHelper.transform.localScale = finalScale;
        ResizeParent.Instance.ResizeHelper.transform.position = ParentTier.transform.position;

        // Update the tier of this object
        parentItem.CurrentTier = ThisTier - 1;

        // De-parent
        parentItem.transform.SetParent(null, true);
        return parentPodium;
    }

    void UpdateEmbedItem(ResizeParent obj)
    {
        // If so, embed this tier to itemPlacement first
        embedItem.transform.SetParent(itemPlacement, false);
        embedItem.transform.localPosition = Vector3.zero;
        embedItem.transform.localScale = Vector3.one;
        embedItem.transform.localRotation = Quaternion.identity;

        // Update the tier of this object
        embedItem.CurrentTier = ThisTier + 1;

        // Parent this to the resize parent
        embedItem.transform.SetParent(obj.transform, true);
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        // Do nothing
        //throw new NotImplementedException();
    }
}
