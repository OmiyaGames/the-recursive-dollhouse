using UnityEngine;

[RequireComponent(typeof(CodeWall))]
public class CodeLabel : PrintedCode
{
    CodeWall wallCache = null;

    public override int ThisTier
    {
        get
        {
            if(wallCache == null)
            {
                wallCache = GetComponent<CodeWall>();
            }
            return wallCache.ThisTier;
        }
    }
}
