using UnityEngine;
using OmiyaGames;
using System;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(CodeLabel))]
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
    Vector3 rotationCache;
    CodeLabel labelCache = null;

    public bool IsOn
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

    public CodeLabel AssociatedCode
    {
        get
        {
            if (labelCache == null)
            {
                labelCache = GetComponent<CodeLabel>();
            }
            return labelCache;
        }
    }

    protected virtual void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;
        UpdateAnimation();
    }

    protected virtual void OnEnable()
    {
        UpdateAnimation();
    }

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
            if (IsOn == false)
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
            IsOn = !IsOn;

            // Run gaze exit
            OnGazeExit(gazer);
        }
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        if ((trigger != null) && (gameObject.activeInHierarchy == true))
        {
            trigger.IsEnabled = false;
            OnGazeExit(null);
        }
        AssociatedCode.OnTierChanged();
        UpdateAnimation();
    }

    protected virtual void Instance_OnBeforeResize(ResizeParent obj)
    {
        if ((trigger != null) && (gameObject.activeInHierarchy == true))
        {
            trigger.IsEnabled = false;
            OnGazeExit(null);
        }
        UpdateAnimation();
    }

    protected virtual void Instance_OnAfterResize(ResizeParent obj)
    {
        if ((trigger != null) && (gameObject.activeInHierarchy == true))
        {
            trigger.IsEnabled = (obj.CurrentTier == ThisTier);
        }
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (switchAnimation.gameObject.activeInHierarchy == true)
        {
            switchAnimation.SetBool(StateField, IsOn);
        }
    }
}
