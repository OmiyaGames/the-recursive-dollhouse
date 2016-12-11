using UnityEngine;

public class ItemHolderTrigger : IGazed
{
    [SerializeField]
    IGazed interactWith;

    public override void OnGazeEnter(Gazer gazer)
    {
        interactWith.OnGazeEnter(gazer);
    }

    public override void OnGazeExit(Gazer gazer)
    {
        interactWith.OnGazeExit(gazer);
    }

    public override void OnInteract(Gazer gazer)
    {
        interactWith.OnInteract(gazer);
    }
}
