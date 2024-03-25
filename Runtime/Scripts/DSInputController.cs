using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DialogSystemController))]
public class DSInputController : MonoBehaviour
{
    [SerializeField] private List<InputActionReference> inputKeys;

    private DialogSystemController _dialogSystemController;
    private Coroutine _dialogCo;
    private void Awake()
    {
        DialogSystemController.onShowNewDialog += OnNewDialog;
        _dialogSystemController = GetComponent<DialogSystemController>();
    }
    
    private void OnNewDialog(DSDialog node)
    {
        DisableInputKeys();
        EnableInputKeys();
    }
    
    private void NextMessageCallback(InputAction.CallbackContext context)
    {
        NextMessage();
    }
    
    private IEnumerator NextMessageCoroutine()
    {
        yield return new WaitForSeconds(_dialogSystemController.DialogTime);
        NextMessage();
    }

    private void NextMessage()
    {
        DisableInputKeys();
        DDEvents.onGetNextNode?.Invoke(0);
    }
    
    private void EnableInputKeys()
    {
        foreach (var key in inputKeys)
            key.action.started += NextMessageCallback;
        
        if (_dialogSystemController.DialogTimer)
        {
            _dialogCo = StartCoroutine(NextMessageCoroutine());
        }
    }
        
    private void DisableInputKeys()
    {
        foreach (var key in inputKeys)
            key.action.started -= NextMessageCallback;
    }

    private void OnEnable()
    {
        foreach (var key in inputKeys) key.action.Enable();
    }

    private void OnDisable()
    {
        foreach (var key in inputKeys) key.action.Disable();
    }

    private void OnDestroy()
    {
        DisableInputKeys();
        DialogSystemController.onShowNewDialog -= OnNewDialog;
        if(_dialogCo != null) StopCoroutine(_dialogCo);
    }
}
