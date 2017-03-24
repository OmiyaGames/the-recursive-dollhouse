using UnityEngine;
using Toggler;

public class DoorLever : IDoor
{
    [Header("Switch Stuff")]
    [SerializeField]
    bool isOnDoor = true;

    public override void OnGazeEnter(Gazer gazer)
    {
        // Do nothing
    }

    public override void OnGazeExit(Gazer gazer)
    {
        // Do nothing
    }

    public override Gazer.SoundEffectType OnInteract(Gazer gazer)
    {
        return Gazer.SoundEffectType.None;
    }

    protected override void Start()
    {
        LeverGroup.OnBeforeStateChanged += OnStateChanged;
        base.Start();
    }

    void OnStateChanged(LeverGroup source, bool before, bool after)
    {
        if (before != after)
        {
            IsOpen = (after == isOnDoor);
        }
    }
}
