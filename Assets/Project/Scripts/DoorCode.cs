using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using OmiyaGames;

public class DoorCode : IDoor
{
    public enum KeypadState
    {
        Disabled,
        Enabled,
        Complete
    }

    public const string EnterCodeText = "Enter Code:";
    public const string RightCodeText = "Success!";
    public const string FirstWrongCodeText = "Wrong Code";
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
    Canvas setupCanvas;
    [SerializeField]
    GraphicRaycaster raycaster;
    [SerializeField]
    Text codeLabel;
    [SerializeField]
    Text enterLabel;

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
    int gazeInNumber = 0;
    IEnumerator failAnimation = null;
    WaitForSeconds blinkOnDurationEnum, blinkOffDurationEnum;
    readonly StringBuilder CodeBuilder = new StringBuilder();
    static string[] allNumberKeyCodes = null;

    static string[] AllNumberKeyCodes
    {
        get
        {
            if(allNumberKeyCodes == null)
            {
                allNumberKeyCodes = new string[10];
                for(int index = 0; index < allNumberKeyCodes.Length; ++index)
                {
                    allNumberKeyCodes[index] = index.ToString();
                }
            }
            return allNumberKeyCodes;
        }
    }

    #region Button Events
    public void OnGazeEnter()
    {
        OnGazeEnter(null);
    }

    public void OnGazeExit()
    {
        OnGazeExit(null);
    }

    public void OnKeyPressed(int key)
    {
        // Update enter label
        CodeBuilder.Append(key % 10);
        enterLabel.text = CodeBuilder.ToString();

        // Check the length of input
        if (CodeBuilder.Length >= PrintedCode.NumberOfDigitsInCode)
        {
            // Revert Code Builder
            CodeBuilder.Length = 0;

            if(failAnimation != null)
            {
                // Stop the failAnimation
                StopCoroutine(failAnimation);
                failAnimation = null;

                // Revert the code label
                codeLabel.enabled = true;
                codeLabel.text = EnterCodeText;
            }

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
    #endregion

    #region Overrides
    protected override void Start()
    {
        // Setup
        codeLabel.text = associatedCode.CodeString;
        setupCanvas.gameObject.SetActive(true);
        blinkOnDurationEnum = new WaitForSeconds(blinkOnDuration);
        blinkOffDurationEnum = new WaitForSeconds(blinkOffDuration);

        // Call base method last
        base.Start();
    }

    public override void OnGazeEnter(Gazer gazer)
    {
        if((state != KeypadState.Complete) && (ResizeParent.Instance.CurrentTier == ThisTier))
        {
            // Check if this is the first time gazing
            if(gazeInNumber == 0)
            {
                // Allow typing into the keyboard
                state = KeypadState.Enabled;

                // Turn on the reticle
                Singleton.Get<MenuManager>().Show<ReticleMenu>();
            }

            // Increment gaze number
            gazeInNumber += 1;
        }
    }

    public override void OnGazeExit(Gazer gazer)
    {
        if (state == KeypadState.Enabled)
        {
            // Decrement gaze number
            gazeInNumber -= 1;

            // Check if we should disable the keypad
            if(gazeInNumber <= 0)
            {
                ResetGaze();
            }
        }
    }

    public override void OnInteract(Gazer gazer)
    {
        // Do nothing!
    }

    protected override void Instance_OnBeforeResize(ResizeParent obj)
    {
        base.Instance_OnBeforeResize(obj);

        if (raycaster != null)
        {
            raycaster.enabled = false;
            ResetGaze();
        }
    }

    protected override void Instance_OnAfterResize(ResizeParent obj)
    {
        base.Instance_OnAfterResize(obj);

        if (raycaster != null)
        {
            raycaster.enabled = (obj.CurrentTier == ThisTier);
            ResetGaze();
        }
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        base.OnThisTierChanged(obj);

        if (raycaster != null)
        {
            raycaster.enabled = (obj.CurrentTier == ThisTier);
            ResetGaze();
        }
    }
    #endregion

    void Update()
    {
        // Check if the keypad is active
        if(state == KeypadState.Enabled)
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
        // Prevent gazeNumber from going below 0
        gazeInNumber = 0;

        // Disable typing into the keyboard
        state = KeypadState.Disabled;

        // Turn off the reticle
        Singleton.Get<MenuManager>().Hide<ReticleMenu>();
    }

    IEnumerator PlayFailAnimation()
    {
        // Update label
        if(firstTimeTryingCode == true)
        {
            codeLabel.text = FirstWrongCodeText;
        }
        else
        {
            codeLabel.text = OtherWrongCodeText.RandomElement;
        }
        codeLabel.text = RightCodeText;
        codeLabel.enabled = true;
        yield return blinkOnDurationEnum;

        // Repeat blink on and off
        for (int index = 0; index < numberOfBlinks; ++index)
        {
            codeLabel.enabled = false;
            yield return blinkOffDurationEnum;
            codeLabel.enabled = true;
            yield return blinkOnDurationEnum;
        }

        // Empty code
        codeLabel.enabled = true;
        codeLabel.text = EnterCodeText;
        enterLabel.text = null;
        failAnimation = null;
    }

    IEnumerator PlaySuccessAnimation()
    {
        state = KeypadState.Complete;

        // Update label
        enterLabel.text = RightCodeText;
        enterLabel.enabled = true;
        yield return blinkOnDurationEnum;

        // Repeat blink on and off
        for(int index = 0; index < numberOfBlinks; ++index)
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
}
