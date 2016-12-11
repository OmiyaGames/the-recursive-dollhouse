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

    PauseMenu pauseCache = null;

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
        if(pauseCache == null)
        {
            pauseCache = Singleton.Get<MenuManager>().GetMenu<PauseMenu>();
        }
        if((pauseCache != null) && (pauseCache.CurrentState != IMenu.State.Hidden))
        {
            return;
        }
        base.RotateView();
    }

    protected override void UpdateMouseLock()
    {
        // FIXME: until I know what Scene transition menu does, don't lock anything
        //m_MouseLook.UpdateCursorLock();
    }

    public override void StartSlowdown(bool diveIn)
    {
        base.StartSlowdown(diveIn);
        if(diveIn == true)
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
}
