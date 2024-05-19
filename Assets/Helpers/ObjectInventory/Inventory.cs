using UnityEngine;
using PDSim.Simulation.SimulationObject;
using System.Collections;
using System.Collections.Generic;

namespace Helpers.ObjectInventory
{
     public enum InventoryType
    {
        Stack,
        List,
    }

    public abstract class Inventory : MonoBehaviour
    {
        public abstract void AddItem(PdSimSimulationObject item);
        public abstract PdSimSimulationObject RemoveItem();
        public abstract PdSimSimulationObject PeekItem();
        public abstract void Clear();
        public abstract int Count();
        public abstract PdSimSimulationObject GetObject(int index);
    }
}