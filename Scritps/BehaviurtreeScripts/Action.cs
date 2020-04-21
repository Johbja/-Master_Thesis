using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionNode : BaseNode {

    public delegate NodeState ActionNodeDelegate();

    private ActionNodeDelegate action;

    public ActionNode(ActionNodeDelegate action) {
        this.action = action;
    }

    public override NodeState Evaluate() {
        switch(action()) {
            case NodeState.Sucesess:
                nodeState = NodeState.Sucesess;
                return nodeState;
            case NodeState.Failure:
                nodeState = NodeState.Failure;
                return nodeState;
            case NodeState.Running:
                nodeState = NodeState.Running;
                return nodeState;
            default:
                nodeState = NodeState.Failure;
                return nodeState;
        }
    }
}
