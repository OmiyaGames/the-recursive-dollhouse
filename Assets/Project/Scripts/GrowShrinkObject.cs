using UnityEngine;
using System.Collections.Generic;

public class GrowShrinkObject : MonoBehaviour
{
    public event System.Action<GrowShrinkObject> OnAfterShrinkTierChanged;

    [SerializeField]
    Transform changeObject;
    [SerializeField]
    float smoothTime = 0.5f;
    [SerializeField]
    float scaleIncrement = 10f;
    [SerializeField]
    float targetYPosition = 1f;

    int shrinkTier = 0;

    bool changingScale = false;
    Vector3 originalLocalScale;
    Vector3 targetLocalScale = Vector3.one;
    Vector3 localScaleVelocity = Vector3.zero;

    readonly Stack<Transform> centers = new Stack<Transform>();
    float shrinkTierYPosition = 0f;
    float yPositionVelocity = 0f;
    Vector3 localPosition = Vector3.zero;

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
        centers.Push(centerTo);

        CalculateTargetScaleAndPosition();

        if (OnAfterShrinkTierChanged != null)
        {
            OnAfterShrinkTierChanged(this);
        }
    }

    public void DecrementShrinkTier()
    {
        if (shrinkTier > 0)
        {
            shrinkTier -= 1;
            centers.Pop();

            CalculateTargetScaleAndPosition();

            if (OnAfterShrinkTierChanged != null)
            {
                OnAfterShrinkTierChanged(this);
            }
        }
    }

    void Start()
    {
        localPosition = Changing.localPosition;

        originalLocalScale = Changing.localScale;
        targetLocalScale = originalLocalScale;
    }

    void Update()
    {
        if(changingScale == true)
        {
            if (Mathf.Approximately(Changing.localScale.x, targetLocalScale.x) == false)
            {
                localPosition.y = Mathf.SmoothDamp(localPosition.y, shrinkTierYPosition, ref yPositionVelocity, smoothTime);
                Changing.localPosition = localPosition;
                Changing.localScale = Vector3.SmoothDamp(Changing.localScale, targetLocalScale, ref localScaleVelocity, smoothTime);
            }
            else
            {
                localPosition.y = shrinkTierYPosition;
                Changing.localPosition = localPosition;
                Changing.localScale = targetLocalScale;
                changingScale = false;
            }
        }
    }

    // FIXME: probably want to get rid of
    float CurrentTierYPosition
    {
        get
        {
            float yPosition = transform.position.y;
            if (centers.Peek() != null)
            {
                yPosition = centers.Peek().position.y;
            }
            return yPosition;
        }
    }

    void CalculateTargetScaleAndPosition()
    {
        // Calculate new scale
        targetLocalScale.x = (originalLocalScale.x * Mathf.Pow(scaleIncrement, shrinkTier));
        targetLocalScale.y = (originalLocalScale.y * Mathf.Pow(scaleIncrement, shrinkTier));
        targetLocalScale.z = (originalLocalScale.z * Mathf.Pow(scaleIncrement, shrinkTier));
        changingScale = true;

        // FIXME: calculate the proper y position
        //shrinkTierYPosition
    }
}
