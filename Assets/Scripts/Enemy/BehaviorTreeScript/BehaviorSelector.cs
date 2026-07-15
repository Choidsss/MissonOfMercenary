using NUnit.Framework;
using System.Data;
using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary
{
    public abstract class BTNode
    {
        public enum State { Success, Failure, Running}
        public abstract State Evaluate();
    }

    public class BehaviorSelector : BTNode
    {
        protected List<BTNode> _children = new List<BTNode>();
        public void AddChild(BTNode node) => _children.Add(node);

        public override State Evaluate()
        {
            foreach (var child in _children)
            {
                var result = child.Evaluate();
                if (result != State.Failure)
                {
                    return result;
                }
            }
            return State.Failure;
        }
    }

    public class Sequence : BTNode
    {
        List<BTNode> _children = new List<BTNode>();
        public void AddChild(BTNode node) => _children.Add(node);

        public override State Evaluate()
        {
            foreach (var child in _children)
            {
                var result = child.Evaluate();
                if (result != State.Success)
                {
                    return result;
                }
            }
            return State.Success;
        }
    }
}
