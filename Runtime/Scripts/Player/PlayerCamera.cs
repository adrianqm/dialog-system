using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AQM.Tools
{
    public class PlayerCamera : MonoBehaviour
    {
        
        public float sensX;
        public float sensY;
        public Transform orientation;
        public float lookAtTime;
        
        private float _mDesiredYaw;
        private float _mDesiredPitch;
        private InputAction _inputControls;
        private bool _isMoving;
        private Coroutine _smoothMove;
        private bool _isPaused;
        
        private InputActions.PlayerActions _playerInputs;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _inputControls = InputManager.Instance.playerInputActions.Player.Look;

            DDEvents.actorConversationStarted += LookAt;
            DialogSystemController.onConversationEnded += OnConversationEnded;
        }

        private void Update()
        {
            if(_isPaused) return;
            
            // get input
            var inputMouse = _inputControls.ReadValue<Vector2>();

            _mDesiredYaw += inputMouse.x * sensX * Time.deltaTime;
            _mDesiredPitch -= inputMouse.y * sensY * Time.deltaTime;
            _mDesiredPitch = Mathf.Clamp(_mDesiredPitch, -90f, 90f);
            
            // rotate and transform
            transform.rotation = Quaternion.Euler(_mDesiredPitch,_mDesiredYaw, 0);
            orientation.rotation = Quaternion.Euler(0,_mDesiredYaw, 0);
        }

        private void LookAt(Transform lookAtPoint)
        {
            _isPaused = true;
            
            //Start new look-at coroutine
            if (_smoothMove == null)
                _smoothMove = StartCoroutine(LookAtSmoothly(lookAtPoint.position, lookAtTime));
            else
            {
                //Stop old one then start new one
                StopCoroutine(_smoothMove);
                _smoothMove = StartCoroutine(LookAtSmoothly(lookAtPoint.position, lookAtTime));
            }
        }
        
        IEnumerator LookAtSmoothly(Vector3 worldPosition, float duration)
        {
            Quaternion startRot = transform.rotation;
            
            for (float t=0f; t < duration; t += Time.deltaTime) {
                Quaternion endRot = Quaternion.LookRotation(worldPosition - transform.position);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t / duration);
                yield return null;
            }

            Transform trans;
            (trans = transform).rotation = Quaternion.LookRotation(worldPosition - transform.position);
            Vector3 angles = trans.rotation.eulerAngles;
            _mDesiredPitch = angles.x;
            _mDesiredYaw = angles.y;
        }

        private void OnConversationEnded()
        {
            _isPaused = false;
        }

        private void OnDestroy()
        {
            DDEvents.actorConversationStarted -= LookAt;
            DialogSystemController.onConversationEnded -= OnConversationEnded;
        }
    }
}
