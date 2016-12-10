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

    void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;

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
        // Update the tier of this object
        embedItem.CurrentTier = ThisTier - 1;
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
