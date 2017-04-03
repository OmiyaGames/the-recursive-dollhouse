using UnityEngine;
using OmiyaGames;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonModifiedController : FirstPersonController
{
    [Header("Modifications")]
    [SerializeField]
    protected SoundEffect footstepSoundEffect;
    [SerializeField]
    protected SoundEffect landSoundEffect;
    [SerializeField]
    protected SoundEffect jumpSoundEffect;

    [Header("Zoom")]
    [SerializeField]
    protected ParticleSystem m_ZoomEffect;
    [SerializeField]
    protected Kino.Motion m_BlurEffect;
    [SerializeField]
    protected SoundEffect diveInEffect;
    [SerializeField]
    protected SoundEffect jumpOutEffect;
    [SerializeField]
    protected Gazer gazer;

    PauseMenu pauseCache = null;
    bool allowMovement = true;
    Rigidbody controller;

    public Rigidbody Controller
    {
        get
        {
            if(controller == null)
            {
                controller= GetComponent<Rigidbody>();
            }
            return controller;
        }
    }

    public Gazer PlayerGazer
    {
        get
        {
            return gazer;
        }
    }

    public bool AllowMovement
    {
        get
        {
            bool returnFlag = allowMovement;
            if (pauseCache == null)
            {
                pauseCache = Singleton.Get<MenuManager>().GetMenu<PauseMenu>();
            }
            if ((pauseCache != null) && (pauseCache.CurrentState != IMenu.State.Hidden))
            {
                returnFlag = false;
            }
            return returnFlag;
        }
        set
        {
            if (allowMovement != value)
            {
                allowMovement = value;
                UpdateMouseLock();
            }
        }
    }

    public override bool IsGrounded
    {
        get
        {
            if (AllowMovement == true)
            {
                return m_CharacterController.isGrounded;
            }
            else
            {
                return true;
            }
        }
    }

    protected override void PlayLandingSound()
    {
        landSoundEffect.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    protected override void PlayJumpSound()
    {
        jumpSoundEffect.Play();
    }

    protected override void PlayFootStepAudio()
    {
        if ((!m_CharacterController.isGrounded) || (m_Slowdown == true))
        {
            return;
        }
        footstepSoundEffect.Play();
    }

    protected override void RotateView()
    {
        if (AllowMovement == true)
        {
            base.RotateView();
        }
    }

    protected override void GetInput(out float speed)
    {
        speed = 0;
        if (AllowMovement == true)
        {
            base.GetInput(out speed);
        }
    }

    protected override void GetJump(ref bool jump)
    {
        if (AllowMovement == true)
        {
            base.GetJump(ref jump);
        }
    }

    protected override void Start()
    {
        base.Start();

#if UNITY_WEBGL
        if(m_BlurEffect != null)
        {
            Destroy(m_BlurEffect);
        }
#endif

        m_MouseLook.OnGetIsSmooth += GetSmooth;
        m_MouseLook.OnGetXRotationAxis += GetXRotationAxis;
        m_MouseLook.OnGetYRotationAxis += GetYRotationAxis;
        OnGetHeadBobEnabled += GetHeadBob;
        OnGetXMovementAxis += GetXMovementAxis;
        OnGetYMovementAxis += GetYMovementAxis;
        if (Singleton.Instance.IsWebplayer == true)
        {
            Singleton.Get<MenuManager>().Show<LevelIntroMenu>(StartMovement);
            AllowMovement = false;
        }
    }

    void GetSmooth(MouseLook sender, MouseLook.SmoothEventArgs args)
    {
        GameSettings settings = Singleton.Get<GameSettings>();
        if (settings != null)
        {
            args.Smooth = settings.IsSmoothCameraEnabled;
        }
    }

    void GetXRotationAxis(MouseLook sender, MouseLook.RotationAxisEventArgs args)
    {
        GameSettings settings = Singleton.Get<GameSettings>();
        if(settings != null)
        {
            args.Sensitivity *= AdjustSensitivityBySetting(settings.MouseXAxisSensitivity, settings.IsMouseXAxisInverted);
        }
    }

    void GetYRotationAxis(MouseLook sender, MouseLook.RotationAxisEventArgs args)
    {
        GameSettings settings = Singleton.Get<GameSettings>();
        if (settings != null)
        {
            args.Sensitivity *= AdjustSensitivityBySetting(settings.MouseYAxisSensitivity, settings.IsMouseYAxisInverted);
        }
    }

    void GetHeadBob(FirstPersonController sender, HeadBobEventArgs args)
    {
        GameSettings settings = Singleton.Get<GameSettings>();
        if (settings != null)
        {
            args.Enable = settings.IsBobbingCameraEnabled;
        }
    }

    void GetXMovementAxis(FirstPersonController sender, MovementAxisEventArgs args)
    {
        GameSettings settings = Singleton.Get<GameSettings>();
        if (settings != null)
        {
            args.Sensitivity *= AdjustSensitivityBySetting(settings.KeyboardXAxisSensitivity, settings.IsKeyboardXAxisInverted);
        }
    }

    void GetYMovementAxis(FirstPersonController sender, MovementAxisEventArgs args)
    {
        GameSettings settings = Singleton.Get<GameSettings>();
        if (settings != null)
        {
            args.Sensitivity *= AdjustSensitivityBySetting(settings.KeyboardYAxisSensitivity, settings.IsKeyboardYAxisInverted);
        }
    }

    private static float AdjustSensitivityBySetting(float settingsSensitivity, bool settingsInverted)
    {
        float returnFloat = settingsSensitivity / GameSettings.DefaultSensitivity;
        if (settingsInverted == true)
        {
            returnFloat *= -1f;
        }
        return returnFloat;
    }

    public override void UpdateMouseLock()
    {
        if (AllowMovement == true)
        {
            Singleton.Get<SceneTransitionManager>().RevertCursorLockMode(false);
        }
        else
        {
            SceneTransitionManager.CursorMode = CursorLockMode.None;
        }
    }

    public override void StartSlowdown(bool diveIn)
    {
        base.StartSlowdown(diveIn);
        if (diveIn == true)
        {
            diveInEffect.Play();
        }
        else
        {
            jumpOutEffect.Play();
        }
        GameSettings settings = Singleton.Get<GameSettings>();
        if ((settings == null) || (settings.IsFlashesEnabled == true))
        {
            m_ZoomEffect.Play();
        }
#if !UNITY_WEBGL
        if ((settings == null) || (settings.IsMotionBlursEnabled == true))
        {
            m_BlurEffect.enabled = true;
        }
#endif
    }

    public override void StopSlowdown()
    {
        base.StopSlowdown();
        m_ZoomEffect.Stop();
#if !UNITY_WEBGL
        m_BlurEffect.enabled = false;
#endif
    }

    void StartMovement(IMenu menu)
    {
        if(menu.CurrentState == IMenu.State.Hidden)
        {
            AllowMovement = true;
        }
    }
}
