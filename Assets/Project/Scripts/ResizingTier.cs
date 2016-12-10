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
            Instance_OnAfterResize(ResizeParent.Instance);
            if (OnCurrentTierChanged != null)
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
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if(Mathf.Abs(obj.CurrentTier - CurrentTier) > 1)
        {
            gameObject.SetActive(false);
        }
        else
        {
            // Parent this to the resize parent
            gameObject.SetActive(true);
            transform.SetParent(obj.transform, true);
        }
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if (Mathf.Abs(obj.CurrentTier - CurrentTier) > 1)
        {
            gameObject.SetActive(false);
        }
        else
        {
            // Un-embed
            gameObject.SetActive(true);
            transform.SetParent(null, true);
        }
    }
}
