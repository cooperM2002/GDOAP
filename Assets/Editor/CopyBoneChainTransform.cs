using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CopyBoneChainTransform : EditorWindow
{
    private Transform source;
    private Transform target;
    private bool includeRoot;
    private bool copyScale;

    [MenuItem("Tools/Copy Hierarchy Pose")]
    private static void Open() => GetWindow<CopyBoneChainTransform>("Copy Hierarchy Pose");

    private void OnGUI() {
        EditorGUILayout.HelpBox(
            "Choose matching bone roots from two scene instances. Copies local position and rotation for every descendant, including inactive bones. Child names and hierarchy must match; underscores and periods are treated as equivalent everywhere in each name.",
            MessageType.Info);
        source = (Transform)EditorGUILayout.ObjectField("Source (posed arm)", source, typeof(Transform), true);
        target = (Transform)EditorGUILayout.ObjectField("Target (new arm)", target, typeof(Transform), true);
        includeRoot = EditorGUILayout.Toggle("Also copy root pose", includeRoot);
        copyScale = EditorGUILayout.Toggle("Also copy local scale", copyScale);
        using(new EditorGUI.DisabledScope(source == null || target == null || EditorApplication.isPlaying)) {
            if(GUILayout.Button("Copy Entire Pose")) CopyPose();
        }
    }

    private static string MatchName(string boneName) {
        // Normalize every underscore on both source and target for matching only.
        return boneName.Replace('_', '.');
    }

    private static bool Collect(Transform bone, string path, Dictionary<string, Transform> map) {
        if(map.ContainsKey(path)) return false;
        map.Add(path, bone);
        foreach(Transform child in bone) {
            // Slash-separated names identify each bone relative to the chosen root.
            if(child.name.Contains("/")) return false;
            string matchingName = MatchName(child.name);
            string childPath = path.Length == 0 ? matchingName : path + "/" + matchingName;
            if(!Collect(child, childPath, map)) return false;
        }
        return true;
    }

    private void CopyPose() {
        if(source == target || source.IsChildOf(target) || target.IsChildOf(source)) {
            EditorUtility.DisplayDialog("Cannot copy", "Choose two separate arm hierarchies.", "OK");
            return;
        }
        if(EditorUtility.IsPersistent(source) || EditorUtility.IsPersistent(target)) {
            EditorUtility.DisplayDialog("Cannot copy", "Use instances in the scene or bones opened in Prefab Mode, rather than model assets in the Project window.", "OK");
            return;
        }

        var from = new Dictionary<string, Transform>();
        var to = new Dictionary<string, Transform>();
        if(!Collect(source, "", from) || !Collect(target, "", to)) {
            EditorUtility.DisplayDialog("Cannot copy", "Bone paths are ambiguous. Check for duplicate sibling names (including names that become identical when underscores are replaced with periods) or names containing '/'.", "OK");
            return;
        }
        foreach(string path in from.Keys) {
            if(!to.ContainsKey(path)) {
                EditorUtility.DisplayDialog("Hierarchy mismatch", "Target is missing: " + path + "\nChoose corresponding bone roots. Nothing was changed.", "OK");
                return;
            }
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Copy Hierarchy Pose");
        int copied = 0;
        foreach(var entry in from) {
            if(entry.Key.Length == 0 && !includeRoot) continue;
            Transform destination = to[entry.Key];
            Undo.RecordObject(destination, "Copy Hierarchy Pose");
            //destination.localPosition = entry.Value.localPosition;
            destination.localRotation = entry.Value.localRotation;
            if(copyScale) destination.localScale = entry.Value.localScale;
            if(PrefabUtility.IsPartOfPrefabInstance(destination))
                PrefabUtility.RecordPrefabInstancePropertyModifications(destination);
            EditorUtility.SetDirty(destination);
            copied++;
        }
        Undo.CollapseUndoOperations(group);
        SceneView.RepaintAll();
        Debug.Log("Copied pose to " + copied + " transforms.", target);
    }
}