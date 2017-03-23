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

    public const string EnterCodeText = "Door Code";
    public const string RightCodeText = "Door Success";
    public const string FirstWrongCodeText = "Door Wrong Code 1";
    public const string StateField = "State";
    public readonly RandomList<string> OtherWrongCodeText = new RandomList<string>(new string[] {
        "Door Wrong Code 1",
        "Door Wrong Code 2",
        "Door Wrong Code 3",
        "Door Wrong Code 4",
        "Door Wrong Code 5",
        "Door Wrong Code 6",
        "Door Wrong Code 7"
    });

    [Header("Required Components")]
    [SerializeField]
    Animator keypadAnimation;
    [SerializeField]
    InteractionTrigger readyTrigger;
    [SerializeField]
    Canvas setupCanvas;
    [SerializeField]
    TranslatedText codeLabel2;
    [SerializeField]
    TranslatedText errorLabel2;
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

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (associatedCode != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, associatedCode.transform.position);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
#endif

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
            }

            // Enable the code label
            errorLabel2.Label.enabled = true;

            // Update enter label
            CodeBuilder.Append(key % 10);
            errorLabel2.CurrentText = CodeBuilder.ToString();

            // Check the length of input
            if (CodeBuilder.Length >= PrintedCode.NumberOfDigitsInCode)
            {
                // Revert Code Builder
                CodeBuilder.Length = 0;

                // Check if the code is correct
                if (errorLabel2.CurrentText == associatedCode.CodeString)
                {
                    StartCoroutine(PlaySuccessAnimation());
                }
                else
                {
                    failAnimation = PlayFailAnimation();
                    StartCoroutine(failAnimation);
                }
            }
            else
            {
                buttonSound.Play();
            }
        }
    }

    public void OnExitPressed()
    {
        if (CurrentState != KeypadState.Complete)
        {
            ResetGaze();
        }
    }
    #endregion

    #region Overrides
    protected override void Start()
    {
        // Setup
        codeLabel2.TranslationKey = EnterCodeText;
        codeLabel2.Label.color = associatedCode.CodeColor(codeLabel2.Label);
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

    public override Gazer.SoundEffectType OnInteract(Gazer gazer)
    {
        if (CurrentState == KeypadState.Ready)
        {
            // Indicate we allow typing
            CurrentState = KeypadState.Enabled;
        }
        return Gazer.SoundEffectType.None;
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
            // Prevent entering via keyboard if on WebGL.  Sorry, this just makes mouse locking easier.
            if(Singleton.Instance.IsWebplayer == false)
            {
                // Go through all the accepted inputs on the keyboard
                for (int index = 0; index < AllNumberKeyCodes.Length; ++index)
                {
                    // Check if this key is down
                    if (Input.GetKeyDown(AllNumberKeyCodes[index]) == true)
                    {
                        // Enter this key
                        OnKeyPressed(index);
                        break;
                    }
                }
            }

            // Check if the pause key is detected
            if((Input.GetButtonDown(Singleton.Get<MenuManager>().PauseInput) == true) || Input.GetKeyDown(KeyCode.Escape))
            {
                ResetGaze();
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
        codeLabel2.Label.enabled = true;
        codeLabel2.TranslationKey = EnterCodeText;
        errorLabel2.CurrentText = null;
    }

    IEnumerator PlayFailAnimation()
    {
        CurrentState = KeypadState.Disabled;
        failedSound.Play();

        // Update label
        errorLabel2.Label.enabled = true;
        if (firstTimeTryingCode == true)
        {
            // Grab first time text
            errorLabel2.TranslationKey = FirstWrongCodeText;
            firstTimeTryingCode = false;
        }
        else
        {
            errorLabel2.TranslationKey = OtherWrongCodeText.RandomElement;
        }
        yield return blinkOnDurationEnum;

        // Repeat blink on and off
        for (int index = 0; index < numberOfBlinks; ++index)
        {
            errorLabel2.Label.enabled = false;
            yield return blinkOffDurationEnum;
            errorLabel2.Label.enabled = true;
            yield return blinkOnDurationEnum;
        }

        // Update labels
        codeLabel2.Label.enabled = true;
        codeLabel2.TranslationKey = EnterCodeText;
        errorLabel2.CurrentText = null;

        // Empty code
        failAnimation = null;
        ResetGaze();
    }

    IEnumerator PlaySuccessAnimation()
    {
        CurrentState = KeypadState.Complete;
        successSound.Play();

        // Update label
        errorLabel2.Label.enabled = true;
        errorLabel2.TranslationKey = RightCodeText;
        yield return blinkOnDurationEnum;

        // Repeat blink on and off
        for (int index = 0; index < numberOfBlinks; ++index)
        {
            errorLabel2.Label.enabled = false;
            yield return blinkOffDurationEnum;
            errorLabel2.Label.enabled = true;
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
