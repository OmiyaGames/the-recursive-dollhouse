using UnityEngine;
using UnityEngine.UI;

public abstract class PrintedCode : MonoBehaviour
{
    public const int NumberOfDigitsInCode = 4;
    static readonly int MaxCode = (int)Mathf.Pow(10, NumberOfDigitsInCode);

    public enum TierComparison
    {
        CurrentTier,
        ParentTier
    }

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
    [SerializeField]
    protected TierComparison giveLabelsClarityOn = TierComparison.CurrentTier;

    int code = -1;
    string cachedCode = null;

    public string CodeString
    {
        get
        {
            if(cachedCode == null)
            {
                cachedCode = CodeInt.ToString("0000");
            }
            return cachedCode;
        }
    }

    public Color CodeColor(Text label)
    {
        // FIXME: grab a random color, and return that!
        return label.color;
    }

    public int CodeInt
    {
        get
        {
            if(code < 0)
            {
                // Generate a unique code
                code = Random.Range(0, MaxCode);
                while(ResizeParent.Instance.CodeToPrintMap.ContainsKey(code) == true)
                {
                    code = Random.Range(0, MaxCode);
                }

                // Add the code into the map
                ResizeParent.Instance.CodeToPrintMap.Add(code, this);
            }
            return code;
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

        // Generate a unique code, and print that on all the labels
        foreach(Text label in allLabels)
        {
            if(label != null)
            {
                label.text = CodeString;
            }
        }
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
                if (IsLabelVisible(obj) == true)
                {
                    MakeLabelClear(canvas);
                }
                else
                {
                    MakeLabelFuzzy(canvas);
                }
            }
        }
    }

    private bool IsLabelVisible(ResizeParent obj)
    {
        bool returnFlag = false;
        switch(giveLabelsClarityOn)
        {
            case TierComparison.ParentTier:
                returnFlag = (obj.CurrentTier > ThisTier + 1);
                break;
            default:
                returnFlag = (obj.CurrentTier > ThisTier);
                break;
        }
        return returnFlag;
    }

    void MakeLabelFuzzy(RectTransform canvas)
    {
        canvas.sizeDelta = fuzzyScale.dimensions;
        canvas.localScale = fuzzyScale.localScale;
    }

    void MakeLabelClear(RectTransform canvas)
    {
        canvas.sizeDelta = visibleScale.dimensions;
        canvas.localScale = visibleScale.localScale;
    }

#if UNITY_EDITOR

    [ContextMenu("Peview Fuzzy Label")]
    public void MakeLabelFuzzy()
    {
        foreach (RectTransform canvas in canvases)
        {
            canvas.sizeDelta = fuzzyScale.dimensions;
            canvas.localScale = fuzzyScale.localScale;
        }
    }

    [ContextMenu("Peview Clear Label")]
    public void MakeLabelClear()
    {
        foreach (RectTransform canvas in canvases)
        {
            canvas.sizeDelta = visibleScale.dimensions;
            canvas.localScale = visibleScale.localScale;
        }
    }
#endif
}
