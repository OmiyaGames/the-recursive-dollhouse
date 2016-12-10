using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

[DisallowMultipleComponent]
public class ResizeParent : MonoBehaviour
{
    public event System.Action<ResizeParent> OnBeforeResize;
    public event System.Action<ResizeParent> OnAfterResize;

    public enum ResizeDirection
    {
        None = -1,
        Growing = 0,
        Shrinking
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

    public static ResizeParent Instance
    {
        get;
        private set;
    }

    public ResizeDirection currentDirection
    {
        get;
        private set;
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
        float slowdownStartTime = 0;
        FirstPersonController.Instance.StartSlowdown(slowdown);

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
