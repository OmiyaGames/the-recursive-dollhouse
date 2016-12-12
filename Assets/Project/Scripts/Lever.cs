using UnityEngine;
using OmiyaGames;
using System;

public class Lever : IGazed
{
    public enum LabelState
    {
        None,
        SwitchOn,
        SwitchOff
    }

    public const string StateField = "Visible";
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

    public override void OnGazeEnter(Gazer gazer)
    {
        interactive = true;
        if (labelsAnimation != null)
        {
            if (State == false)
            {
                labelsAnimation.SetInteger(StateField, (int)LabelState.SwitchOn);
            }
            else
            {
                labelsAnimation.SetInteger(StateField, (int)LabelState.SwitchOff);
            }
        }
    }

    public override void OnGazeExit(Gazer gazer)
    {
        interactive = false;
        if ((labelsAnimation != null) && (labelsAnimation.gameObject.activeInHierarchy == true))
        {
            labelsAnimation.SetInteger(StateField, (int)LabelState.None);
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
