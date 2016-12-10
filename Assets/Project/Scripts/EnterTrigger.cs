using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(BoxCollider))]
public class EnterTrigger : MonoBehaviour
{
    public enum Change
    {
        Shrink,
        Grow
    }

    [SerializeField]
    DollHouse parentHouse;
    [SerializeField]
    Change action = Change.Shrink;

    BoxCollider thisCollider = null;

    BoxCollider Collider
    {
        get
        {
            if(thisCollider == null)
            {
                thisCollider = GetComponent<BoxCollider>();
            }
            return thisCollider;
        }
    }

    public bool IsEnabled
    {
        get
        {
            return Collider.enabled;
        }
        set
        {
            Collider.enabled = value;
        }
    }

    public Change Action
    {
        get
        {
            return action;
        }
    }

    void Awake()
    {
        // Add this trigger before parentHouse's Start function runs
        parentHouse.AssociateWith(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == true)
        {
            parentHouse.OnTrigger(this);
        }
        if (action == Change.Shrink)
        {
            FirstPersonController.Instance.ActivateSpring();
        }
    }
}
