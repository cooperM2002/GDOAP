using UnityEngine;
using System.Collections.Generic;

public class AgentAction {
    public string Name { get; }
    public float Cost { get; private set; }

    public HashSet<AgentState> Preconditions { get; } = new();
    public HashSet<AgentState> Effects { get; } = new();


    IActionStrategy strategy;
    
    public bool Complete => strategy.Complete;

    AgentAction(string name) {
        Name = name;
    }

    public void Start() => strategy.Start();

    public void Update(float deltaTime) {
        // check if action can be performed and update the strategy
        if (strategy.CanPerform) 
            strategy.Update(deltaTime);
        

        // bail out of the strategy is still executing
        if (!strategy.Complete) 
            return;

        // apply effects
        foreach (AgentState effect in Effects) 
            effect.Evaluate();
    }

    public void Stop() => strategy.Stop();




    public class Builder {

        readonly AgentAction action;

        public Builder(string name) {
            action = new AgentAction(name) {
                Cost = 1
            };
        }

        public Builder WithCost(float cost) {
            action.Cost = cost;
            return this;
        }

        public Builder WithStrategy(IActionStrategy strategy) {
            action.strategy = strategy;
            return this;
        }

        public Builder AddPrecondition(AgentState precondition) {
            action.Preconditions.Add(precondition);
            return this;
        }

        public Builder AddEffect(AgentState effect) {
            action.Effects.Add(effect);
            return this;
        }

        public AgentAction Build() {
            return action;
        }

    }




}
