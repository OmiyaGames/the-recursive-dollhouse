using UnityEngine;
using UnityEngine.UI;
using OmiyaGames;

public class DoorKey : IDoor
{
    public const string FirstWrongKeyText = "Wrong Key";
    public const string VisibleField = "Visible";
    public readonly RandomList<string> OtherWrongKeyText = new RandomList<string>(new string[] {
        "Wrong Key",
        "Incorrect Key",
        "I Hate That Key",
        "Correct Key Not Found",
        "Nooo! Bad Key!",
        "Key No Good!"
    });

    [Header("Required Components")]
    [SerializeField]
    Animator labelAnimation;
    [SerializeField]
    Text codeLabel;
    [SerializeField]
    Text errorLabel;
    [SerializeField]
    ItemHolder keyHolder;
    [SerializeField]
    InteractionTrigger trigger;
    [SerializeField]
    SoundEffect correctKeySound;
    [SerializeField]
    SoundEffect wrongKeySound;

    static bool firstTimeTryingKey = true;
    bool interactive = false;

    bool IsInteractive
    {
        get
        {
            return interactive;
        }
        set
        {
            if(interactive != value)
            {
                interactive = value;
                labelAnimation.SetBool(VisibleField, interactive);
            }
        }
    }

    public bool IsGazeEnabled
    {
        get
        {
            return (ResizeParent.Instance.CurrentTier == ThisTier);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (associatedCode != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, associatedCode.transform.position);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
#endif

    protected override void Start()
    {
        // Setup
        codeLabel.text = associatedCode.CodeString;
        codeLabel.color = associatedCode.CodeColor(codeLabel);

        keyHolder.gameObject.SetActive(true);
        errorLabel.gameObject.SetActive(false);

        // Call base method last
        base.Start();
    }

    public override void OnGazeEnter(Gazer gazer)
    {
        // Check if the user is holding an item
        if ((IsOpen == false) && (gazer.PlayerHolder.HoldingItem != null))
        {
            IsInteractive = true;
        }
    }

    private void HoldingItem_OnAnimationEnd(InventoryItem obj)
    {
        IsOpen = true;
        keyHolder.gameObject.SetActive(false);
        codeLabel.gameObject.SetActive(false);
        errorLabel.gameObject.SetActive(false);
    }

    public override void OnGazeExit(Gazer gazer)
    {
        errorLabel.gameObject.SetActive(false);
        IsInteractive = false;
    }

    public override void OnInteract(Gazer gazer)
    {
        if(IsInteractive == true)
        {
            // Check if they're holding the correct item
            if (gazer.PlayerHolder.HoldingItem == associatedCode)
            {
                // Associate with the end of this key animation
                gazer.PlayerHolder.HoldingItem.OnAnimationEnd += HoldingItem_OnAnimationEnd;
                ItemHolder.TransferItem(gazer.PlayerHolder, keyHolder);

                // Play sound effect
                correctKeySound.Play();
            }
            else
            {
                // Display error
                errorLabel.gameObject.SetActive(true);
                if (firstTimeTryingKey == true)
                {
                    errorLabel.text = FirstWrongKeyText;
                    firstTimeTryingKey = false;
                }
                else
                {
                    errorLabel.text = OtherWrongKeyText.RandomElement;
                }

                // Play sound effect
                wrongKeySound.Play();
            }
            IsInteractive = false;
        }
    }

    protected override void Instance_OnBeforeResize(ResizeParent obj)
    {
        base.Instance_OnBeforeResize(obj);

        if (trigger != null)
        {
            trigger.IsEnabled = false;
            OnGazeExit(null);
        }
    }

    protected override void Instance_OnAfterResize(ResizeParent obj)
    {
        base.Instance_OnAfterResize(obj);

        if (trigger != null)
        {
            trigger.IsEnabled = (obj.CurrentTier == ThisTier);
        }
    }

    protected override void OnThisTierChanged(ResizingTier obj)
    {
        base.OnThisTierChanged(obj);

        if (trigger != null)
        {
            trigger.IsEnabled = (obj.CurrentTier == ThisTier);
        }
    }
}
