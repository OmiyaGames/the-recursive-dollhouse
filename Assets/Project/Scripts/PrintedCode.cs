using UnityEngine;
using UnityEngine.UI;

public abstract class PrintedCode : MonoBehaviour
{
    [System.Serializable]
    public struct CanvasScale
    {
        public Vector2 dimensions;
        public Vector3 localScale;
    }

    [SerializeField]
    protected RectTransform[] canvases;
    [SerializeField]
    protected CanvasScale visibleScale;
    [SerializeField]
    protected CanvasScale fuzzyScale;
    [SerializeField]
    protected Text[] allLabels;

    public string CodeString
    {
        get
        {
            // FIXME: generate some code
            return null;
        }
    }

    public abstract int ThisTier
    {
        get;
    }

    public void OnTierChanged()
    {
        Instance_OnBeforeResize(ResizeParent.Instance);
    }

    // Use this for initialization
    protected virtual void Start()
    {
        ResizeParent.Instance.OnBeforeResize += Instance_OnBeforeResize;
        OnTierChanged();

        // FIXME: generate a unique code, and print that on all the labels
    }

    protected virtual void Update()
    {
        // Do nothing!
    }

    protected virtual void Instance_OnBeforeResize(ResizeParent obj)
    {
        // Check if this code is super large
        if (ThisTier >= 0)
        {
            foreach (RectTransform canvas in canvases)
            {
                if (obj.CurrentTier > ThisTier)
                {
                    canvas.sizeDelta = visibleScale.dimensions;
                    canvas.localScale = visibleScale.localScale;
                }
                else
                {
                    canvas.sizeDelta = fuzzyScale.dimensions;
                    canvas.localScale = fuzzyScale.localScale;
                }
            }
        }
    }
}
