using UnityEngine;
using OmiyaGames;

public abstract class IDoor : IGazed
{
    const string OpenField = "Open";

    [SerializeField]
    protected PrintedCode associatedCode;
    [SerializeField]
    protected Animator doorAnimation;
    [SerializeField]
    protected ParticleSystem openParticles;
    [SerializeField]
    protected ParticleSystem closeParticles;
    [SerializeField]
    protected SoundEffect openSound;
    [SerializeField]
    protected SoundEffect closeSound;
    [SerializeField]
    protected MeshRenderer[] allDoorRenderers;

    bool isOpen = false;

    public bool IsOpen
    {
        get
        {
            return isOpen;
        }
        set
        {
            if (isOpen != value)
            {
                isOpen = value;
                UpdateAnimation();
                if (isOpen == true)
                {
                    if (openParticles != null)
                    {
                        openParticles.Play();
                    }
                    if(openSound != null)
                    {
                        openSound.Play();
                    }
                }
                else
                {
                    if (closeParticles != null)
                    {
                        closeParticles.Play();
                    }
                    if(closeSound != null)
                    {
                        closeSound.Play();
                    }
                }
            }
        }
    }

    public override void SetTheme(MoodTheme theme)
    {
        foreach(MeshRenderer renderer in allDoorRenderers)
        {
            renderer.sharedMaterial = theme.WallMaterial;
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

    protected virtual void Instance_OnBeforeResize(ResizeParent obj)
    {
        UpdateAnimation();
    }

    protected virtual void Instance_OnAfterResize(ResizeParent obj)
    {
        UpdateAnimation();
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (doorAnimation.gameObject.activeInHierarchy == true)
        {
            doorAnimation.SetBool(OpenField, isOpen);
        }
    }
}
