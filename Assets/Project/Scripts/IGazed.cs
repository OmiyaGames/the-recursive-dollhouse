using UnityEngine;

public abstract class IGazed : MonoBehaviour
{
    public abstract void OnGazeEnter(Gazer gazer);
    public abstract void OnGazeExit(Gazer gazer);
    public abstract void OnInteract(Gazer gazer);
}
