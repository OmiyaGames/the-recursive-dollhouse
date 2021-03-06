﻿using UnityEngine;
using System.Collections.Generic;

public class Podium : TierObject, IDelayedSetup
{
    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("item")]
    ResizingTier embedItem;

    [Header("Required Components")]
    [SerializeField]
    Transform itemPlacement;
    [SerializeField]
    MeshRenderer tableRenderer;
    [SerializeField]
    bool rotateItemRandomly = true;

    readonly HashSet<ResizingTier> parentItems = new HashSet<ResizingTier>();

    void Start()
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

        // Rotate the item placement (for variety's sake)
        if(rotateItemRandomly == true)
        {
            itemPlacement.Rotate(0f, Random.Range(0f, 360f), 0f);
        }

        // Bind to events
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(embedItem != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, embedItem.transform.position);
        }
    }
#endif

    public override void SetTheme(MoodTheme theme)
    {
        tableRenderer.sharedMaterial = theme.WallMaterial;
    }


    public void ExtraSetup(ResizeParent obj)
    {
        // Do embedded setup
        Instance_OnBeforeResize(obj);
        Instance_OnAfterResize(obj);

        // Update parents
        if ((embedItem != null) && (obj.AllPodiumsPerTier.ContainsKey(embedItem) == true))
        {
            // Go through all the podiums in the embedded tier
            foreach (Podium childPodium in obj.AllPodiumsPerTier[embedItem])
            {
                // Set their parent to this tier
                childPodium.parentItems.Add(ParentTier);
            }
        }
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if (ResizeParent.Instance.LatestTier == ParentTier)
        {
            if (obj.currentDirection == ResizeParent.ResizeDirection.Shrinking)
            {
                if ((ThisTier - 1) == obj.CurrentTier)
                {
                    // Setup one parent up
                    ResizingTier parentItemLevel0 = SetupParent(obj, this, 2);
                    ResizingTier parentItemLevel1 = null;
                    if (parentItemLevel0 != null)
                    {
                        // Grab the next parent up
                        Podium parentPodium = UpdateParentItem(obj, parentItemLevel0, ResizeParent.Instance.ShrinkScaleVector);

                        // Setup the 2nd level parent
                        parentItemLevel1 = SetupParent(obj, parentPodium, 3);
                    }

                    // Parent this to the resize parent
                    if(parentItemLevel0 != null)
                    {
                        parentItemLevel0.transform.SetParent(obj.transform, true);
                    }
                    if (parentItemLevel1 != null)
                    {
                        parentItemLevel1.transform.SetParent(obj.transform, true);
                    }
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

    static ResizingTier SetupParent(ResizeParent obj, Podium parentPodium, int stackIndexFromTop)
    {
        // Check if there is at least one more parent above this
        ResizingTier parentItem = null;
        if (ResizeParent.Instance.TierHistory.Count >= stackIndexFromTop)
        {
            parentItem = ResizeParent.Instance.TierHistory[ResizeParent.Instance.TierHistory.Count - stackIndexFromTop];
        }
        if ((parentItem != null) && (parentPodium.parentItems.Contains(parentItem) == true) && (parentPodium.ThisTier > 0))
        {
            // Setup this new parent as well
            parentPodium.UpdateParentItem(obj, parentItem, ResizeParent.Instance.GrowScaleVector);

            // Update the parent podum's embedded elements
            foreach (Podium podium in ResizeParent.Instance.AllPodiumsPerTier[parentPodium.ParentTier])
            {
                if (podium != parentPodium)
                {
                    podium.UpdateEmbedItem(obj);
                }
            }
        }
        return parentItem;
    }
}
