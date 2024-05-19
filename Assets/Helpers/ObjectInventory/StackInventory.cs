using System.Collections.Generic;
using UnityEngine;
using PDSim.Simulation.SimulationObject;

namespace Helpers.ObjectInventory
{
    public class StackInventory : Inventory
    {
        private Stack<PdSimSimulationObject> stack = new Stack<PdSimSimulationObject>();

        public override void AddItem(PdSimSimulationObject item)
        {
            stack.Push(item);
        }

        public override PdSimSimulationObject RemoveItem()
        {
            if (stack.Count == 0)
            {
                Debug.LogWarning("Stack is empty!");
                return null;
            }
            return stack.Pop();
        }

        public override PdSimSimulationObject PeekItem()
        {
            if (stack.Count == 0)
            {
                Debug.LogWarning("Stack is empty!");
                return null;
            }
            return stack.Peek();
        }

        public override void Clear()
        {
            stack.Clear();
        }

        public override int Count()
        {
            return stack.Count;
        }

        public override PdSimSimulationObject GetObject(int index)
        {
            Debug.LogWarning("Stack does not support indexing!");
            return null;
        }
    }
}