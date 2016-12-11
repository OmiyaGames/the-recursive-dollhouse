using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

[DisallowMultipleComponent]
public class ResizeParent : MonoBehaviour
{
    public const string TagPlayer = "Player";
    public const string TagItem = "Item";
    public const string TagSwitch = "Switch";

    public event System.Action<ResizeParent> OnBeforeResize;
    public event System.Action<ResizeParent> OnAfterResize;

    public enum ResizeDirection
    {
        None = -1,
        Growing = 0,
        Shrinking
    }

    enum Setup
    {
        Podiums = 0,
        ResizingTiers,
        None
    }

    [SerializeField]
    float shrinkScale = 1;
    [SerializeField]
    float growScale = 20;
    [SerializeField]
    float smoothTime = 0.325f;
    [SerializeField]
    float snapDistance = 0.01f;

    [Header("Slowdown")]
    [SerializeField]
    float slowdown = 0.1f;
    [SerializeField]
    float slowdownDuration = 0.5f;

    Vector3 shrinkScaleVector;
    Vector3 growScaleVector;
    Vector3 targetScale;
    Vector3 velocity;
    IEnumerator lastEnumerator = null;
    int currentTier = 0;
    GameObject resizeHelper = null;
    Setup currentSetup = 0;

    public struct TierPath
    {
        public readonly ResizingTier start;
        public readonly ResizingTier end;

        public TierPath(ResizingTier start, ResizingTier end)
        {
            this.start = start;
            this.end = end;
        }
    }

    public readonly Dictionary<TierPath, Podium> PathToPodiumMap = new Dictionary<TierPath, Podium>();
    public readonly Dictionary<ResizingTier, HashSet<Podium>> AllPodiumsPerTier = new Dictionary<ResizingTier, HashSet<Podium>>();
    public readonly List<ResizingTier> AllTiers = new List<ResizingTier>();
    public readonly List<ResizingTier> TierHistory = new List<ResizingTier>();

    #region Properties
    public ResizingTier LatestTier
    {

        get
        {
            ResizingTier returnTier = null;
            if(TierHistory.Count > 0)
            {
                returnTier = TierHistory[TierHistory.Count - 1];
            }
            return returnTier;
        }
    }

    public static ResizeParent Instance
    {
        get;
        private set;
    }

    public float ShrinkScale
    {
        get
        {
            return shrinkScale;
        }
    }

    public Vector3 ShrinkScaleVector
    {
        get
        {
            return shrinkScaleVector;
        }
    }

    public float GrowScale
    {
        get
        {
            return growScale;
        }
    }

    public Vector3 GrowScaleVector
    {
        get
        {
            return growScaleVector;
        }
    }

    public ResizeDirection currentDirection
    {
        get;
        private set;
    }

    public GameObject ResizeHelper
    {
        get
        {
            if(resizeHelper == null)
            {
                resizeHelper = new GameObject("Resize Helper");
            }
            return resizeHelper;
        }
    }

    public int CurrentTier
    {
        get
        {
            return currentTier;
        }
        private set
        {
            currentTier = value;

            // Prevent the tiers from going below 0
            if(currentTier < 0)
            {
                currentTier = 0;
            }
        }
    }
    #endregion

    public void Shrink(Transform centerTo)
    {
        Shrink(centerTo.position);
    }

    public void Shrink(Vector3 centerTo)
    {
        // Setup variables
        transform.localScale = growScaleVector;
        targetScale = shrinkScaleVector;
        currentDirection = ResizeDirection.Shrinking;

        // Decrement tier
        CurrentTier -= 1;

        // Run the event
        RunResize(centerTo);
    }

    public void Grow(Transform centerTo)
    {
        Grow(centerTo.position);
    }

    public void Grow(Vector3 centerTo)
    {
        // Setup variables
        transform.localScale = shrinkScaleVector;
        targetScale = growScaleVector;
        currentDirection = ResizeDirection.Growing;

        // Increment tier
        CurrentTier += 1;

        // Run the event
        RunResize(centerTo);
    }

    void Awake()
    {
        Instance = this;
        shrinkScaleVector = Vector3.one * shrinkScale;
        growScaleVector = Vector3.one * growScale;
        currentDirection = ResizeDirection.None;
    }

    void Update()
    {
        switch(currentSetup)
        {
            case Setup.Podiums:
                foreach(HashSet<Podium> podiums in AllPodiumsPerTier.Values)
                {
                    foreach(Podium podium in podiums)
                    {
                        podium.ExtraSetup(this);
                    }
                }
                break;
            case Setup.ResizingTiers:
                foreach (ResizingTier tier in AllTiers)
                {
                    tier.ExtraSetup(this);
                }
                break;
        }

        if(currentSetup != Setup.None)
        {
            currentSetup += 1;
        }
    }

    void RunResize(Vector3 centerTo)
    {
        // Position the parent
        transform.position = centerTo;

        // Stop the previous event
        if (lastEnumerator != null)
        {
            StopCoroutine(lastEnumerator);
            lastEnumerator = null;
        }

        // Run the new one
        velocity = Vector3.zero;
        lastEnumerator = ResizeCoroutine();
        StartCoroutine(lastEnumerator);
    }

    IEnumerator ResizeCoroutine()
    {
        // Run the before resize event
        if(OnBeforeResize != null)
        {
            OnBeforeResize(this);
        }

        // Start the slowdown on the player
        float slowdownStartTime = Time.time;
        FirstPersonController.Instance.StartSlowdown(currentDirection == ResizeDirection.Growing);

        // Check if we met the target scale yet
        while (Mathf.Abs(transform.localScale.x - targetScale.x) > snapDistance)
        {
            // If not, smooth damp
            transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref velocity, smoothTime);

            // Check if slowdown should happen
            if((Time.time - slowdownStartTime) > slowdownDuration)
            {
                FirstPersonController.Instance.StopSlowdown();
            }
            yield return null;
        }

        // Snap to the target scale
        transform.localScale = targetScale;
        yield return null;

        // Run the after resize event
        if (OnAfterResize != null)
        {
            OnAfterResize(this);
        }

        // Cleanup everything
        lastEnumerator = null;
        currentDirection = ResizeDirection.None;
        FirstPersonController.Instance.StopSlowdown();
    }
}
