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
    Change action = Change.Shrink;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") == true)
        {
            if(action == Change.Shrink)
            {
                changeObject.ShrinkTier += 1;
            }
            else
            {
                changeObject.ShrinkTier -= 1;
            }
        }
    }
}
