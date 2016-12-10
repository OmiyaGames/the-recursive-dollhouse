using UnityEngine;

public class GrowShrinkObject : MonoBehaviour
{
    public event System.Action<GrowShrinkObject> OnAfterShrinkTierChanged;

    [SerializeField]
    Transform changeObject;
    [SerializeField]
    Transform defaultCenter;
    [SerializeField]
    float smoothTime = 0.5f;
    [SerializeField]
    float scaleIncrement = 10f;

    int shrinkTier = 0;

    bool changingScale = false;
    float originalLocalScale;
    float targetLocalScale;
    float localScaleVelocity = 0;

    Vector3 pivotPoint;
    Vector3 targetScaleVector;
    Vector3 pivotOffset;

    public int ShrinkTier
    {
        get
        {
            return shrinkTier;
        }
    }

    public Transform Changing
    {
        get
        {
            if(changeObject == null)
            {
                changeObject = transform;
            }
            return changeObject;
        }
    }

    public void IncrementShrinkTier(Transform centerTo)
    {
        shrinkTier += 1;
        pivotPoint = centerTo.position;

        CalculateTargetScaleAndPosition();

        if (OnAfterShrinkTierChanged != null)
        {
            OnAfterShrinkTierChanged(this);
        }
    }

    public void DecrementShrinkTier(Transform centerTo)
    {
        if (shrinkTier > 0)
        {
            shrinkTier -= 1;
            pivotPoint = centerTo.position;

            CalculateTargetScaleAndPosition();

            if (OnAfterShrinkTierChanged != null)
            {
                OnAfterShrinkTierChanged(this);
            }
        }
    }

    void Start()
    {
        originalLocalScale = Changing.localScale.x;
        targetLocalScale = originalLocalScale;
        pivotPoint = defaultCenter.position;
    }

    void Update()
    {
        if(changingScale == true)
        {
            float changeInScale = Mathf.SmoothDamp(Changing.localScale.x, targetLocalScale, ref localScaleVelocity, smoothTime);
            targetScaleVector.x = (originalLocalScale * changeInScale);
            targetScaleVector.y = (originalLocalScale * changeInScale);
            targetScaleVector.z = (originalLocalScale * changeInScale);

            // diff from object pivot to desired pivot/origin
            pivotOffset = Changing.position - pivotPoint;

            // calc change in scale
            changeInScale = (targetScaleVector.x / Changing.localScale.x);

            // finally, actually perform the scale/translation
            Changing.localScale = targetScaleVector;
            Changing.position = (pivotOffset * changeInScale) + pivotPoint;

            // Check if we should stop scaling
            if (Mathf.Approximately(Changing.localScale.x, targetLocalScale) == true)
            {
                changingScale = false;
            }
        }
    }

    void CalculateTargetScaleAndPosition()
    {
        // Calculate new scale
        targetLocalScale = (originalLocalScale * Mathf.Pow(scaleIncrement, shrinkTier));
        changingScale = true;
    }
}
