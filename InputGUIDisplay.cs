using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads an input action asset from unitys new input system and shows it in a GUI update. Has not been tested extensively with various mappings so expect bugs.
/// </summary>
public class InputGUIDisplay : MonoBehaviour
{
    /// <summary>
    /// The input asset to read off of. Make sure to asign it in the inspector.
    /// </summary>
    [SerializeField] InputActionAsset input;
    [SerializeField] bool showGUI;
    [SerializeField] Vector3 GUI_Position;
    [SerializeField] Color textBackGroundColor = new Color(0.2F, 0.2F, 0.2F, 0.7F);
    [SerializeField] GUIStyle GUI_Style; 
    string text;
    float previousHeight;
    float previousWidth;
    Color previousBackgroundColor;
    /// <summary> 
    /// Dictionary&lt;Action map name, Dictionary&lt;action name, Dictionary&lt;input source, List&lt;control name&gt;&gt;&gt;&gt;
    /// </summary>
    Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> bindingsDictionary;
     
    //Builds a table out of the control map and then sets some text in the scene.
    void Start()
    { 
        text = "";
        bindingsDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        var actionMaps = input.actionMaps;
        foreach (var actionMap in actionMaps)
        {
            if (!bindingsDictionary.ContainsKey(actionMap.name))
                bindingsDictionary.Add(actionMap.name, new Dictionary<string, Dictionary<string, List<string>>>());

            var bindings = actionMap.bindings;
            foreach (var binding in bindings)
            {
                if (!bindingsDictionary[actionMap.name].ContainsKey(binding.action))
                    bindingsDictionary[actionMap.name].Add(binding.action, new Dictionary<string, List<string>>());

                var parseInputArray = binding.effectivePath.Split('<', '>');
                if (parseInputArray.Length > 2)
                {
                    var inputSource = parseInputArray[1];
                    if (!bindingsDictionary[actionMap.name][binding.action].ContainsKey(inputSource))
                        bindingsDictionary[actionMap.name][binding.action].Add(inputSource, new List<string>());

                    var input = parseInputArray[2].Split('/');
                    if (input.Length == 2)//keyboard input
                        bindingsDictionary[actionMap.name][binding.action][inputSource].Add(input[1]);
                    else if (input.Length == 3)//controller
                        bindingsDictionary[actionMap.name][binding.action][inputSource].Add($"{input[1]}-{input[2]}");
                }
            }
            foreach (var actionMapTypes in bindingsDictionary)
            {
                text += $"{actionMapTypes.Key}";
                foreach (var actions in actionMapTypes.Value)
                {
                    text += $"\n  <color=yellow>{actions.Key}</color>";
                    foreach (var inputSource in actions.Value)
                    {
                        text += $"\n    <color=blue>{inputSource.Key}</color> [";
                        int inputCount = 0;
                        foreach (var input in inputSource.Value)
                        {
                            if (inputCount >= inputSource.Value.Count-1)
                                text += $"{input}";
                            else
                                text += $"{input},";
                            inputCount++;
                        }
                        text += "]";
                    }
                }
                text += "\n";
            }
        } 
    }
    private void OnGUI()
    {
        if (!showGUI) return;
          
        float width = Screen.width / 1200.0f;
        float height = Screen.height / 800.0f;
        if (width != previousWidth || height != previousHeight || GUI_Style.normal.background == null || previousBackgroundColor != textBackGroundColor)
        {
            previousBackgroundColor = textBackGroundColor;
            previousWidth = width;
            previousHeight = height;
            GUI_Style.normal.background =
                MakeTex(Mathf.CeilToInt(width), Mathf.CeilToInt(height), textBackGroundColor);
        }
        GUI.matrix = Matrix4x4.TRS(GUI_Position, Quaternion.identity, new Vector3(width, height, 1.0f));
          
        GUILayout.Label(text, GUI_Style);
    }
    Texture2D MakeTex(int width, int height, Color col)
    {
        var pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }

        var result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
