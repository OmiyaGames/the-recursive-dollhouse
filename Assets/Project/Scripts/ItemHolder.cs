using UnityEngine;
using System.Collections;
using System;

public class ItemHolder : IGazed
{
    public enum Type
    {
        Table,
        House,
        Player
    }

    public enum LabelState
    {
        None,
        PickUp,
        Drop
    }

    [SerializeField]
    Type type;
    [SerializeField]
    InventoryItem holdingItem;

    [Header("Required Components")]
    [SerializeField]
    Transform placement;

    [Header("Optional Components")]
    [SerializeField]
    Animator labelsAnimation;
    [SerializeField]
    ParticleSystem interactiveIndicator;

    // Use this for initialization
    void Start()
    {

    }

    public override void OnGazeEnter(Gazer gazer)
    {
        throw new NotImplementedException();
    }

    public override void OnGazeExit(Gazer gazer)
    {
        throw new NotImplementedException();
    }

    public override void OnInteract(Gazer gazer)
    {
        throw new NotImplementedException();
    }
}
