using UnityEngine;

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

    ResizeParent.TierPair? springPath = null;

    void Start()
    {
        // Add path to embedded doll house
        if (embedItem != null)
        {
            ResizeParent.Instance.PodiumMap.Add(new ResizeParent.TierPair(ParentTier, embedItem), this);
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
                UpdateParentItem(obj);
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

    void UpdateParentItem(ResizeParent obj)
    {
        // Generate the spring's path
        if(springPath == null)
        {
            springPath = new ResizeParent.TierPair(parentItem, ParentTier);
        }

        // Grab the parent podium
        Podium parentPodium;
        if(ResizeParent.Instance.PodiumMap.TryGetValue(springPath.Value, out parentPodium) == true)
        {
            // Setup the resize helper to the proportions of the podium
            ResizeParent.Instance.ResizeHelper.transform.SetParent(parentPodium.itemPlacement, false);
            ResizeParent.Instance.ResizeHelper.transform.localPosition = Vector3.zero;
            ResizeParent.Instance.ResizeHelper.transform.localScale = Vector3.zero;
            ResizeParent.Instance.ResizeHelper.transform.localRotation = Quaternion.identity;

            // Parent the parentTier to the resize helper
            ResizeParent.Instance.ResizeHelper.transform.SetParent(null, true);
            parentItem.transform.SetParent(ResizeParent.Instance.ResizeHelper.transform, true);

            // Position and resize the resize helper
            ResizeParent.Instance.ResizeHelper.transform.localScale = Vector3.one;
            ResizeParent.Instance.ResizeHelper.transform.position = ParentTier.transform.position;

            // Update the tier of this object
            parentItem.CurrentTier = ThisTier - 1;

            // Parent this to the resize parent
            parentItem.transform.SetParent(obj.transform, true);
        }
        else
        {
            throw new System.ArgumentException("No mapping to shrink found!");
        }
        throw new System.NotImplementedException();
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
