using System;
using UnityEngine;
using UnityEngine.Events;

public class ActionHandler : MonoBehaviour
{
    [Range(0f, 1f)]
    public float TimeBetweenKeys = 0.3f;
    public KeyAction[] KeyActions;

    [Serializable]
    public struct KeyAction
    {
        public string keyName;
        public KeyCode[] keyCodes;
        public UnityEvent action;

        [HideInInspector] public int currentStep;
        [HideInInspector] public float timer;
    }

    void Update()
    {
        for (int i = 0; i < KeyActions.Length; i++)
        {
            CheckComboSequence(ref KeyActions[i]);
        }
    }

    private void CheckComboSequence(ref KeyAction combo)
    {
        if (combo.keyCodes == null || combo.keyCodes.Length == 0) return;

        combo.timer += Time.deltaTime;
        
        if (combo.timer > TimeBetweenKeys)
        {
            combo.currentStep = 0;
            combo.timer = 0f;
        }

        KeyCode expectedKey = combo.keyCodes[combo.currentStep];
        if (Input.GetKeyDown(expectedKey))
        {
            combo.currentStep++;
            combo.timer = 0f;

            if (combo.currentStep >= combo.keyCodes.Length)
            {
                Debug.Log($"Action: {combo.keyName} is been executed");
                combo.action?.Invoke();
                combo.currentStep = 0;
            }
        }
    }
}