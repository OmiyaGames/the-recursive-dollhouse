using UnityEngine;

[RequireComponent(typeof(TierObject))]
public class CodeLabel : PrintedCode
{
    TierObject wallCache = null;

    public override int ThisTier
    {
        get
        {
            if (wallCache == null)
            {
                wallCache = GetComponent<TierObject>();
            }
            return wallCache.ThisTier - 1;
        }
    }
}
