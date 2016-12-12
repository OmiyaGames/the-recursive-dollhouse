using UnityEngine;

public class ResizingTier : MonoBehaviour, IDelayedSetup
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

                ExtraSetup(ResizeParent.Instance);
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
            ResizeParent.Instance.TierHistory.Add(this);
        }

        // Bind to event
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += ExtraSetup;
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if (IsActive(obj) == true)
        {
            // Parent this to the resize parent
            gameObject.SetActive(true);
            transform.SetParent(obj.transform, true);
        }
        else if((obj.currentDirection == ResizeParent.ResizeDirection.Shrinking) && ((obj.CurrentTier + 2) == CurrentTier))
        {
            // Leave this object visible
            gameObject.SetActive(true);
            transform.SetParent(obj.transform, true);
        }
        else if ((obj.currentDirection == ResizeParent.ResizeDirection.Growing) && ((obj.CurrentTier - 2) == CurrentTier))
        {
            // Leave this object visible
            gameObject.SetActive(true);
            transform.SetParent(obj.transform, true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ExtraSetup(ResizeParent obj)
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
        return (Mathf.Abs(obj.CurrentTier - CurrentTier) <= 1);
    }
}
