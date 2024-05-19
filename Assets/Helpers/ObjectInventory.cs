using UnityEngine;
using Helpers.ObjectInventory;
using PDSim.Simulation.SimulationObject;
public class ObjectInventory : MonoBehaviour
    {
        public int maxItems = 10;
        public InventoryType inventoryType;
        private Inventory inventory;

        private void Start()
        {
            CreateInventory();
        }

        public void CreateInventory()
        {
            switch (inventoryType)
            {
                case InventoryType.Stack:
                    inventory = gameObject.AddComponent<StackInventory>();
                    break;
                case InventoryType.List:
                    inventory = gameObject.AddComponent<ListInventory>();
                    break;
                default:
                    Debug.LogError("Unknown Inventory Type!");
                    break;
            }
        }

        public void AddItem(PdSimSimulationObject item)
        {
            if (inventory.Count() >= maxItems)
            {
                Debug.LogWarning("Inventory is full!");
                return;
            }
            inventory.AddItem(item);
        }

        public PdSimSimulationObject RemoveItem()
        {
            return inventory.RemoveItem();
        }

        public PdSimSimulationObject PeekItem()
        {
            return inventory.PeekItem();
        }

        public void Clear()
        {
            inventory.Clear();
        }

        public int Count()
        {
            return inventory.Count();
        }

        public PdSimSimulationObject GetObject(int index)
        {
            return inventory.GetObject(index);
        }
    }