using UnityEngine;

public class ResizingTier : MonoBehaviour
{
    [SerializeField]
    int startingTier = 0;

    public event System.Action<ResizingTier> OnCurrentTierChanged;

    public int CurrentTier
    {
        get
        {
            return startingTier;
        }
        set
        {
            startingTier = value;
            if(OnCurrentTierChanged != null)
            {
                OnCurrentTierChanged(this);
            }
        }
    }

    // Use this for initialization
    void Awake()
    {
        TierObject[] allObjects = GetComponentsInChildren<TierObject>();
        foreach (TierObject tier in allObjects)
        {
            tier.ParentTier = this;
        }
    }

    void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        // Parent this to the resize parent
        transform.SetParent(obj.transform, true);
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        // Un-embed
        transform.SetParent(null, true);
    }
}
