using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public interface IMouseLockChanger
    {
        void UpdateMouseLock();
    }

    [Serializable]
    public class MouseLook
    {
        public class RotationAxisEventArgs : EventArgs
        {
            public float Sensitivity
            {
                get;
                set;
            }
        }
        public class SmoothEventArgs : EventArgs
        {
            public bool Smooth
            {
                get;
                set;
            }
        }

        public delegate void OnGetRotationAxis(MouseLook sender, RotationAxisEventArgs args);
        public event OnGetRotationAxis OnGetXRotationAxis;
        public event OnGetRotationAxis OnGetYRotationAxis;

        public delegate void OnGetSmooth(MouseLook sender, SmoothEventArgs args);
        public event OnGetSmooth OnGetIsSmooth;

        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;
        private readonly RotationAxisEventArgs m_finalXSensitivity = new RotationAxisEventArgs();
        private readonly RotationAxisEventArgs m_finalYSensitivity = new RotationAxisEventArgs();
        private readonly SmoothEventArgs m_finalSmooth = new SmoothEventArgs();

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }


        public void LookRotation(Transform character, Transform camera, IMouseLockChanger controller)
        {
            m_finalYSensitivity.Sensitivity = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            if (OnGetXRotationAxis != null)
            {
                OnGetXRotationAxis(this, m_finalYSensitivity);
            }

            m_finalXSensitivity.Sensitivity = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;
            if (OnGetYRotationAxis != null)
            {
                OnGetYRotationAxis(this, m_finalXSensitivity);
            }

            m_CharacterTargetRot *= Quaternion.Euler (0f, m_finalYSensitivity.Sensitivity, 0f);
            m_CameraTargetRot *= Quaternion.Euler (-m_finalXSensitivity.Sensitivity, 0f, 0f);

            if(clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);

            m_finalSmooth.Smooth = smooth;
            if(OnGetIsSmooth != null)
            {
                OnGetIsSmooth(this, m_finalSmooth);
            }
            if(m_finalSmooth.Smooth == true)
            {
                character.localRotation = Quaternion.Slerp (character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            controller.UpdateMouseLock();
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if(!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
