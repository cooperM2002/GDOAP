using System.Collections.Generic;
using UnityEngine;

public class AgentGoal {
    
    public string Name { get; }
    public float Priority { get; private set; }
    public HashSet<AgentState> DesiredEffects { get; } = new();


    AgentGoal(string name) {
        Name = name;
    }

    public class Builder {

        readonly AgentGoal goal;

        public Builder(string name) {
            goal = new AgentGoal(name);
        }

        public Builder WithPriority(float priority) {
            goal.Priority = priority;
            return this;
        }

        public Builder WithDesiredEffects(AgentState effect) {
            goal.DesiredEffects.Add(effect);
            return this;
        }

        public AgentGoal Build() {
            return goal;
        }

    }



}
