using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DialogSystemController))]
public class DSInputController : MonoBehaviour
{
    
    [Header("Dialog Config")]
        
    [SerializeField] private bool dialogTimer;
    [SerializeField] private float dialogTime = 2f;
    
    [SerializeField] private List<InputActionReference> inputKeys;
    
    private Coroutine _dialogCo;
    private void Awake()
    {
        DialogSystemController.onShowNewDialog += OnNewDialog;
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
        yield return new WaitForSeconds(dialogTime);
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
        
        if (dialogTimer)
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
