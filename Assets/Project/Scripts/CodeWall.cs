using UnityEngine;

[RequireComponent(typeof(CodeLabel))]
public class CodeWall : TierObject
{
    CodeLabel labelCache = null;

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        if(labelCache == null)
        {
            labelCache = GetComponent<CodeLabel>();
        }
        labelCache.OnTierChanged();
    }
}
