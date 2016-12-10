using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class EnterTrigger : MonoBehaviour
{
    public enum Change
    {
        Shrink,
        Grow
    }

    [SerializeField]
    GrowShrinkObject changeObject;
    [SerializeField]
    Transform center;
    [SerializeField]
    Change action = Change.Shrink;
    [SerializeField]
    int tier = 1;

    BoxCollider thisCollider = null;

    void Start()
    {
        thisCollider = GetComponent<BoxCollider>();
        changeObject.OnAfterShrinkTierChanged += UpdateColliderState;
        UpdateColliderState(changeObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == true)
        {
            if (action == Change.Shrink)
            {
                changeObject.IncrementShrinkTier(center);
            }
            else
            {
                changeObject.DecrementShrinkTier();
            }
        }
    }

    void UpdateColliderState(GrowShrinkObject obj)
    {
        // Setup whether to turn the colliders on or off
        thisCollider.enabled = false;
        if ((action == Change.Shrink) && (obj.ShrinkTier == (tier - 1)))
        {
            thisCollider.enabled = true;
        }
        else if ((action == Change.Grow) && (obj.ShrinkTier == (tier + 1)))
        {
            thisCollider.enabled = true;
        }
    }
}
