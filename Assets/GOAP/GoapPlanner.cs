using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IGoapPlanner {
    ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
}

public class GoapPlanner : IGoapPlanner {

    public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null) {
        // Order goals by descending
        List<AgentGoal> orderedGoals = goals
            .Where(goal => goal.DesiredEffects.Any(state => !state.Evaluate()))
            .OrderByDescending(goal => goal == mostRecentGoal ? goal.Priority - 0.01 : goal.Priority)
            .ToList();

        // Try to solve each goal in order, DFS
        foreach (AgentGoal goal in orderedGoals) {
            Node goalNode = new Node(null, null, goal.DesiredEffects, 0);

            // If we can find a path to the goal, return the plan Stack
            if (FindPath(goalNode, agent.actions)) {
                // If we came back and the Goal Node is dead (No Leaves & No Action), everything was already success, nothing to do, pick new goal
                if(goalNode.IsLeafDead) continue;

                //walk from goal node down to last leaf
                Stack<AgentAction> actionStack = new Stack<AgentAction>();
                while (goalNode.Leaves.Count > 0) {
                    Node cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                    goalNode = cheapestLeaf;
                    actionStack.Push(cheapestLeaf.Action);
                }

                return new ActionPlan(goal, actionStack, goalNode.Cost);
            }
        }

        Debug.LogWarning("No plan found");
        return null;

    }

    bool FindPath(Node parent, HashSet<AgentAction> actions) {
        // Order actions by cost, ascending
        IOrderedEnumerable<AgentAction> orderedActions = actions.OrderBy(action => action.Cost);

        foreach (AgentAction action in actions) {
            HashSet<AgentState> requiredEffects = parent.RequiredEffects;


            // Remove any effects that evaluate to true, there is no action to take
            requiredEffects.RemoveWhere(state => state.Evaluate());

            // If there are no required effects to fulfill, we have a plan
            if (requiredEffects.Count == 0) {
                return true;
            }

            // See if this action has any effects that match up with the required effects for the goal
            if(action.Effects.Any(requiredEffects.Contains)) {
                HashSet<AgentState> newRequiredEffects = new HashSet<AgentState>(requiredEffects); // List of requirements

                // New set of requirements
                newRequiredEffects.ExceptWith(action.Effects); // Remove effects that would be satisfied
                newRequiredEffects.UnionWith(action.Preconditions); // Add preconditions of this action

                // !! Depending on if I want my plans to have repeated actions, i.e. use actions MORE THAN ONCE
                // !! LOOK BACK ON THIS and re evaluate if I want this included ... MAYBE remove this ...
                HashSet<AgentAction> newAvailableActions = new HashSet<AgentAction>(actions);
                newAvailableActions.Remove(action);

                // Now we've decided we are going to use this action
                Node newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);

                // Explore new Node recursively
                if (FindPath(newNode, newAvailableActions)) {
                    parent.Leaves.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                }
            
                // If nothing left in the newRequiredEffects, we have found a soln for every possible Precondition in this list, have a Plan
                if (newRequiredEffects.Count == 0) {
                    return true;
                }
                // If we have looped through all the Actions & unable to find any branches where we can satisfy all the preconditions, we can't meet the Goal, try different Goal   
            }
        }

        return false;
    }



}


public class Node {
    public Node Parent { get; }
    public AgentAction Action { get; }
    public HashSet<AgentState> RequiredEffects { get; }
    public List<Node> Leaves { get; }
    public float Cost { get; }

    public bool IsLeafDead => Leaves.Count == 0 && Action == null;

    public Node(Node parent, AgentAction action, HashSet<AgentState> effects, float cost) {
        Parent = parent;
        Action = action;
        RequiredEffects = new HashSet<AgentState>(effects);
        Leaves = new List<Node>();
        Cost = cost;
    }


}


public class ActionPlan {
    
    public AgentGoal AgentGoal { get; }
    public Stack<AgentAction> Actions { get; }
    public float TotalCost { get; set; }

    public ActionPlan(AgentGoal goal, Stack<AgentAction> actions, float totalCost) {
        AgentGoal = goal;
        Actions = actions;
        TotalCost = totalCost;
    }



}
