using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using OmiyaGames;

public class Gazer : MonoBehaviour
{
    public const string GazeInteractInput = "Fire1";

    public enum SoundEffectType
    {
        None,
        PickUpKey,
        DropKey
    }

    [SerializeField]
    FirstPersonModifiedController controller;
    [SerializeField]
    ItemHolder playerHolder;
    [SerializeField]
    float raycastDistance = 3f;
    [SerializeField]
    float raycastDistanceWhenHoldingItem = 5f;
    [SerializeField]
    LayerMask raycastMask;

    [Header("Sound effects")]
    [SerializeField]
    SoundEffect pickup;
    [SerializeField]
    SoundEffect drop;

    Ray rayCache;
    RaycastHit info;
    InteractionTrigger currentTrigger = null;
    InteractionTrigger lastTrigger = null;

    public ItemHolder PlayerHolder
    {
        get
        {
            return playerHolder;
        }
    }

    public float RaycastDistance
    {
        get
        {
            float returnDistance = raycastDistance;
            if (PlayerHolder.HoldingItem != null)
            {
                return raycastDistanceWhenHoldingItem;
            }
            return returnDistance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Reset current trigger this frame
        currentTrigger = null;

        // Make sure that we want to allow raycasting
        if ((controller.IsGrounded == true) && (Singleton.Get<MenuManager>().PauseMenu.CurrentState == IMenu.State.Hidden))
        {
            // Update ray-casting
            rayCache.origin = transform.position;
            rayCache.direction = transform.forward;

            // Ray cast
            if (Physics.Raycast(rayCache, out info, RaycastDistance, raycastMask) == true)
            {
                // Grab ray-casted object
                currentTrigger = info.collider.GetComponent<InteractionTrigger>();

                // Interact with the trigger
                if (currentTrigger != null)
                {
                    if (lastTrigger != currentTrigger)
                    {
                        currentTrigger.OnGazeEnter(this);
                    }
                    if (CrossPlatformInputManager.GetButton(GazeInteractInput) == true)
                    {
                        switch (currentTrigger.OnInteract(this))
                        {
                            case SoundEffectType.PickUpKey:
                                pickup.Play();
                                break;
                            case SoundEffectType.DropKey:
                                drop.Play();
                                break;
                        }
                    }
                }
            }
        }

        // Update lastTrigger
        if ((lastTrigger != null) && (lastTrigger != currentTrigger))
        {
            lastTrigger.OnGazeExit(this);
        }
        lastTrigger = currentTrigger;
    }
}
