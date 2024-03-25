using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AQM.Tools
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed;
        public float sprintSpeed;
        public Transform orientation;
        public float groundDrag;
        
        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask whatIsGround;
        bool _grounded;

        [Header("Keys")] 
        [SerializeField] private InputActionReference sprint;
        
        private Vector2 _inputVector;
        private Vector3 _moveDirection;
        private Rigidbody _rb;
        private InputActions.PlayerActions _playerInputs;
        private InputAction _inputControls;
        private float _moveSpeed;
        private bool _sprintingPressed;

        private void Awake()
        {
            Application.targetFrameRate = 120;
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            _inputControls = InputManager.Instance.playerInputActions.Player.Movement;
            DialogSystemController.onConversationStarted += OnStartConversation;
            DialogSystemController.onConversationEnded+= OnConversationEnded;
        }

        private void Update()
        {
            // ground check
            _grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
            
            OnMovementInput();
            SpeedControl();
            StateHandler();
            
            // handle drag
            if (_grounded)
                _rb.drag = groundDrag;
            else
                _rb.drag = 0;
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void OnMovementInput()
        {
            _inputVector = _inputControls.ReadValue<Vector2>();
            var transVar = transform;
        }

        private void StateHandler()
        {
            if (_grounded && sprint.action.IsPressed()) 
            {
                // Mode - sprinting
                _moveSpeed = sprintSpeed;
            }else if (_grounded) 
            {
                // Mode - walking
                _moveSpeed = walkSpeed;
            }
        }

        private void MovePlayer()
        {
            _moveDirection = orientation.forward * _inputVector.y + orientation.right * _inputVector.x;
            _rb.AddForce(_moveDirection.normalized * (_moveSpeed * 10f), ForceMode.Force);
        }
        
        private void SpeedControl()
        {
            var velocity = _rb.velocity;
            Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);

            // limit velocity if needed
            if(flatVel.magnitude > _moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * _moveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }

        private void OnEnable()
        {
            sprint.action.Enable();
        }
        
        private void OnDisable()
        {
            sprint.action.Disable();
        }

        private void OnStartConversation()
        {
            InputManager.Instance.ToogleActionMap(InputManager.Instance.playerInputActions.UI);
        }
        
        private void OnConversationEnded()
        {
            InputManager.Instance.ToogleActionMap(InputManager.Instance.playerInputActions.Player);
        }

        private void OnDestroy()
        {
            DialogSystemController.onConversationStarted -= OnStartConversation;
            DialogSystemController.onConversationEnded-= OnConversationEnded;
        }
    }
}
