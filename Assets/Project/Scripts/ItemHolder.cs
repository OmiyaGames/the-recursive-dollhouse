using UnityEngine;
using OmiyaGames;
using UnityStandardAssets.Characters.FirstPerson;

public class ItemHolder : IGazed
{
    public const string StateField = "State";

    public enum Type
    {
        Table,
        House,
        Player
    }

    public enum LabelState
    {
        None,
        PickUp,
        Drop
    }

    [SerializeField]
    Type type;
    [SerializeField]
    InventoryItem holdingItem;

    [Header("Required Components")]
    [SerializeField]
    Transform placement;
    [SerializeField]
    SoundEffect placeSoundEffect;

    [Header("Optional Components")]
    [SerializeField]
    Animator labelsAnimation;
    [SerializeField]
    InteractionTrigger trigger;

    bool interactive = false;

    public Type ThisType
    {
        get
        {
            return type;
        }
    }

    public Transform ItemPosition
    {

        get
        {
            return placement;
        }
    }

    public InventoryItem HoldingItem
    {
        get
        {
            return holdingItem;
        }
        set
        {
            if (holdingItem != value)
            {
                if (holdingItem != null)
                {
                    // Reset previous item, if any
                    holdingItem.HeldIn = null;
                }
                holdingItem = value;
                if (holdingItem != null)
                {
                    // Set new item, if any
                    holdingItem.HeldIn = this;
                }
                OnGazeExit(null);
            }
        }
    }

    public int InteractiveTier
    {
        get
        {
            switch (type)
            {
                case Type.Player:
                    return ResizeParent.Instance.CurrentTier;
                case Type.House:
                    return ThisTier - 1;
                default:
                    return ThisTier;
            }
        }
    }

    public bool IsActive
    {
        set
        {
            gameObject.SetActive(value);
            if(trigger != null)
            {
                trigger.IsEnabled = value;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        if (type != Type.Player)
        {
            ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
            ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;
        }
        if (holdingItem != null)
        {
            // Set new item, if any
            holdingItem.HeldIn = this;
        }
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    Vector3 rotationCache;
    void Update()
    {
        if ((interactive == true) && (labelsAnimation != null))
        {
            // Rotate the label to look at the player
            labelsAnimation.transform.LookAt(FirstPersonController.Instance.transform.position);
            rotationCache = labelsAnimation.transform.rotation.eulerAngles;
            rotationCache.x = 0;
            rotationCache.y += 180f;
            rotationCache.z = 0;
            labelsAnimation.transform.rotation = Quaternion.Euler(rotationCache);
        }
    }

    public override void OnGazeEnter(Gazer gazer)
    {
        interactive = false;
        if ((HoldingItem != null) && (gazer.PlayerHolder.HoldingItem == null))
        {
            interactive = true;
            if (labelsAnimation != null)
            {
                labelsAnimation.SetInteger(StateField, (int)LabelState.PickUp);
            }
        }
        else if ((HoldingItem == null) && (gazer.PlayerHolder.HoldingItem != null))
        {
            interactive = true;
            if (labelsAnimation != null)
            {
                labelsAnimation.SetInteger(StateField, (int)LabelState.Drop);
            }
        }
        else if (labelsAnimation != null)
        {
            labelsAnimation.SetInteger(StateField, (int)LabelState.None);
        }
    }

    public override void OnGazeExit(Gazer gazer)
    {
        interactive = false;
        if ((labelsAnimation != null) && (labelsAnimation.gameObject.activeInHierarchy == true))
        {
            labelsAnimation.SetInteger(StateField, (int)LabelState.None);
        }
    }

    public override void OnInteract(Gazer gazer)
    {
        if (interactive == true)
        {
            if (HoldingItem != null)
            {
                TransferItem(this, gazer.PlayerHolder);
                OnGazeExit(gazer);
            }
            else if(gazer.PlayerHolder.HoldingItem != null)
            {
                TransferItem(gazer.PlayerHolder, this);
                OnGazeExit(gazer);
            }
        }
    }

    public static void TransferItem(ItemHolder oldHolder, ItemHolder newHolder)
    {
        // Cache a reference to the item
        InventoryItem itemToTransfer = oldHolder.HoldingItem;

        // For event triggering reasons, set the properties in sequential order,
        // starting with removal
        oldHolder.HoldingItem = null;
        newHolder.HoldingItem = itemToTransfer;
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        // If holding an item, change this item's tier value
        // (or do it automatically from the item's perspective)
        if(HoldingItem != null)
        {
            HoldingItem.OnTierChanged();
        }
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if ((trigger != null) && (gameObject.activeInHierarchy == true))
        {
            trigger.IsEnabled = false;
            OnGazeExit(null);
        }
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if ((trigger != null) && (gameObject.activeInHierarchy == true))
        {
            trigger.IsEnabled = (obj.CurrentTier == InteractiveTier);
        }
    }
}
