﻿using UnityEngine;
using OmiyaGames;

public class ResizingTier : MonoBehaviour, IDelayedSetup
{
    public event System.Action<ResizingTier> OnCurrentTierChanged;

    [SerializeField]
    int startingTier = 0;

    MoodTheme assignedTheme = null;
    AudioClip assignedMusic = null;
    TierObject[] allObjects = null;

    public int CurrentTier
    {
        get
        {
            return startingTier;
        }
        set
        {
            if (startingTier != value)
            {
                startingTier = value;

                // Prevent the tiers from going below 0
                if (startingTier < 0)
                {
                    startingTier = 0;
                }

                ExtraSetup(ResizeParent.Instance);
                if (OnCurrentTierChanged != null)
                {
                    OnCurrentTierChanged(this);
                }
            }
        }
    }

    public TierObject[] AllObjects
    {
        get
        {
            if(allObjects == null)
            {
                allObjects = GetComponentsInChildren<TierObject>();
            }
            return allObjects;
        }
    }

    void Awake()
    {
        foreach (TierObject tier in AllObjects)
        {
            tier.ParentTier = this;
        }
    }

    void Start()
    {
        // Adjust to theme
        assignedTheme = MoodSetter.Instance.RandomTheme;
        foreach (TierObject tier in AllObjects)
        {
            tier.SetTheme(assignedTheme);
        }

        // Need to add this tier into all the lists
        ResizeParent.Instance.AllTiers.Add(this);

        // Add this tier to the stack if this is the first tier
        if (startingTier == 0)
        {
            ResizeParent.Instance.TierHistory.Add(this);
            ApplyTheme();
        }

        // Bind to event
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += ExtraSetup;
    }

    public void ExtraSetup(ResizeParent obj)
    {
        if (IsActive(obj) == true)
        {
            // Un-embed
            gameObject.SetActive(true);
            transform.SetParent(null, true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ApplyTheme()
    {
        // Change lighting
        MoodSetter.Instance.CurrentTheme = assignedTheme;

        // Change Music
        if(assignedMusic == null)
        {
            assignedMusic = MoodSetter.Instance.RandomMusic;
        }
        Singleton.Get<BackgroundMusic>().ChangeCurrentMusic(assignedMusic, true);
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        if (IsActive(obj) == true)
        {
            // Parent this to the resize parent
            gameObject.SetActive(true);
            transform.SetParent(obj.transform, true);
        }
        else if((obj.currentDirection == ResizeParent.ResizeDirection.Shrinking) && ((obj.CurrentTier + 2) == CurrentTier))
        {
            // Leave this object visible
            gameObject.SetActive(true);
            transform.SetParent(obj.transform, true);
        }
        else if ((obj.currentDirection == ResizeParent.ResizeDirection.Growing) && ((obj.CurrentTier - 2) == CurrentTier))
        {
            // Leave this object visible
            gameObject.SetActive(true);
            transform.SetParent(obj.transform, true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    bool IsActive(ResizeParent obj)
    {
        return (Mathf.Abs(obj.CurrentTier - CurrentTier) <= 1);
    }
}
