using UnityEngine;
using System.Collections;

public class GrowShrinkObject : MonoBehaviour
{
    [SerializeField]
    Transform changeObject;
    [SerializeField]
    float smoothTime = 0.5f;
    [SerializeField]
    float scaleIncrement = 10f;
    [SerializeField]
    float changeYPosition = -10f;

    int shrinkTier = 0;

    bool changingScale = false;
    Vector3 originalLocalScale;
    Vector3 targetLocalScale = Vector3.one;
    Vector3 localScaleVelocity = Vector3.zero;

    bool changingPosition = false;
    Vector3 originalLocalPosition;
    Vector3 targetLocalPosition = Vector3.one;
    Vector3 localPositionVelocity = Vector3.zero;

    public int ShrinkTier
    {
        get
        {
            return shrinkTier;
        }
        set
        {
            if(shrinkTier != value)
            {
                shrinkTier = value;
                if(shrinkTier < 0)
                {
                    shrinkTier = 0;
                }

                // Calculate new scale
                targetLocalScale.x = (originalLocalScale.x + (scaleIncrement * shrinkTier));
                targetLocalScale.y = (originalLocalScale.y + (scaleIncrement * shrinkTier));
                targetLocalScale.z = (originalLocalScale.z + (scaleIncrement * shrinkTier));
                changingScale = true;

                // Calculate new position
                targetLocalPosition.y = (originalLocalPosition.y + (changeYPosition * shrinkTier));
                changingPosition = true;
            }
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

    void Start()
    {
        originalLocalPosition = Changing.localPosition;
        targetLocalPosition = originalLocalPosition;

        originalLocalScale = Changing.localScale;
        targetLocalScale = originalLocalScale;
    }

    void Update()
    {
        if(changingScale == true)
        {
            if (Mathf.Approximately(Changing.localScale.x, targetLocalScale.x) == false)
            {
                Changing.localScale = Vector3.SmoothDamp(Changing.localScale, targetLocalScale, ref localScaleVelocity, smoothTime);
            }
            else
            {
                Changing.localScale = targetLocalScale;
                changingScale = false;
            }
        }
        if (changingPosition == true)
        {
            if (Mathf.Approximately(Changing.localPosition.y, targetLocalPosition.y) == false)
            {
                Changing.localPosition = Vector3.SmoothDamp(Changing.localPosition, targetLocalPosition, ref localPositionVelocity, smoothTime);
            }
            else
            {
                Changing.localPosition = targetLocalPosition;
                changingPosition = false;
            }
        }
    }
}
