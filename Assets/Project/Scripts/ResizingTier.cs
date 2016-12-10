using UnityEngine;

public class ResizingTier : MonoBehaviour
{
    [SerializeField]
    int startingTier = 0;

    public int CurrentTier
    {
        get;
        private set;
    }

    // Use this for initialization
    void Awake()
    {
        TierObject[] allObjects = GetComponentsInChildren<TierObject>();
        foreach (TierObject tier in allObjects)
        {
            tier.ParentTier = this;
        }
        CurrentTier = startingTier;
    }

    void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        throw new System.NotImplementedException();
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        throw new System.NotImplementedException();
    }
}
