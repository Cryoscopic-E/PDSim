using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Utils
{
    /// <summary>
    /// A simple object pooling system to reuse GameObjects and reduce instantiation overhead.
    /// </summary>
    public class SimpleObjectPool : MonoBehaviour
    {
        private static SimpleObjectPool _instance;

        /// <summary>
        /// Singleton instance of the SimpleObjectPool.
        /// </summary>
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

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();

        #region Public API

        /// <summary>
        /// Retrieves an object from the pool or instantiates a new one if the pool is empty.
        /// </summary>
        /// <param name="prefab">The prefab to retrieve from the pool.</param>
        /// <returns>A pooled or new instance of the prefab.</returns>
        public GameObject Get(GameObject prefab)
        {
            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = new Queue<GameObject>();
            }

            if (_pools[prefab].Count > 0)
            {
                var obj = _pools[prefab].Dequeue();
                if (obj != null)
                {
                    obj.SetActive(true);
                    return obj;
                }
            }

            var newObj = Instantiate(prefab);
            var tracker = newObj.AddComponent<PoolTracker>();
            tracker.PoolKey = prefab;
            return newObj;
        }

        /// <summary>
        /// Returns an object to the pool for later reuse.
        /// </summary>
        /// <param name="obj">The GameObject to return to the pool.</param>
        public void Return(GameObject obj)
        {
            var tracker = obj.GetComponent<PoolTracker>();
            if (tracker != null)
            {
                obj.SetActive(false);
                obj.transform.SetParent(transform);

                if (!_pools.ContainsKey(tracker.PoolKey))
                {
                    _pools[tracker.PoolKey] = new Queue<GameObject>();
                }
                _pools[tracker.PoolKey].Enqueue(obj);
            }
            else
            {
                Debug.LogWarning("[PDSim] Returning object to pool without PoolTracker. Destroying instead.");
                Destroy(obj);
            }
        }

        #endregion
    }

    /// <summary>
    /// Component attached to pooled objects to track which pool they belong to.
    /// </summary>
    public class PoolTracker : MonoBehaviour
    {
        /// <summary>
        /// The unique key for the pool this object belongs to.
        /// </summary>
        public GameObject PoolKey;
    }
}
