using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public Transform cameraTransform;
    public float rayDistance;
    
    [Header("Interact")]
    [SerializeField] private InputActionReference interact;

    private bool _canInteract = true;

    private void Awake()
    {
        DialogSystemController.onConversationEnded += ResetActivate;
    }

    void Update()
    {
        if (_canInteract && interact.action.WasPressedThisFrame())
        {
            var camPos = cameraTransform.position;
            var camForward = cameraTransform.forward;
            Ray r = new Ray(camPos, camForward);
            if (Physics.Raycast(r, out var hitInfo, rayDistance))
            {
                if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    _canInteract = false;
                    interactable.Interact();
                }
            }
        }
    }

    public void ResetActivate()
    {
        StartCoroutine(ResetInteraction());
    }

    IEnumerator ResetInteraction()
    {
        yield return new WaitForEndOfFrame();
        _canInteract = true;
    }

    private void OnEnable()
    {
        interact.action.Enable();
    }
    
    private void OnDisable()
    {
        interact.action.Disable();
    }

    private void OnDestroy()
    {
        DialogSystemController.onConversationEnded -= ResetActivate;
    }
}
