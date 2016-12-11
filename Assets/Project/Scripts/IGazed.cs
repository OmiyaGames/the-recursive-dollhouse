﻿public abstract class IGazed : TierObject
{
    public abstract void OnGazeEnter(Gazer gazer);
    public abstract void OnGazeExit(Gazer gazer);
    public abstract void OnInteract(Gazer gazer);
}