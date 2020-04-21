using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : BaseNode {

    protected List<BaseNode> childNodes = new List<BaseNode>();

    public Selector(List<BaseNode> childNodes) {
        this.childNodes = childNodes;
    }

    public override NodeState Evaluate() {
        foreach(BaseNode child in childNodes) {
            switch(child.Evaluate()) {
                case NodeState.Failure:
                    continue;
                case NodeState.Sucesess:
                    nodeState = NodeState.Sucesess;
                    return nodeState;
                case NodeState.Running:
                    nodeState = NodeState.Running;
                    return nodeState;
                default:
                    continue;
            }
        }

        nodeState = NodeState.Failure;
        return nodeState;
    }

}
