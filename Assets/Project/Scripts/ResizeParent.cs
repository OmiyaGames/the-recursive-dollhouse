using UnityEngine;
using System.Collections;

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

    Vector3 shrinkScaleVector;
    Vector3 growScaleVector;
    Vector3 targetScale;
    Vector3 velocity;
    IEnumerator lastEnumerator = null;

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

    public void Shrink(Transform centerTo)
    {
        // Setup variables
        transform.localScale = growScaleVector;
        targetScale = shrinkScaleVector;
        currentDirection = ResizeDirection.Shrinking;

        // Run the event
        RunResize(centerTo);
    }

    public void Grow(Transform centerTo)
    {
        // Setup variables
        transform.localScale = shrinkScaleVector;
        targetScale = growScaleVector;
        currentDirection = ResizeDirection.Growing;

        // Run the event
        RunResize(centerTo);
    }

    void Start()
    {
        Instance = this;
        shrinkScaleVector = Vector3.one * shrinkScale;
        growScaleVector = Vector3.one * growScale;
        currentDirection = ResizeDirection.None;
    }

    void RunResize(Transform centerTo)
    {
        // Position the parent
        transform.position = centerTo.position;

        // Stop the previous event
        if (lastEnumerator != null)
        {
            StopCoroutine(lastEnumerator);
            lastEnumerator = null;
        }

        // Run the new one
        velocity = Vector3.zero;
        lastEnumerator = Neato();
        StartCoroutine(lastEnumerator);
    }

    IEnumerator Neato()
    {
        // Run the before resize event
        if(OnBeforeResize != null)
        {
            OnBeforeResize(this);
        }

        // Check if we met the target scale yet
        while(Mathf.Approximately(transform.localScale.x, targetScale.x) == false)
        {
            // If not, smooth damp
            transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref velocity, smoothTime);
            yield return null;
        }

        // Cleanup everything
        lastEnumerator = null;
        currentDirection = ResizeDirection.None;

        // Run the after resize event
        if (OnAfterResize != null)
        {
            OnAfterResize(this);
        }
    }
}
