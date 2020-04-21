using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : BaseNode {

    private List<BaseNode> childNodes = new List<BaseNode>();

    public Sequence(List<BaseNode> childNodes) {
        this.childNodes = childNodes;
    }

    public override NodeState Evaluate() {
        bool anyChildRunning = false;

        foreach(BaseNode child in childNodes) {
            switch(child.Evaluate()) {
                case NodeState.Failure:
                    nodeState = NodeState.Failure;
                    return nodeState;
                case NodeState.Sucesess:
                    continue;
                case NodeState.Running:
                    anyChildRunning = true;
                    continue;
                default:
                    nodeState = NodeState.Sucesess;
                    return nodeState;
            }
        }

        nodeState = anyChildRunning ? NodeState.Running : NodeState.Sucesess;
        return nodeState;
    }
}

