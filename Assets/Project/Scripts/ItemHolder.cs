using UnityEngine;
using System.Collections;
using System;
using OmiyaGames;

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
    [SerializeField]
    SoundEffect placeSoundEffect;

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

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        // FIXME: if holding an item, change this item's tier value
        throw new NotImplementedException();
    }
}
