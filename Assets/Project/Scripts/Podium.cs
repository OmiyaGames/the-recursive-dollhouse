using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Podium : TierObject
{
    static bool isParentItemsSetup = false;

    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("item")]
    ResizingTier embedItem;

    [Header("Required Components")]
    [SerializeField]
    Transform itemPlacement;

    readonly HashSet<ResizingTier> parentItems = new HashSet<ResizingTier>();

    IEnumerator Start()
    {
        // Add path to embedded doll house
        HashSet<Podium> podiums;
        if (ResizeParent.Instance.AllPodiumsPerTier.TryGetValue(ParentTier, out podiums) == false)
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

        // Wait until everything is setup
        yield return null;

        // Go through all the podiums in the embedded tier
        foreach(Podium childPodium in ResizeParent.Instance.AllPodiumsPerTier[embedItem])
        {
            // Set their parent to this tier
            childPodium.parentItems.Add(ParentTier);
        }
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if (ResizeParent.Instance.TopTierOnStack == ParentTier)
        {
            if (obj.currentDirection == ResizeParent.ResizeDirection.Shrinking)
            {
                ResizingTier parentItemLevel0 = null;
                if(ResizeParent.Instance.TierStack.Count > 2)
                {
                    parentItemLevel0 = ResizeParent.Instance.TierStack[ResizeParent.Instance.TierStack.Count - 2];
                }
                if ((parentItemLevel0 != null) && (parentItems.Contains(parentItemLevel0) == true) && ((ThisTier - 1) == obj.CurrentTier))
                {
                    // Check if this object is only one step larger than the current tier
                    Podium parentPodium = UpdateParentItem(obj, parentItemLevel0, ResizeParent.Instance.ShrinkScaleVector);

                    // Update the parent podum's embedded elements
                    foreach (Podium podium in ResizeParent.Instance.AllPodiumsPerTier[parentPodium.ParentTier])
                    {
                        if (podium != parentPodium)
                        {
                            podium.UpdateEmbedItem(obj);
                        }
                    }

                    // Check if there is at least one more parent above this
                    ResizingTier parentItemLevel1 = null;
                    if (ResizeParent.Instance.TierStack.Count > 3)
                    {
                        parentItemLevel1 = ResizeParent.Instance.TierStack[ResizeParent.Instance.TierStack.Count - 3];
                    }
                    if ((parentItemLevel1 != null) && (parentPodium.parentItems.Contains(parentItemLevel1) == true) && (parentPodium.ThisTier > 0))
                    {
                        // Setup this new parent as well
                        parentPodium.UpdateParentItem(obj, parentItemLevel1, ResizeParent.Instance.GrowScaleVector);

                        // Parent this to the resize parent
                        parentItemLevel1.transform.SetParent(obj.transform, true);
                    }

                    // Parent this to the resize parent
                    parentItemLevel0.transform.SetParent(obj.transform, true);
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
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if (embedItem != null)
        {
            // Un-embed
            embedItem.transform.SetParent(null, true);
        }
    }

    Podium UpdateParentItem(ResizeParent obj, ResizingTier parentItem, Vector3 finalScale)
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
