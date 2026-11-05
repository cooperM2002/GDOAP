using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// <tt><b>StateFactory</b></tt> provides methods for creating <tt><b>AgentState</b></tt> objects and adding them to the State Dictionary
/// </summary>
public class StateFactory {

    readonly GoapAgent agent;
    readonly Dictionary<string, AgentState> states;


    public StateFactory(GoapAgent agent, Dictionary<string, AgentState> states) {
        this.agent = agent;
        this.states = states;
    }


    public void AddState(string key, Func<bool> condition) {
        states.Add(key, new AgentState.Builder(key)
            .WithCondition(condition)
            .Build());
    }


    public void AddSensorState(string key, Sensor sensor) {
        states.Add(key, new AgentState.Builder(key)
            .WithCondition(() => sensor.IsTargetInRange)
            .WithLocation(() => sensor.TargetPosition)
            .Build());
    }


    //overload, convert transform to vector3
    public void AddLocationState(string key, float distance, Transform locationCondition) { AddLocationState(key, distance, locationCondition.position); }
    public void AddLocationState(string key, float distance, Vector3 locationCondition) {
        states.Add(key, new AgentState.Builder(key)
            .WithCondition(() => InRangeOf(locationCondition, distance))
            .WithLocation(() => locationCondition)
            .Build());
    }


    ///caluclate if within range of something
    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(agent.transform.position, pos) < range;

}


/// <summary>
/// <tt><b>AgentState</b></tt> represents one name condition about the NPC or its surroundings
/// <list type="bullet">
///     <item>
///     string <tt><b>Name</b></tt>: name of the condition
///     </item>
///     <item>
///     Func : bool <tt><b>condition</b></tt>: reference to a function that checks if condition is <i><b>currently</b></i> true
///     </item>
///     <item>
///     Func : Vector3 <tt><b>observedLocation</b></tt>: reference to a function that provides a relevant position
///     </item>
/// </list>
/// </summary>
public class AgentState {

    public string Name { get; }
    Func<bool> condition = () => false;
    Func<Vector3> observedLocation = () => Vector3.zero;
    public Vector3 Location => observedLocation();


    
    //builder

    AgentState(string name) {
        Name = name;
    }

    public bool Evaluate() => condition();


    /// <summary>
    /// Creating a <tt><b>Builder</b></tt> creates and configure one <tt><b>AgentState</b></tt>
    /// <list type="bullet">
    ///     <item>
    ///     <tt><b> WithCondition(Func:bool condition)</b></tt> assigns the function used by <tt><b>Evaluate()</b></tt>
    ///     </item>
    ///     <item>
    ///     <tt><b> WithLocation(Func:Vector3 location)</b></tt> assigns the function used by <tt><b>Location()</b></tt>
    ///     </item>
    /// </list>
    /// </summary>
    public class Builder {
    
        readonly AgentState state;
        public Builder(string name) {
            state = new AgentState(name);
        }

        public Builder WithCondition(Func<bool> condition) {
            state.condition = condition;
            return this;

        }

        public Builder WithLocation(Func<Vector3> observedLocation) {
            state.observedLocation = observedLocation;
            return this;
        }

        public AgentState Build() {
            return state;
        }

    }


    
}
