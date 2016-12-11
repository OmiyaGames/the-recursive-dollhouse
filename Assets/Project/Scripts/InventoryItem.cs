using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PrintedCode))]
public class InventoryItem : MonoBehaviour
{
    public event System.Action<InventoryItem> OnAnimationEnd;

    static readonly Vector3 TargetPosition = Vector3.zero;
    static readonly Vector3 TargetScale = Vector3.one;
    static readonly Quaternion TargetRotation = Quaternion.identity;

    [SerializeField]
    ParticleSystem interactiveIndicator;
    [SerializeField]
    float speed;
    [Header("Debug variables (DON'T CHANGE!!!)")]
    [SerializeField]
    ItemHolder heldIn;

    bool animate = false;
    Vector3 velocity;

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

    void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        OnTierChanged();
    }

    public void OnTierChanged()
    {
        Instance_OnBeforeResize(ResizeParent.Instance);
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if((interactiveIndicator != null) && (HeldIn != null))
        {
            if(obj.CurrentTier == HeldIn.InteractiveTier)
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
    void Update()
    {
        if(animate == true)
        {
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
