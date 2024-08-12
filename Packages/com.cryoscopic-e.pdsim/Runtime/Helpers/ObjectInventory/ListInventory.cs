using UnityEngine;
using System.Collections.Generic;
using PDSim.Simulation;

namespace PDSim.Helpers.ObjectInventory
{
    public class ListInventory : Inventory
    {
        private List<PdSimSimulationObject> list = new List<PdSimSimulationObject>();

        public override void AddItem(PdSimSimulationObject item)
        {
            list.Add(item);
        }

        public override PdSimSimulationObject RemoveItem()
        {
            if (list.Count == 0)
            {
                Debug.LogWarning("List is empty!");
                return null;
            }
            PdSimSimulationObject item = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return item;
        }

        public override PdSimSimulationObject PeekItem()
        {
            if (list.Count == 0)
            {
                Debug.LogWarning("List is empty!");
                return null;
            }
            return list[list.Count - 1];
        }

        public override void Clear()
        {
            list.Clear();
        }

        public override int Count()
        {
            return list.Count;
        }

        public override PdSimSimulationObject GetObject(int index)
        {
            if (index < 0 || index >= list.Count)
            {
                Debug.LogWarning("Index out of range!");
                return null;
            }
            return list[index];
        }
    }
}