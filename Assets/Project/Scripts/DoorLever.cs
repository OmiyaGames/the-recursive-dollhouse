using UnityEngine;
using UnityEngine.UI;

public class DoorLever : IDoor
{
    [Header("Switch Stuff")]
    [SerializeField]
    Lever associatedLever;
    [SerializeField]
    Text codeLabel;
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

    public override void OnInteract(Gazer gazer)
    {
        // Do nothing
    }

    protected override void Start()
    {
        // Setup
        if(associatedCode == null)
        {
            associatedCode = associatedLever.AssociatedCode;
        }
        codeLabel.text = associatedCode.CodeString;
        codeLabel.color = associatedCode.CodeColor(codeLabel);

        associatedLever.OnStateChanged += AssociatedLever_OnStateChanged;
        AssociatedLever_OnStateChanged(associatedLever);

        base.Start();
    }

    private void AssociatedLever_OnStateChanged(Lever obj)
    {
        IsOpen = (associatedLever.IsOn == isOnDoor);
    }
}
