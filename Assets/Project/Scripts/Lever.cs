using UnityEngine;
using OmiyaGames;
using UnityStandardAssets.Characters.FirstPerson;

namespace Toggler
{
    public class Lever : IGazed
    {
        public enum LabelState
        {
            None,
            SwitchOn,
            SwitchOff
        }

        public const string StateField = "Visible";
        public const string LabelField = "State";

        [Header("Required Components")]
        [SerializeField]
        SoundEffect trueStateSoundEffect;
        [SerializeField]
        SoundEffect falseStateSoundEffect;
        [SerializeField]
        Animator switchAnimation;
        [SerializeField]
        Animator labelsAnimation;
        [SerializeField]
        InteractionTrigger trigger;

        bool interactive = false;
        Vector3 rotationCache;

        public override void SetTheme(MoodTheme theme)
        {
            // FIXME: update all the material textures!
            //throw new System.NotImplementedException();
        }

        protected virtual void Start()
        {
            LeverGroup.OnBeforeStateChanged += OnStateChanged;
            ResizeParent.Instance.OnBeforeResize += OnBeforeResize;
            ResizeParent.Instance.OnAfterResize += OnAfterResize;
            UpdateAnimation();
        }

        protected virtual void OnEnable()
        {
            UpdateAnimation();
        }

        void Update()
        {
            if ((interactive == true) && (labelsAnimation != null))
            {
                // Rotate the label to look at the player
                labelsAnimation.transform.LookAt(FirstPersonController.Instance.transform.position);
                rotationCache = labelsAnimation.transform.rotation.eulerAngles;
                rotationCache.x = 0;
                rotationCache.y += 180f;
                rotationCache.z = 0;
                labelsAnimation.transform.rotation = Quaternion.Euler(rotationCache);
            }
        }

        public override void OnGazeEnter(Gazer gazer)
        {
            interactive = true;
            if (labelsAnimation != null)
            {
                if (LeverGroup.IsOn == false)
                {
                    labelsAnimation.SetInteger(LabelField, (int)LabelState.SwitchOn);
                }
                else
                {
                    labelsAnimation.SetInteger(LabelField, (int)LabelState.SwitchOff);
                }
            }
        }

        public override void OnGazeExit(Gazer gazer)
        {
            interactive = false;
            if ((labelsAnimation != null) && (labelsAnimation.gameObject.activeInHierarchy == true))
            {
                labelsAnimation.SetInteger(LabelField, (int)LabelState.None);
            }
        }

        public override Gazer.SoundEffectType OnInteract(Gazer gazer)
        {
            if (interactive == true)
            {
                // Toggle state
                LeverGroup.IsOn = !LeverGroup.IsOn;

                // Run gaze exit
                OnGazeExit(gazer);
            }
            return Gazer.SoundEffectType.None;
        }

        protected override void OnThisTierChanged(ResizingTier obj)
        {
            if ((trigger != null) && (gameObject.activeInHierarchy == true))
            {
                trigger.IsEnabled = false;
                OnGazeExit(null);
            }
            //AssociatedCode.OnTierChanged();
            UpdateAnimation();
        }

        void OnStateChanged(LeverGroup source, bool before, bool after)
        {
            if (before != after)
            {
                // Play sound effect
                if (after == true)
                {
                    trueStateSoundEffect.Play();
                }
                else
                {
                    falseStateSoundEffect.Play();
                }

                // Play animation
                switchAnimation.SetBool(StateField, after);
            }
        }

        protected virtual void OnBeforeResize(ResizeParent obj)
        {
            if ((trigger != null) && (gameObject.activeInHierarchy == true))
            {
                trigger.IsEnabled = false;
                OnGazeExit(null);
            }
            UpdateAnimation();
        }

        protected virtual void OnAfterResize(ResizeParent obj)
        {
            if ((trigger != null) && (gameObject.activeInHierarchy == true))
            {
                trigger.IsEnabled = (obj.CurrentTier == ThisTier);
            }
            UpdateAnimation();
        }

        void UpdateAnimation()
        {
            if (switchAnimation.gameObject.activeInHierarchy == true)
            {
                switchAnimation.SetBool(StateField, LeverGroup.IsOn);
            }
        }
    }
}
