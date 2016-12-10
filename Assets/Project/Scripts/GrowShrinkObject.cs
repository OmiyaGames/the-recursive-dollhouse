using UnityEngine;
using System.Collections.Generic;

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
            Vector3 A = Changing.position;
            Vector3 B = pivotPoint;

            float RS = Mathf.SmoothDamp(Changing.localScale.x, targetLocalScale, ref localScaleVelocity, smoothTime);
            Vector3 endScale = Vector3.one * (originalLocalScale * RS);

            Vector3 C = A - B; // diff from object pivot to desired pivot/origin

            // calc final position post-scale
            RS = (endScale.x / Changing.localScale.x);
            Vector3 FP = (C * RS) + B;

            // finally, actually perform the scale/translation
            Changing.localScale = endScale;
            Changing.position = FP;

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
