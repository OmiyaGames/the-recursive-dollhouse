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

    public bool IsGrounded
    {
        get
        {
            return m_CharacterController.isGrounded;
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
        m_MouseLook.XRotationAxis = GetXRotationAxis;
        m_MouseLook.YRotationAxis = GetYRotationAxis;
        if (Singleton.Instance.IsWebplayer == true)
        {
            Singleton.Get<MenuManager>().Show<LevelIntroMenu>(StartMovement);
            AllowMovement = false;
        }
    }

    float GetXRotationAxis(float input, float customSensitivity)
    {
        float returnFloat = input * customSensitivity;
        GameSettings settings = Singleton.Get<GameSettings>();
        if(settings != null)
        {
            returnFloat *= AdjustSensitivityBySetting(settings.MouseXAxisSensitivity, settings.IsMouseXAxisInverted);
        }
        return returnFloat;
    }

    float GetYRotationAxis(float input, float customSensitivity)
    {
        float returnFloat = input * customSensitivity;
        GameSettings settings = Singleton.Get<GameSettings>();
        if (settings != null)
        {
            returnFloat *= AdjustSensitivityBySetting(settings.MouseYAxisSensitivity, settings.IsMouseYAxisInverted);
        }
        return returnFloat;
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
        m_BlurEffect.enabled = true;
        m_ZoomEffect.Play();
    }

    public override void StopSlowdown()
    {
        base.StopSlowdown();
        m_BlurEffect.enabled = false;
        m_ZoomEffect.Stop();
    }

    void StartMovement(IMenu menu)
    {
        if(menu.CurrentState == IMenu.State.Hidden)
        {
            AllowMovement = true;
        }
    }
}
