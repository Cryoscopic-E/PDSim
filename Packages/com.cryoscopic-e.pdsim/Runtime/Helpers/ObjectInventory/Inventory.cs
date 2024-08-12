using UnityEngine;
using PDSim.Simulation;

namespace PDSim.Helpers.ObjectInventory
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