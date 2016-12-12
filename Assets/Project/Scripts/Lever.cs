using UnityEngine;
using OmiyaGames;
using System;
using UnityStandardAssets.Characters.FirstPerson;

public class Lever : IGazed
{
    public enum LabelState
    {
        None,
        SwitchOn,
        SwitchOff
    }

    public const string StateField = "Visible";
    public const string LabelField = "State";
    public event Action<Lever> OnStateChanged;

    [Header("Required Components")]
    [SerializeField]
    SoundEffect trueStateSoundEffect;
    [SerializeField]
    SoundEffect falseStateSoundEffect;
    [SerializeField]
    Animator switchAnimation;
    [SerializeField]
    Animator labelsAnimation;
    [SerializeField]
    InteractionTrigger trigger;

    bool state = false;
    bool interactive = false;

    public bool State
    {
        get
        {
            return state;
        }
        private set
        {
            if (state != value)
            {
                state = value;

                // Play sound effect
                if (state == true)
                {
                    trueStateSoundEffect.Play();
                }
                else
                {
                    falseStateSoundEffect.Play();
                }

                // Play animation
                switchAnimation.SetBool(StateField, state);

                // Run event
                if (OnStateChanged != null)
                {
                    OnStateChanged(this);
                }
            }
        }
    }

    Vector3 rotationCache;
    void Update()
    {
        if ((interactive == true) && (labelsAnimation != null))
        {
            // Rotate the label to look at the player
            labelsAnimation.transform.LookAt(FirstPersonController.Instance.transform.position);
            rotationCache = labelsAnimation.transform.rotation.eulerAngles;
            rotationCache.x = 0;
            rotationCache.y += 180f;
            rotationCache.z = 0;
            labelsAnimation.transform.rotation = Quaternion.Euler(rotationCache);
        }
    }

    public override void OnGazeEnter(Gazer gazer)
    {
        interactive = true;
        if (labelsAnimation != null)
        {
            if (State == false)
            {
                labelsAnimation.SetInteger(LabelField, (int)LabelState.SwitchOn);
            }
            else
            {
                labelsAnimation.SetInteger(LabelField, (int)LabelState.SwitchOff);
            }
        }
    }

    public override void OnGazeExit(Gazer gazer)
    {
        interactive = false;
        if ((labelsAnimation != null) && (labelsAnimation.gameObject.activeInHierarchy == true))
        {
            labelsAnimation.SetInteger(LabelField, (int)LabelState.None);
        }
    }

    public override void OnInteract(Gazer gazer)
    {
        if(interactive == true)
        {
            // Toggle state
            State = !State;

            // Run gaze exit
            OnGazeExit(gazer);
        }
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if ((trigger != null) && (gameObject.activeInHierarchy == true))
        {
            trigger.IsEnabled = false;
            OnGazeExit(null);
        }
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if ((trigger != null) && (gameObject.activeInHierarchy == true))
        {
            trigger.IsEnabled = (obj.CurrentTier == ThisTier);
        }
    }
}
