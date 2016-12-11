using UnityEngine;

[DisallowMultipleComponent]
public class InventoryItem : PrintedCode
{
    public event System.Action<InventoryItem> OnAnimationEnd;

    static readonly Vector3 TargetPosition = Vector3.zero;
    static readonly Vector3 TargetScale = Vector3.one;
    static readonly Quaternion TargetRotation = Quaternion.identity;

    [Header("Key variables")]
    [SerializeField]
    ParticleSystem interactiveIndicator;
    [SerializeField]
    float speed;
    [Header("Debug variables (DON'T CHANGE!!!)")]
    [SerializeField]
    ItemHolder heldIn;

    bool animate = false;
    Vector3 velocity;
    PrintedCode cacheCode;

    public ItemHolder HeldIn
    {
        get
        {
            return heldIn;
        }
        set
        {
            heldIn = value;
            if (heldIn != null)
            {
                // Update parent
                transform.SetParent(heldIn.ItemPosition, true);

                // Update particle effect
                OnTierChanged();

                // Reset animation
                velocity.x = 0;
                velocity.y = 0;
                velocity.z = 0;
                animate = true;
            }
        }
    }

    public override int ThisTier
    {
        get
        {
            int returnTier = -1;
            if(HeldIn != null)
            {
                returnTier = HeldIn.InteractiveTier;
            }
            return returnTier;
        }
    }

    protected override void Instance_OnBeforeResize(ResizeParent obj)
    {
        // Call base function
        base.Instance_OnBeforeResize(obj);

        if((interactiveIndicator != null) && (HeldIn != null))
        {
            if((obj.CurrentTier == HeldIn.InteractiveTier) && (HeldIn.ThisType != ItemHolder.Type.Player))
            {
                interactiveIndicator.Play();
            }
            else
            {
                interactiveIndicator.Stop();
            }
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        if(animate == true)
        {
            base.Update();

            if (transform.localPosition.sqrMagnitude < ResizeParent.Instance.SnapDistance)
            {
                // Flag end of animation
                animate = false;

                // Snap everything
                transform.localPosition = TargetPosition;
                transform.localScale = TargetScale;
                transform.localRotation = TargetRotation;

                // Run the animation end event
                if(OnAnimationEnd != null)
                {
                    OnAnimationEnd(this);
                }
            }
            else
            {
                transform.localPosition = Vector3.Slerp(transform.localPosition, TargetPosition, (Time.deltaTime * speed));
                transform.localScale = Vector3.Slerp(transform.localScale, TargetScale, (Time.deltaTime * speed));
                transform.localRotation = Quaternion.Slerp(transform.localRotation, TargetRotation, (Time.deltaTime * speed));
            }
        }
    }
}
