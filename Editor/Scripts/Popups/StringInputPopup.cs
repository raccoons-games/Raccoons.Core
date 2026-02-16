using UnityEditor;
using UnityEngine;
using System;

public class StringInputPopup : EditorWindow
{
    private string _input = "";
    private Action<string> _onClose;
    private bool _focusRequested;

    public static void Show(string title, string defaultValue, Action<string> onClose)
    {
        var window = CreateInstance<StringInputPopup>();
        window.titleContent = new GUIContent(title);
        window._input = defaultValue;
        window._onClose = onClose;
        window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 300, 100);
        window.ShowUtility(); // Modal-style window
    }

    private void OnGUI()
    {
        HandleKeyboard();

        GUILayout.Label("Enter name:", EditorStyles.boldLabel);

        GUI.SetNextControlName("InputField");
        _input = EditorGUILayout.TextField(_input);

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("OK"))
        {
            Accept();
        }

        if (GUILayout.Button("Cancel"))
        {
            Cancel();
        }

        EditorGUILayout.EndHorizontal();

        // Set focus only once
        if (!_focusRequested)
        {
            EditorGUI.FocusTextInControl("InputField");
            _focusRequested = true;
        }
    }

    private void HandleKeyboard()
    {
        var e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                e.Use(); // prevent default
                Accept();
            }
            else if (e.keyCode == KeyCode.Escape)
            {
                e.Use();
                Cancel();
            }
        }
    }

    private void Accept()
    {
        _onClose?.Invoke(_input);
        Close();
    }

    private void Cancel()
    {
        _onClose?.Invoke(null);
        Close();
    }
}