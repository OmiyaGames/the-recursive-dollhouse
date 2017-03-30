using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractiveDecoration : TierObject
{
    public const int MaxTierDistance = 1;

    //[SerializeField]
    //float startingForce = 0f;
    //[SerializeField]
    //float startingTorque = 0f;
    [SerializeField]
    Vector2 sizeRange = new Vector2(1f, 1.5f);

    //Vector3 randomVelocity = Vector3.zero;
    Rigidbody body;
    Vector3 originalLocalPosition;
    Quaternion originalLocalRotation;

    void Start()
    {
        ResizeParent.Instance.OnBeforeResize += OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += OnAfterResize;

        body = GetComponent<Rigidbody>();
        transform.localScale = Random.Range(sizeRange.x, sizeRange.y) * Vector3.one;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    public override void SetTheme(MoodTheme theme)
    {
        // Do nothing
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        // Do nothing
    }

    private void OnAfterResize(ResizeParent obj)
    {
        // Enable body
        body.isKinematic = false;
        //if ((obj.CurrentTier - ThisTier) <= MaxTierDistance)
        //{
        //    randomVelocity.y = Random.Range(0, startingForce);
        //    body.AddForce(randomVelocity, ForceMode.VelocityChange);
        //    body.AddRelativeTorque(Random.insideUnitSphere * startingTorque, ForceMode.VelocityChange);
        //}
    }

    private void OnBeforeResize(ResizeParent obj)
    {
        // Disable body movement
        body.isKinematic = true;

        // Check if this object should now be out of view
        if((obj.CurrentTier - ThisTier) > MaxTierDistance)
        {
            body.velocity = Vector3.zero;
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
        }
    }
}
