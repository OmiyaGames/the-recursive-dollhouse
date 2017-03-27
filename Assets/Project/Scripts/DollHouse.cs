using UnityEngine;
using System.Collections.Generic;
using OmiyaGames;

public class DollHouse : TierObject
{
    readonly List<EnterTrigger> growTriggers = new List<EnterTrigger>();
    readonly List<EnterTrigger> shrinkTriggers = new List<EnterTrigger>();

    [SerializeField]
    bool enableItemHolder = true;
    [SerializeField]
    bool enabledSpring = true;
    [SerializeField]
    bool lastHouse = false;
    [SerializeField]
    float offsetOnShrink = 1.5f;

    [Header("Required Components")]
    [SerializeField]
    Transform bottom;
    [SerializeField]
    Transform growPoint;
    [SerializeField]
    Collider ceiling;
    [SerializeField]
    ItemHolder itemHolder;
    [SerializeField]
    Renderer houseRenderer;

    Vector3 offsetOnShrinkVector = Vector3.zero;

    public bool IsItemHolderEnabled
    {
        get
        {
            return enableItemHolder;
        }
        set
        {
            if(enableItemHolder != value)
            {
                enableItemHolder = value;
                if (itemHolder != null)
                {
                    itemHolder.gameObject.SetActive(enableItemHolder);
                }
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        if(itemHolder != null)
        {
            itemHolder.IsActive = enableItemHolder;
        }

        // Setup vector
        offsetOnShrinkVector.z = offsetOnShrink;

        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        ResizeParent.Instance.OnAfterResize += Instance_OnAfterResize;

        Instance_OnAfterResize(ResizeParent.Instance);

    }

    public void AssociateWith(EnterTrigger triggerInfo)
    {
        if (triggerInfo.Action == EnterTrigger.Change.Grow)
        {
            growTriggers.Add(triggerInfo);
        }
        else
        {
            shrinkTriggers.Add(triggerInfo);
        }
    }

    public void OnTrigger(EnterTrigger triggerInfo)
    {
        if (triggerInfo.Action == EnterTrigger.Change.Grow)
        {
            // Update stack
            ResizeParent.Instance.TierHistory.Add(ParentTier);
            ParentTier.ApplyTheme();

            // Run event
            ResizeParent.Instance.Grow(growPoint);

            // Check if we should play the credits...
            if (lastHouse == true)
            {
                Singleton.Get<SceneTransitionManager>().LoadNextLevel();
            }
        }
        else
        {
            // Run event
            Vector3 shrinkOrigin = triggerInfo.transform.position;
            shrinkOrigin += Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * offsetOnShrinkVector;
            ResizeParent.Instance.Shrink(shrinkOrigin);

            // Update stack
            if (ResizeParent.Instance.LatestTier == ParentTier)
            {
                ResizeParent.Instance.TierHistory.RemoveAt(ResizeParent.Instance.TierHistory.Count - 1);
                if (ResizeParent.Instance.LatestTier != null)
                {
                    ResizeParent.Instance.LatestTier.ApplyTheme();
                }
            }
        }
    }

    private void Instance_OnBeforeResize(ResizeParent obj)
    {
        foreach (EnterTrigger trigger in growTriggers)
        {
            trigger.IsEnabled = false;
        }
        foreach (EnterTrigger trigger in shrinkTriggers)
        {
            trigger.IsEnabled = false;
        }

        // Turn on the ceiling
        ceiling.enabled = false;
        if ((obj.currentDirection == ResizeParent.ResizeDirection.Shrinking) && (ThisTier == obj.CurrentTier))
        {
            ceiling.enabled = true;
        }
    }

    private void Instance_OnAfterResize(ResizeParent obj)
    {
        if (obj.CurrentTier == ThisTier)
        {
            foreach (EnterTrigger trigger in shrinkTriggers)
            {
                trigger.IsEnabled = true;
            }
        }
        else if ((obj.CurrentTier + 1) == ThisTier)
        {
            foreach (EnterTrigger trigger in growTriggers)
            {
                trigger.IsEnabled = true;
            }
        }

        foreach (EnterTrigger trigger in shrinkTriggers)
        {
            trigger.gameObject.SetActive((ThisTier > 0) && (enabledSpring == true));
        }
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        Instance_OnAfterResize(ResizeParent.Instance);
    }

    public override void SetTheme(MoodTheme theme)
    {
        UpdateTheme(houseRenderer, MoodSetter.Instance, theme);
    }

    public static void UpdateTheme(Renderer model, MoodSetter setter, MoodTheme theme)
    {
        // Update all the material textures!
        if (model.sharedMaterials.Length > 0)
        {
            Material[] newTheme = new Material[model.sharedMaterials.Length];

            // First set the floor material
            int index = newTheme.Length - 1;
            newTheme[index] = setter.RandomFloorMaterial;

            // Next, set the wall material
            --index;
            for (; index >= 0; --index)
            {
                newTheme[index] = theme.WallMaterial;
            }

            // Update material
            model.sharedMaterials = newTheme;
        }
    }
}
