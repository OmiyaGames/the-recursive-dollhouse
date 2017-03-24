using UnityEngine;

public abstract class TierObject : MonoBehaviour
{
    ResizingTier parentTier;

    public ResizingTier ParentTier
    {
        get
        {
            return parentTier;
        }
        set
        {
            if (parentTier != null)
            {
                parentTier.OnCurrentTierChanged -= OnThisTierChanged;
            }
            parentTier = value;
            if(parentTier != null)
            {
                parentTier.OnCurrentTierChanged += OnThisTierChanged;
            }
        }
    }

    public abstract void SetTheme(MoodTheme theme);
    protected abstract void OnThisTierChanged(ResizingTier obj);

    public int ThisTier
    {
        get
        {
            return ParentTier.CurrentTier;
        }
    }
}
