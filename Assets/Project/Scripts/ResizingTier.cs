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
            if (startingTier != value)
            {
                startingTier = value;

                // Prevent the tiers from going below 0
                if (startingTier < 0)
                {
                    startingTier = 0;
                }

                Instance_OnAfterResize(ResizeParent.Instance);
                if (OnCurrentTierChanged != null)
                {
                    OnCurrentTierChanged(this);
                }
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
        // Need to add this tier into all the lists
        ResizeParent.Instance.AllTiers.Add(this);

        // Add this tier to the stack if this is the first tier
        if (startingTier == 0)
        {
            ResizeParent.Instance.TierStack.Push(this);
        }

        // Bind to event
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;

        // Setup this tier
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if (IsActive(obj) == true)
        {
            // Parent this to the resize parent
            gameObject.SetActive(true);
            transform.SetParent(obj.transform, true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if (IsActive(obj) == true)
        {
            // Un-embed
            gameObject.SetActive(true);
            transform.SetParent(null, true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    bool IsActive(ResizeParent obj)
    {
        bool returnFlag = false;
        if(obj.CurrentTier == CurrentTier)
        {
            // Make only this tier visible
            returnFlag = true; //(ResizeParent.Instance.TierStack.Peek() == this);
        }
        else if (Mathf.Abs(obj.CurrentTier - CurrentTier) <= 1)
        {
            returnFlag = true;
        }
        return returnFlag;
    }
}
