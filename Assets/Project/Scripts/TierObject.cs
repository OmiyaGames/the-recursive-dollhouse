using UnityEngine;

public abstract class TierObject : MonoBehaviour
{
    public ResizingTier ParentTier
    {
        get;
        set;
    }

    public int ThisTier
    {
        get
        {
            return ParentTier.CurrentTier;
        }
    }
}
