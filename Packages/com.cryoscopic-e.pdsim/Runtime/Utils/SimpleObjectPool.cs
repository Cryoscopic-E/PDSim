using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Utils
{
    public class SimpleObjectPool : MonoBehaviour
    {
        private static SimpleObjectPool _instance;
        public static SimpleObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PDSim_ObjectPool");
                    _instance = go.AddComponent<SimpleObjectPool>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();

        public GameObject Get(GameObject prefab)
        {
            // Use prefab name (or instance ID) as key
            // Note: prefab.name might need to be unique enough
            string key = prefab.GetInstanceID().ToString();

            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
            }

            if (_pools[key].Count > 0)
            {
                var obj = _pools[key].Dequeue();
                if (obj != null) // Check in case it was destroyed externally
                {
                    obj.SetActive(true);
                    return obj;
                }
            }

            // Create new
            var newObj = Instantiate(prefab);
            // We need a way to link this instance back to its key when returning
            // For simplicity, we assume the caller knows the original prefab or we track it.
            // But getting the key back from the instance is tricky without a component.
            // Let's add a tracker component.
            var tracker = newObj.AddComponent<PoolTracker>();
            tracker.poolKey = key;
            return newObj;
        }

        public void Return(GameObject obj)
        {
            var tracker = obj.GetComponent<PoolTracker>();
            if (tracker != null)
            {
                obj.SetActive(false);
                obj.transform.SetParent(transform); // Move to pool container
                
                if (!_pools.ContainsKey(tracker.poolKey))
                {
                    _pools[tracker.poolKey] = new Queue<GameObject>();
                }
                _pools[tracker.poolKey].Enqueue(obj);
            }
            else
            {
                Debug.LogWarning("Returning object to pool without PoolTracker. Destroying instead.");
                Destroy(obj);
            }
        }
    }

    public class PoolTracker : MonoBehaviour
    {
        public string poolKey;
    }
}
