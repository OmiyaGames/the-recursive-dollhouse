using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour, IMouseLockChanger
    {
        public class MovementAxisEventArgs : System.EventArgs
        {
            public float Sensitivity
            {
                get;
                set;
            }
        }

        public class HeadBobEventArgs : System.EventArgs
        {
            public bool Enable
            {
                get;
                set;
            }
        }

        public static FirstPersonController Instance
        {
            get;
            private set;
        }

        public static Camera InstanceCamera
        {
            get
            {
                if(Instance.m_Camera == null)
                {
                    Instance.m_Camera = Camera.main;
                }
                return Instance.m_Camera;
            }
        }

        public delegate void OnGetMovementAxis(FirstPersonController sender, MovementAxisEventArgs args);
        public event OnGetMovementAxis OnGetXMovementAxis;
        public event OnGetMovementAxis OnGetYMovementAxis;

        public delegate void OnGetHeadBob(FirstPersonController sender, HeadBobEventArgs args);
        public event OnGetHeadBob OnGetHeadBobEnabled;

        [SerializeField]
        protected bool m_IsWalking;
        [SerializeField]
        protected float m_WalkSpeed;
        [SerializeField]
        protected float m_RunSpeed;
        [SerializeField]
        [Range(0f, 1f)]
        protected float m_RunstepLenghten;
        [SerializeField]
        protected float m_JumpSpeed;
        [SerializeField]
        protected float m_SpringSpeed;
        [SerializeField]
        protected float m_StickToGroundForce;
        [SerializeField]
        protected float m_GravityMultiplier;
        [SerializeField]
        protected MouseLook m_MouseLook;
        [SerializeField]
        protected bool m_UseFovKick;
        [SerializeField]
        protected FOVKick m_FovKick = new FOVKick();
        [SerializeField]
        protected bool m_UseHeadBob;
        [SerializeField]
        protected CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField]
        protected LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField]
        protected float m_StepInterval;
        [SerializeField]
        protected AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField]
        protected AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField]
        protected AudioClip m_LandSound;           // the sound played when character touches back on ground.
        [SerializeField]
        protected float hitForceMultiplier = 0.1f;

        protected Camera m_Camera;
        protected bool m_Spring;
        protected bool m_Jump;
        protected float m_YRotation;
        protected Vector2 m_Input;
        protected Vector3 m_MoveDir = Vector3.zero;
        protected CharacterController m_CharacterController;
        protected CollisionFlags m_CollisionFlags;
        protected bool m_PreviouslyGrounded;
        protected Vector3 m_OriginalCameraPosition;
        protected float m_StepCycle;
        protected float m_NextStep;
        protected bool m_Jumping;
        protected AudioSource m_AudioSource;
        protected bool m_Slowdown = false;
        private readonly MovementAxisEventArgs m_XMovementArgs = new MovementAxisEventArgs();
        private readonly MovementAxisEventArgs m_YMovementArgs = new MovementAxisEventArgs();
        private readonly HeadBobEventArgs m_HeadBobArgs = new HeadBobEventArgs();

        public virtual void StartSlowdown(bool diveIn)
        {
            m_Slowdown = true;
        }

        public virtual void StopSlowdown()
        {
            m_Slowdown = false;
        }

        public void ActivateSpring()
        {
            m_Spring = true;
        }

        protected void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        protected virtual void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_OriginalCameraPosition = InstanceCamera.transform.localPosition;
            m_FovKick.Setup(InstanceCamera);
            m_HeadBob.Setup(InstanceCamera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
            m_MouseLook.Init(transform, InstanceCamera.transform);
            //m_ZoomEffect.Stop();
        }

        public virtual bool IsGrounded
        {
            get
            {
                return m_CharacterController.isGrounded;
            }
        }

        // Update is called once per frame
        protected void Update()
        {
            RotateView();

            // the jump state needs to read here to make sure it is not missed
            GetJump(ref m_Jump);

            if (!m_PreviouslyGrounded && IsGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!IsGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = IsGrounded;
        }


        protected virtual void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        protected void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_XMovementArgs.Sensitivity = desiredMove.x * speed;
            m_YMovementArgs.Sensitivity = desiredMove.z * speed;
            if (OnGetXMovementAxis != null)
            {
                OnGetXMovementAxis(this, m_XMovementArgs);
            }
            if (OnGetYMovementAxis != null)
            {
                OnGetYMovementAxis(this, m_YMovementArgs);
            }
            m_MoveDir.x = m_XMovementArgs.Sensitivity;
            m_MoveDir.z = m_YMovementArgs.Sensitivity;


            if (IsGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if(m_Spring)
                {
                    m_MoveDir.y = m_SpringSpeed;
                    PlayJumpSound();
                    m_Spring = false;
                    m_Jump = false;
                    m_Jumping = true;
                }
                else if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);
        }

        public virtual void UpdateMouseLock()
        {
            m_MouseLook.UpdateCursorLock();
        }

        protected virtual void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }

        protected virtual void GetJump(ref bool jump)
        {
            if (!jump)
            {
                jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }

        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        protected virtual void PlayFootStepAudio()
        {
            if ((!IsGrounded) || (m_Slowdown == true))
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            m_HeadBobArgs.Enable = m_UseHeadBob;
            if(OnGetHeadBobEnabled != null)
            {
                OnGetHeadBobEnabled(this, m_HeadBobArgs);
            }
            if (m_HeadBobArgs.Enable == false)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && IsGrounded)
            {
                InstanceCamera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = InstanceCamera.transform.localPosition;
                newCameraPosition.y = InstanceCamera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = InstanceCamera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            InstanceCamera.transform.localPosition = newCameraPosition;
        }


        protected virtual void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        protected virtual void RotateView()
        {
            m_MouseLook.LookRotation(transform, InstanceCamera.transform, this);
        }


        protected void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * hitForceMultiplier, hit.point, ForceMode.Impulse);
        }
    }
}
