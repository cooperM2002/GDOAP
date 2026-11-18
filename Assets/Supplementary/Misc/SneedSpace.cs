using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class SneedSpace : MonoBehaviour {

    //====================//
    // Inspector Settings //
    //====================//
    #region inspector

    //struct to hold all debug data
    [Serializable] public struct DebugGroup {
        [ColorUsage(false, false)] public Color debugColor;
        [Tooltip("Leave blank to use the class name automatically")]
        public string customIdentifier;
        public bool hideDebug;
        public bool hideError;
        public bool hideInfo;
    }

    //collapsible area
    [Header("Debug Settings")]
    [SerializeField]
    private DebugGroup settings = new DebugGroup {
        hideDebug = false,
        hideError = false,
        debugColor = new Color(0.77f, 0.11f, 0.56f) //default pink #c41d8f
    };


    #endregion
    //=============//
    // Console Log //
    //=============//
    #region


    ////fallback logic for label, override
    //protected virtual string Identifier => string.IsNullOrEmpty(settings.customIdentifier)
    //    ? GetType().Name
    //    : settings.customIdentifier;
    protected string identifierExtra = "";

    protected string Identifier {
        get {
            string baseIdentifier = string.IsNullOrEmpty(settings.customIdentifier)
                ? GetType().Name
                : settings.customIdentifier;

            return string.IsNullOrEmpty(identifierExtra)
                ? baseIdentifier
                : $"{baseIdentifier} {identifierExtra}";
        }
    }


    protected void OnValidate() {
        if(string.IsNullOrEmpty(settings.customIdentifier)) {
            settings.customIdentifier = GetType().Name;
        }
    }

    protected void PrintMessage(string message) {
        if(settings.hideDebug) return;

        string hexColor = ColorUtility.ToHtmlStringRGB(settings.debugColor);
        Debug.Log($"<color=#{hexColor}><b>[{Identifier}]</b></color>:\n{message}");
    }

    protected void PrintError(string message) {
        if(settings.hideError) return;

        Debug.Log($"<color=#d94141><b>[{Identifier}] ERROR: </b></color>:\n{message}");
    }

    protected void PrintInfo(string message) {

        string hexColor = ColorUtility.ToHtmlStringRGB(settings.debugColor);
        Debug.Log($"<color=#{hexColor}><b>[{Identifier}]</b></color>:\n{message}");
    }


    #endregion
    //=============================//
    // STARTUP INFO BUFFER METHODS //
    //=============================//
    #region startinfo


    /* * * * * * * * *
     * Usage Example *
     * * * * * * * * * 
        ClearPrintInfo();

        AddInfoLine("NPC Startup Info");
        AddInfoHeader("References");
        AddInfoRefCheck("Animator", animator);
        AddInfoRefCheck("NavMeshAgent", agent);
        AddInfoRefCheck("Rigidbody", rb);
        AddInfoHeader("Settings");
        AddInfoLine($"Health: {health}");
        AddInfoLine($"Move Speed: {moveSpeed}");
        AddInfoHeader("Logic Checks");
        AddInfoCheck("Health above zero", health > 0);
        AddInfoCheck("Move speed positive", moveSpeed > 0);

        PrintStartInfo();
     */

    private readonly StringBuilder printInfoBuilder = new StringBuilder();

    protected void ClearPrintInfo() { printInfoBuilder.Clear(); }
    protected void AddInfoBlank() { printInfoBuilder.AppendLine(); }
    protected void AddInfoLine(string line) { printInfoBuilder.AppendLine($"  {line}"); }//#6883fc
    protected void AddInfoHeader(string header) { printInfoBuilder.AppendLine($"<b>{header}</b>"); }

    protected void AddInfoValue<T>(string label, T value) {
        printInfoBuilder.AppendLine($"  • {label} = <color=#f54c87><i>{value}</i></color>");////#6883fc
    }

    protected void AddInfoDict<TKey, TValue>(
        string header, 
        Dictionary<TKey, TValue> dict, 
        Func<TValue, string> valFormatter = null) 
    {
        string formattedString = string.Join(
            Environment.NewLine, 
            dict.Select( kvp => {

                string valStr = valFormatter!=null 
                    ? valFormatter(kvp.Value) 
                    : kvp.Value?.ToString() ?? "null";

                return $"    {kvp.Key}   =    {valStr}";
                })
            );
        printInfoBuilder.AppendLine($"  • <b>{header}:</b>\n{formattedString}");
    }

    protected void AddInfoCheck(string label, bool condition) {
        string stringTrue = $"<color=#19a849><b>OK</b></color>";
        string stringFalse = $"<color=#d94141><b>FAILED</b></color>";
        string result = condition ? stringTrue : stringFalse;

        printInfoBuilder.AppendLine($"  • {label}: {result}");
    }

    protected void AddInfoRefCheck(string label, UnityEngine.Object reference) {
        string stringTrue = $"<color=#15945b><b>FOUND</b></color>";
        string stringFalse = $"<color=#c7473c><b>MISSING</b></color>";
        string result = reference != null ? stringTrue : stringFalse;

        printInfoBuilder.AppendLine($"  • {label}: {result}");
    }

    protected void PrintStartInfo() {
        if(settings.hideInfo) return;
        if(printInfoBuilder.Length == 0) {
            PrintMessage("No startup info added.");
            return;
        }

        PrintInfo(printInfoBuilder.ToString());
    }


    #endregion

}


