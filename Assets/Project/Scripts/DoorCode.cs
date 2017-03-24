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
    SoundEffect enterCodeSound;
    [SerializeField]
    SoundEffect buttonSound;
    [SerializeField]
    SoundEffect failedSound;
    [SerializeField]
    SoundEffect successSound;

    [Header("Camera Position")]
    [SerializeField]
    Transform fpsControllerPosition;
    [SerializeField]
    Transform fpsControllerCameraAngle;
    [SerializeField]
    float maxCameraMovement = 10f;
    [SerializeField]
    float smoothCameraMovement = 0.5f;
    [SerializeField]
    float maxCameraRotation = 1f;
    [SerializeField]
    float smoothCameraRotation = 0.05f;

    static bool firstTimeTryingCode = true;
    static string[] allNumberKeyCodes = null;

    bool isAnimatingPlayer = false;
    KeypadState state = KeypadState.Disabled;
    IEnumerator failAnimation = null;
    WaitForSeconds blinkOnDurationEnum, blinkOffDurationEnum;
    Vector3 playerPositionVelocity = Vector3.zero, playerRotationVelocity = Vector3.zero, cameraRotationVelocity = Vector3.zero;

    readonly StringBuilder CodeBuilder = new StringBuilder();

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
                Player.AllowMovement = (state != KeypadState.Enabled);

                // Setup Ready Trigger
                switch (state)
                {
                    case KeypadState.Enabled:
                        isAnimatingPlayer = true;
                        readyTrigger.IsEnabled = false;
                        break;
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

    private FirstPersonModifiedController Player
    {
        get
        {
            return ((FirstPersonModifiedController)FirstPersonController.Instance);
        }
    }

    private Camera PlayerCamera
    {
        get
        {
            return FirstPersonController.InstanceCamera;
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
            failedSound.Play();
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
            enterCodeSound.Play();
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

            if(isAnimatingPlayer == true)
            {
                // Check if we're close enough
                if (Approximately(Player.transform.position, fpsControllerPosition.position) &&
                    Approximately(Player.transform.rotation, fpsControllerPosition.rotation) &&
                    Approximately(PlayerCamera.transform.localRotation, fpsControllerCameraAngle.localRotation))
                {
                    // Snap to the proper location
                    Player.transform.position = fpsControllerPosition.position;
                    Player.transform.rotation = fpsControllerPosition.rotation;
                    PlayerCamera.transform.localRotation = fpsControllerCameraAngle.localRotation;

                    // Remove velocity
                    playerPositionVelocity = Vector3.zero;
                    playerRotationVelocity = Vector3.zero;
                    cameraRotationVelocity = Vector3.zero;

                    // Indicate we don't want to animate anymore
                    isAnimatingPlayer = false;
                }
                else
                {
                    // Animate the player movement
                    Player.transform.position = Vector3.SmoothDamp(Player.transform.position, fpsControllerPosition.position, ref playerPositionVelocity, smoothCameraMovement, maxCameraMovement);
                    Player.transform.rotation = SmoothDamp(Player.transform.rotation, fpsControllerPosition.rotation, ref playerRotationVelocity, smoothCameraRotation, maxCameraRotation);
                    PlayerCamera.transform.localRotation = SmoothDamp(PlayerCamera.transform.localRotation, fpsControllerCameraAngle.localRotation, ref cameraRotationVelocity, smoothCameraRotation, maxCameraRotation);
                }
            }

            // Check if the pause key is detected
            if ((Input.GetButtonDown(Singleton.Get<MenuManager>().PauseInput) == true) || Input.GetKeyDown(KeyCode.Escape))
            {
                ResetGaze();
            }
        }
    }

    Quaternion SmoothDamp(Quaternion begin, Quaternion end, ref Vector3 velocity, float smooth, float maxSpeed)
    {
        Vector3 beginEular = begin.eulerAngles;
        Vector3 endEular = end.eulerAngles;
        return Quaternion.Euler(
            Mathf.SmoothDampAngle(beginEular.x, endEular.x, ref velocity.x, smooth, maxSpeed),
            Mathf.SmoothDampAngle(beginEular.y, endEular.y, ref velocity.y, smooth, maxSpeed),
            Mathf.SmoothDampAngle(beginEular.z, endEular.z, ref velocity.z, smooth, maxSpeed)
            );
    }

    bool Approximately(Vector3 v1, Vector3 v2)
    {
        return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z);
    }

    bool Approximately(Quaternion q1, Quaternion q2)
    {
        return Mathf.Approximately(q1.x, q2.x) && Mathf.Approximately(q1.y, q2.y) && Mathf.Approximately(q1.z, q2.z) && Mathf.Approximately(q1.w, q2.w);
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
