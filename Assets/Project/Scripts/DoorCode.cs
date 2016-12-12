using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using OmiyaGames;
using UnityStandardAssets.Characters.FirstPerson;

public class DoorCode : IDoor
{
    public enum KeypadState
    {
        Disabled,
        Ready,
        Enabled,
        Complete
    }

    public const string EnterCodeText = "Enter Code:";
    public const string RightCodeText = "Success!";
    public const string FirstWrongCodeText = "Wrong Code";
    public const string StateField = "State";
    public readonly RandomList<string> OtherWrongCodeText = new RandomList<string>(new string[] {
        "Wrong Code",
        "Incorrect Code",
        "Hacker Detected",
        "Access Denied",
        "Try Again",
        "Non-matching Crednetials",
        "Don't Give Up!"
    });

    [Header("Required Components")]
    [SerializeField]
    Animator keypadAnimation;
    [SerializeField]
    InteractionTrigger readyTrigger;
    [SerializeField]
    Canvas setupCanvas;
    [SerializeField]
    Text codeLabel;
    [SerializeField]
    Text enterLabel;
    [SerializeField]
    Button[] allNumberButtons;

    [Header("Animation")]
    [SerializeField]
    float blinkOnDuration = 0.5f;
    [SerializeField]
    float blinkOffDuration = 0.5f;
    [SerializeField]
    int numberOfBlinks = 3;

    [Header("Sound Effects")]
    [SerializeField]
    SoundEffect buttonSound;
    [SerializeField]
    SoundEffect failedSound;
    [SerializeField]
    SoundEffect successSound;

    static bool firstTimeTryingCode = true;

    KeypadState state = KeypadState.Disabled;
    IEnumerator failAnimation = null;
    WaitForSeconds blinkOnDurationEnum, blinkOffDurationEnum;
    readonly StringBuilder CodeBuilder = new StringBuilder();
    static string[] allNumberKeyCodes = null;

    static string[] AllNumberKeyCodes
    {
        get
        {
            if (allNumberKeyCodes == null)
            {
                allNumberKeyCodes = new string[10];
                for (int index = 0; index < allNumberKeyCodes.Length; ++index)
                {
                    allNumberKeyCodes[index] = index.ToString();
                }
            }
            return allNumberKeyCodes;
        }
    }

    public KeypadState CurrentState
    {
        get
        {
            return state;
        }
        protected set
        {
            if (state != value)
            {
                state = value;
                keypadAnimation.SetInteger(StateField, (int)state);

                // Setup movement
                ((FirstPersonModifiedController)FirstPersonController.Instance).AllowMovement = (state != KeypadState.Enabled);

                // Setup Ready Trigger
                switch (state)
                {
                    case KeypadState.Enabled:
                    case KeypadState.Complete:
                        readyTrigger.IsEnabled = false;
                        break;
                    default:
                        readyTrigger.IsEnabled = true;
                        break;
                }

                foreach(Button button in allNumberButtons)
                {
                    button.interactable = (state == KeypadState.Enabled);
                }
            }
        }
    }

    #region Button Events
    public void OnKeyPressed(int key)
    {
        if (CurrentState == KeypadState.Enabled)
        {
            if (failAnimation != null)
            {
                // Stop the failAnimation
                StopCoroutine(failAnimation);
                failAnimation = null;

                // Revert the code label
                codeLabel.enabled = true;
                codeLabel.text = EnterCodeText;
            }

            // Update enter label
            CodeBuilder.Append(key % 10);
            enterLabel.text = CodeBuilder.ToString();

            // Check the length of input
            if (CodeBuilder.Length >= PrintedCode.NumberOfDigitsInCode)
            {
                // Revert Code Builder
                CodeBuilder.Length = 0;

                // Check if the code is correct
                if (enterLabel.text == associatedCode.CodeString)
                {
                    StartCoroutine(PlaySuccessAnimation());
                }
                else
                {
                    failAnimation = PlayFailAnimation();
                    StartCoroutine(failAnimation);
                }
            }
        }
    }

    public void OnExitPressed()
    {
        OnGazeExit(null);
    }
    #endregion

    #region Overrides
    protected override void Start()
    {
        // Setup
        codeLabel.text = EnterCodeText;
        foreach (Button button in allNumberButtons)
        {
            button.interactable = false;
        }

        // Setup canvas
        setupCanvas.gameObject.SetActive(true);
        setupCanvas.worldCamera = FirstPersonController.InstanceCamera;

        // Reset everything
        ResetGaze();

        // Cache enumerators
        blinkOnDurationEnum = new WaitForSeconds(blinkOnDuration);
        blinkOffDurationEnum = new WaitForSeconds(blinkOffDuration);

        // Call base method last
        base.Start();
    }

    public override void OnGazeEnter(Gazer gazer)
    {
        if ((CurrentState == KeypadState.Disabled) && (ResizeParent.Instance.CurrentTier == ThisTier))
        {
            // Indicate we're ready
            CurrentState = KeypadState.Ready;
        }
    }

    public override void OnGazeExit(Gazer gazer)
    {
        if (CurrentState == KeypadState.Ready)
        {
            ResetGaze();
        }
    }

    public override void OnInteract(Gazer gazer)
    {
        if (CurrentState == KeypadState.Ready)
        {
            // Indicate we allow typing
            CurrentState = KeypadState.Enabled;
        }
    }

    protected override void Instance_OnBeforeResize(ResizeParent obj)
    {
        base.Instance_OnBeforeResize(obj);

        if (readyTrigger != null)
        {
            readyTrigger.IsEnabled = false;
            ResetGaze();
        }
    }

    protected override void Instance_OnAfterResize(ResizeParent obj)
    {
        base.Instance_OnAfterResize(obj);

        if (readyTrigger != null)
        {
            readyTrigger.IsEnabled = (obj.CurrentTier == ThisTier);
            ResetGaze();
        }
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        base.OnThisTierChanged(obj);

        if (readyTrigger != null)
        {
            readyTrigger.IsEnabled = (obj.CurrentTier == ThisTier);
            ResetGaze();
        }
    }
    #endregion

    void Update()
    {
        // Check if the keypad is active
        if (CurrentState == KeypadState.Enabled)
        {
            // Go through all the accepted inputs on the keyboard
            for (int index = 0; index < AllNumberKeyCodes.Length; ++index)
            {
                // Check if this key is down
                if (Input.GetKey(AllNumberKeyCodes[index]) == true)
                {
                    // Enter this key
                    OnKeyPressed(index);
                    break;
                }
            }
        }
    }

    void ResetGaze()
    {
        // Disable typing into the keyboard
        CurrentState = KeypadState.Disabled;

        // Revert Code Builder
        CodeBuilder.Length = 0;

        // Update labels
        codeLabel.enabled = true;
        codeLabel.text = EnterCodeText;
        enterLabel.text = null;
    }

    IEnumerator PlayFailAnimation()
    {
        CurrentState = KeypadState.Disabled;

        // Update label
        enterLabel.enabled = true;
        if (firstTimeTryingCode == true)
        {
            enterLabel.text = FirstWrongCodeText;
        }
        else
        {
            enterLabel.text = OtherWrongCodeText.RandomElement;
        }
        yield return blinkOnDurationEnum;

        // Repeat blink on and off
        for (int index = 0; index < numberOfBlinks; ++index)
        {
            enterLabel.enabled = false;
            yield return blinkOffDurationEnum;
            enterLabel.enabled = true;
            yield return blinkOnDurationEnum;
        }

        // Update labels
        codeLabel.enabled = true;
        codeLabel.text = EnterCodeText;
        enterLabel.text = null;

        // Empty code
        failAnimation = null;
        ResetGaze();
    }

    IEnumerator PlaySuccessAnimation()
    {
        CurrentState = KeypadState.Complete;

        // Update label
        enterLabel.enabled = true;
        enterLabel.text = RightCodeText;
        yield return blinkOnDurationEnum;

        // Repeat blink on and off
        for (int index = 0; index < numberOfBlinks; ++index)
        {
            enterLabel.enabled = false;
            yield return blinkOffDurationEnum;
            enterLabel.enabled = true;
            yield return blinkOnDurationEnum;
        }

        // Open door
        setupCanvas.gameObject.SetActive(false);
        IsOpen = true;
    }

    [ContextMenu("GetAllButons")]
    private void GetAllButtons()
    {
        allNumberButtons = GetComponentsInChildren<Button>();
    }
}
