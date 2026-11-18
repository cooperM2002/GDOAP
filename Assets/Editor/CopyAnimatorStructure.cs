#if UNITY_EDITOR

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimatorStructureCopier
{
    [MenuItem("Tools/Animator/Copy Selected Animator Structure")]
    private static void CopySelectedAnimatorStructure() {
        Animator animator = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<Animator>()
            : null;

        RuntimeAnimatorController runtimeController =
            animator != null
                ? animator.runtimeAnimatorController
                : Selection.activeObject as RuntimeAnimatorController;

        if(runtimeController == null) {
            Debug.LogError(
                "Select a GameObject with an Animator, " +
                "or select an Animator Controller asset."
            );

            return;
        }

        AnimatorController controller =
            GetBaseController(runtimeController);

        if(controller == null) {
            Debug.LogError(
                "Could not find an AnimatorController. " +
                "The selected controller may not be a standard Mecanim controller."
            );

            return;
        }

        StringBuilder output = new StringBuilder(16000);

        output.AppendLine("========================================");
        output.AppendLine("ANIMATOR CONTROLLER STRUCTURE");
        output.AppendLine("========================================");
        output.AppendLine($"Controller: {controller.name}");
        output.AppendLine(
            $"Asset: {AssetDatabase.GetAssetPath(controller)}"
        );

        if(animator != null) {
            output.AppendLine($"GameObject: {animator.gameObject.name}");
            output.AppendLine($"Apply Root Motion: {animator.applyRootMotion}");
            output.AppendLine($"Update Mode: {animator.updateMode}");
            output.AppendLine($"Culling Mode: {animator.cullingMode}");
        }

        output.AppendLine();

        DumpOverrides(output, runtimeController);
        DumpParameters(output, controller);
        DumpLayers(output, controller);

        EditorGUIUtility.systemCopyBuffer = output.ToString();

        Debug.Log(
            $"Copied Animator structure for '{controller.name}' " +
            $"to the clipboard.\n\n{output}"
        );
    }

    [MenuItem(
        "Tools/Animator/Copy Selected Animator Structure",
        true
    )]
    private static bool ValidateCopySelectedAnimatorStructure() {
        if(Selection.activeObject is RuntimeAnimatorController)
            return true;

        if(Selection.activeGameObject == null)
            return false;

        Animator animator =
            Selection.activeGameObject.GetComponent<Animator>();

        return animator != null &&
               animator.runtimeAnimatorController != null;
    }

    private static AnimatorController GetBaseController(
        RuntimeAnimatorController runtimeController
    ) {
        while(runtimeController is AnimatorOverrideController overrideController) {
            runtimeController = overrideController.runtimeAnimatorController;
        }

        return runtimeController as AnimatorController;
    }

    private static void DumpOverrides(
        StringBuilder output,
        RuntimeAnimatorController runtimeController
    ) {
        if(!(runtimeController is AnimatorOverrideController overrideController))
            return;

        output.AppendLine("ANIMATION OVERRIDES");
        output.AppendLine("----------------------------------------");

        List<KeyValuePair<AnimationClip, AnimationClip>> overrides =
            new List<KeyValuePair<AnimationClip, AnimationClip>>();

        overrideController.GetOverrides(overrides);

        foreach(KeyValuePair<AnimationClip, AnimationClip> pair in overrides) {
            string original = pair.Key != null
                ? pair.Key.name
                : "<None>";

            string replacement = pair.Value != null
                ? pair.Value.name
                : "<Uses Original>";

            output.AppendLine($"- {original} -> {replacement}");
        }

        output.AppendLine();
    }

    private static void DumpParameters(
        StringBuilder output,
        AnimatorController controller
    ) {
        output.AppendLine("PARAMETERS");
        output.AppendLine("----------------------------------------");

        if(controller.parameters.Length == 0) {
            output.AppendLine("<No parameters>");
            output.AppendLine();
            return;
        }

        foreach(AnimatorControllerParameter parameter in controller.parameters) {
            output.AppendLine(
                $"- {parameter.name} [{parameter.type}]" +
                GetDefaultValue(parameter)
            );
        }

        output.AppendLine();
    }

    private static string GetDefaultValue(
        AnimatorControllerParameter parameter
    ) {
        switch(parameter.type) {
            case AnimatorControllerParameterType.Float:
            return $" Default={parameter.defaultFloat:0.###}";

            case AnimatorControllerParameterType.Int:
            return $" Default={parameter.defaultInt}";

            case AnimatorControllerParameterType.Bool:
            return $" Default={parameter.defaultBool}";

            case AnimatorControllerParameterType.Trigger:
            return "";

            default:
            return "";
        }
    }

    private static void DumpLayers(
        StringBuilder output,
        AnimatorController controller
    ) {
        output.AppendLine("LAYERS");
        output.AppendLine("========================================");

        AnimatorControllerLayer[] layers = controller.layers;

        for(int i = 0; i < layers.Length; i++) {
            AnimatorControllerLayer layer = layers[i];

            output.AppendLine();
            output.AppendLine($"LAYER {i}: {layer.name}");
            output.AppendLine("----------------------------------------");
            output.AppendLine($"Weight: {layer.defaultWeight:0.###}");
            output.AppendLine($"Blending: {layer.blendingMode}");
            output.AppendLine(
                $"Avatar Mask: " +
                $"{(layer.avatarMask != null ? layer.avatarMask.name : "<None>")}"
            );
            output.AppendLine($"IK Pass: {layer.iKPass}");
            output.AppendLine($"Synced Layer Index: {layer.syncedLayerIndex}");
            output.AppendLine(
                $"Synced Layer Affects Timing: " +
                $"{layer.syncedLayerAffectsTiming}"
            );

            output.AppendLine();

            DumpStateMachine(
                output,
                layer.stateMachine,
                "",
                layer.stateMachine
            );
        }
    }

    private static void DumpStateMachine(
        StringBuilder output,
        AnimatorStateMachine stateMachine,
        string indent,
        AnimatorStateMachine parentStateMachine
    ) {
        output.AppendLine(
            $"{indent}STATE MACHINE: {stateMachine.name}"
        );

        output.AppendLine(
            $"{indent}Default State: " +
            $"{GetStateName(stateMachine.defaultState)}"
        );

        DumpBehaviours(
            output,
            stateMachine.behaviours,
            indent + "  "
        );

        DumpEntryTransitions(
            output,
            stateMachine,
            indent + "  "
        );

        DumpAnyStateTransitions(
            output,
            stateMachine,
            indent + "  "
        );

        output.AppendLine($"{indent}States:");

        if(stateMachine.states.Length == 0)
            output.AppendLine($"{indent}  <No states>");

        foreach(ChildAnimatorState childState in stateMachine.states) {
            bool isDefault =
                childState.state == stateMachine.defaultState;

            DumpState(
                output,
                childState.state,
                indent + "  ",
                isDefault
            );
        }

        if(stateMachine.stateMachines.Length > 0) {
            output.AppendLine($"{indent}Sub-State Machines:");

            foreach(
                ChildAnimatorStateMachine childMachine
                in stateMachine.stateMachines
            ) {
                AnimatorTransition[] machineTransitions =
                    stateMachine.GetStateMachineTransitions(
                        childMachine.stateMachine
                    );

                if(machineTransitions.Length > 0) {
                    output.AppendLine(
                        $"{indent}  Transitions from " +
                        $"Sub-State Machine '{childMachine.stateMachine.name}':"
                    );

                    foreach(AnimatorTransition transition in machineTransitions) {
                        DumpBasicTransition(
                            output,
                            transition,
                            childMachine.stateMachine.name,
                            indent + "    "
                        );
                    }
                }

                output.AppendLine();

                DumpStateMachine(
                    output,
                    childMachine.stateMachine,
                    indent + "  ",
                    stateMachine
                );
            }
        }
    }

    private static void DumpState(
        StringBuilder output,
        AnimatorState state,
        string indent,
        bool isDefault
    ) {
        string defaultMarker = isDefault
            ? " [DEFAULT]"
            : "";

        output.AppendLine();
        output.AppendLine(
            $"{indent}STATE: {state.name}{defaultMarker}"
        );

        output.AppendLine(
            $"{indent}  Motion: {GetMotionName(state.motion)}"
        );

        output.AppendLine(
            $"{indent}  Speed: {state.speed:0.###}"
        );

        output.AppendLine(
            $"{indent}  Mirror: {state.mirror}"
        );

        output.AppendLine(
            $"{indent}  Cycle Offset: {state.cycleOffset:0.###}"
        );

        output.AppendLine(
            $"{indent}  Foot IK: {state.iKOnFeet}"
        );

        output.AppendLine(
            $"{indent}  Write Defaults: {state.writeDefaultValues}"
        );

        if(!string.IsNullOrEmpty(state.tag))
            output.AppendLine($"{indent}  Tag: {state.tag}");

        DumpBehaviours(
            output,
            state.behaviours,
            indent + "  "
        );

        if(state.motion is BlendTree blendTree) {
            DumpBlendTree(
                output,
                blendTree,
                indent + "  "
            );
        }

        output.AppendLine(
            $"{indent}  Transitions:"
        );

        if(state.transitions.Length == 0) {
            output.AppendLine(
                $"{indent}    <No transitions>"
            );

            return;
        }

        foreach(
            AnimatorStateTransition transition
            in state.transitions
        ) {
            DumpStateTransition(
                output,
                transition,
                state.name,
                indent + "    "
            );
        }
    }

    private static void DumpEntryTransitions(
        StringBuilder output,
        AnimatorStateMachine stateMachine,
        string indent
    ) {
        AnimatorTransition[] transitions =
            stateMachine.entryTransitions;

        if(transitions.Length == 0)
            return;

        output.AppendLine($"{indent}Entry Transitions:");

        foreach(AnimatorTransition transition in transitions) {
            DumpBasicTransition(
                output,
                transition,
                "Entry",
                indent + "  "
            );
        }
    }

    private static void DumpAnyStateTransitions(
        StringBuilder output,
        AnimatorStateMachine stateMachine,
        string indent
    ) {
        AnimatorStateTransition[] transitions =
            stateMachine.anyStateTransitions;

        if(transitions.Length == 0)
            return;

        output.AppendLine($"{indent}Any State Transitions:");

        foreach(AnimatorStateTransition transition in transitions) {
            DumpStateTransition(
                output,
                transition,
                "Any State",
                indent + "  "
            );
        }
    }

    private static void DumpStateTransition(
        StringBuilder output,
        AnimatorStateTransition transition,
        string source,
        string indent
    ) {
        output.AppendLine(
            $"{indent}{source} -> {GetDestination(transition)}"
        );

        output.AppendLine(
            $"{indent}  Has Exit Time: {transition.hasExitTime}"
        );

        if(transition.hasExitTime) {
            output.AppendLine(
                $"{indent}  Exit Time: {transition.exitTime:0.###}"
            );
        }

        string durationUnit = transition.hasFixedDuration
            ? "seconds"
            : "normalized";

        output.AppendLine(
            $"{indent}  Duration: " +
            $"{transition.duration:0.###} {durationUnit}"
        );

        output.AppendLine(
            $"{indent}  Offset: {transition.offset:0.###}"
        );

        output.AppendLine(
            $"{indent}  Interruption Source: " +
            $"{transition.interruptionSource}"
        );

        output.AppendLine(
            $"{indent}  Ordered Interruption: " +
            $"{transition.orderedInterruption}"
        );

        output.AppendLine(
            $"{indent}  Can Transition To Self: " +
            $"{transition.canTransitionToSelf}"
        );

        output.AppendLine(
            $"{indent}  Mute: {transition.mute}, " +
            $"Solo: {transition.solo}"
        );

        DumpConditions(
            output,
            transition.conditions,
            indent + "  "
        );
    }

    private static void DumpBasicTransition(
        StringBuilder output,
        AnimatorTransition transition,
        string source,
        string indent
    ) {
        output.AppendLine(
            $"{indent}{source} -> {GetDestination(transition)}"
        );

        output.AppendLine(
            $"{indent}  Mute: {transition.mute}, " +
            $"Solo: {transition.solo}"
        );

        DumpConditions(
            output,
            transition.conditions,
            indent + "  "
        );
    }

    private static void DumpConditions(
        StringBuilder output,
        AnimatorCondition[] conditions,
        string indent
    ) {
        output.AppendLine($"{indent}Conditions:");

        if(conditions.Length == 0) {
            output.AppendLine($"{indent}  <None>");
            return;
        }

        foreach(AnimatorCondition condition in conditions) {
            output.AppendLine(
                $"{indent}  - {FormatCondition(condition)}"
            );
        }
    }

    private static string FormatCondition(
        AnimatorCondition condition
    ) {
        switch(condition.mode) {
            case AnimatorConditionMode.If:
            return $"{condition.parameter} == true";

            case AnimatorConditionMode.IfNot:
            return $"{condition.parameter} == false";

            case AnimatorConditionMode.Greater:
            return $"{condition.parameter} > {condition.threshold:0.###}";

            case AnimatorConditionMode.Less:
            return $"{condition.parameter} < {condition.threshold:0.###}";

            case AnimatorConditionMode.Equals:
            return $"{condition.parameter} == {condition.threshold:0.###}";

            case AnimatorConditionMode.NotEqual:
            return $"{condition.parameter} != {condition.threshold:0.###}";

            default:
            return
                $"{condition.parameter} " +
                $"{condition.mode} " +
                $"{condition.threshold:0.###}";
        }
    }

    private static void DumpBlendTree(
        StringBuilder output,
        BlendTree blendTree,
        string indent
    ) {
        output.AppendLine(
            $"{indent}Blend Tree: {blendTree.name}"
        );

        output.AppendLine(
            $"{indent}  Type: {blendTree.blendType}"
        );

        output.AppendLine(
            $"{indent}  Parameter X: {blendTree.blendParameter}"
        );

        if(blendTree.blendType != BlendTreeType.Simple1D &&
           blendTree.blendType != BlendTreeType.Direct) {
            output.AppendLine(
                $"{indent}  Parameter Y: {blendTree.blendParameterY}"
            );
        }

        output.AppendLine(
            $"{indent}  Children:"
        );

        ChildMotion[] children = blendTree.children;

        if(children.Length == 0) {
            output.AppendLine(
                $"{indent}    <No children>"
            );

            return;
        }

        for(int i = 0; i < children.Length; i++) {
            ChildMotion child = children[i];

            output.AppendLine(
                $"{indent}    [{i}] {GetMotionName(child.motion)}"
            );

            output.AppendLine(
                $"{indent}      Threshold: {child.threshold:0.###}"
            );

            output.AppendLine(
                $"{indent}      Position: " +
                $"({child.position.x:0.###}, {child.position.y:0.###})"
            );

            output.AppendLine(
                $"{indent}      Time Scale: {child.timeScale:0.###}"
            );

            output.AppendLine(
                $"{indent}      Mirror: {child.mirror}"
            );

            if(!string.IsNullOrEmpty(child.directBlendParameter)) {
                output.AppendLine(
                    $"{indent}      Direct Parameter: " +
                    $"{child.directBlendParameter}"
                );
            }

            if(child.motion is BlendTree childBlendTree) {
                DumpBlendTree(
                    output,
                    childBlendTree,
                    indent + "      "
                );
            }
        }
    }

    private static void DumpBehaviours(
        StringBuilder output,
        StateMachineBehaviour[] behaviours,
        string indent
    ) {
        if(behaviours == null || behaviours.Length == 0)
            return;

        output.AppendLine($"{indent}Behaviours:");

        foreach(StateMachineBehaviour behaviour in behaviours) {
            string behaviourName = behaviour != null
                ? behaviour.GetType().Name
                : "<Missing Behaviour>";

            output.AppendLine(
                $"{indent}  - {behaviourName}"
            );
        }
    }

    private static string GetDestination(
        AnimatorTransitionBase transition
    ) {
        if(transition.isExit)
            return "Exit";

        if(transition.destinationState != null)
            return transition.destinationState.name;

        if(transition.destinationStateMachine != null)
            return $"State Machine: {transition.destinationStateMachine.name}";

        return "<No Destination>";
    }

    private static string GetStateName(
        AnimatorState state
    ) {
        return state != null
            ? state.name
            : "<None>";
    }

    private static string GetMotionName(
        Motion motion
    ) {
        if(motion == null)
            return "<None>";

        return $"{motion.name} [{motion.GetType().Name}]";
    }
}

#endif
