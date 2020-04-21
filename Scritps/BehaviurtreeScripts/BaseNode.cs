using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeState {
    Sucesess,
    Failure,
    Running
}

public abstract class BaseNode  {
    
    public delegate NodeState NodeReturn();

    protected NodeState nodeState;

    public NodeState NodeState {
        get { return nodeState; }
    }

    public BaseNode() { }

    public abstract NodeState Evaluate();

}
