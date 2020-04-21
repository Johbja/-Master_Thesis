using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : BaseNode {
    
    private BaseNode childNode;

    public BaseNode ChildNode {
        get { return childNode; }
    }

    public Inverter(BaseNode childNode) {
        this.childNode = childNode;
    }

    public override NodeState Evaluate() {
        switch(childNode.Evaluate()) {
            case NodeState.Failure:
                nodeState = NodeState.Sucesess;
                return nodeState;
            case NodeState.Sucesess:
                nodeState = NodeState.Failure;
                return nodeState;
            case NodeState.Running:
                nodeState = NodeState.Running;
                return nodeState;
        }
        nodeState = NodeState.Sucesess;
        return nodeState;
    }
}
