using UnityEngine;
using OmiyaGames;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class ReticleMenu : IMenu
{
    public const string VisibleField = "Visible";

    [SerializeField]
    bool forceToBack = true;

    public override Type MenuType
    {
        get
        {
            return Type.UnmanagedMenu;
        }
    }

    public override GameObject DefaultUi
    {
        get
        {
            return null;
        }
    }

    protected virtual void Start()
    {
        if (forceToBack == true)
        {
            // Always make this the background
            transform.SetAsFirstSibling();
        }
    }

    protected override void OnStateChanged(State from, State to)
    {
        // Update the animator
        if (to == State.Visible)
        {
            Animator.SetBool(VisibleField, true);
        }
        else
        {
            Animator.SetBool(VisibleField, false);
        }
    }
}
