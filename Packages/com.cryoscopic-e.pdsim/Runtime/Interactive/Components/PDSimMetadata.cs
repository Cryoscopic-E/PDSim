using System.Collections.Generic;
using UnityEngine;
using GeTPlan.Core.Models;

namespace PDSim.Interactive
{
    /// <summary>
    /// Functional metadata for PDSim objects.
    /// Replaces Visual Scripting for identifying specific points or parts of a model.
    /// </summary>
    public class PDSimMetadata : MonoBehaviour
    {
        [System.Serializable]
        public class Entry<T>
        {
            public string name;
            public T reference;
        }

        [Header("Planning")]
        [SerializeField, Tooltip("The type name used in the planning domain (e.g., 'robot', 'location').")]
        private string planTypeName = "object";
        public string PlanTypeName => planTypeName;

        [Header("Functional Categories")]
        public List<Entry<Transform>> anchors = new List<Entry<Transform>>();
        public List<Entry<Renderer>> renders = new List<Entry<Renderer>>();
        public List<Entry<Component>> ui = new List<Entry<Component>>();
        public List<Entry<string>> attributes = new List<Entry<string>>();

        // Runtime Caches
        private Dictionary<string, Transform> _anchorCache;
        private Dictionary<string, Renderer> _renderCache;
        private Dictionary<string, Component> _uiCache;
        private Dictionary<string, string> _attributeCache;

        private void Awake()
        {
            InitializeCaches();
        }

        private void OnEnable()
        {
            if (PDSimWorldObserver.Instance != null)
                PDSimWorldObserver.Instance.RegisterObject(this);
        }

        private void OnDisable()
        {
            if (PDSimWorldObserver.Instance != null)
                PDSimWorldObserver.Instance.UnregisterObject(this);
        }

        public PlanObject ToPlanObject()
        {
            return new PlanObject(gameObject.name, new PlanType(planTypeName));
        }

        private void InitializeCaches()
        {
            _anchorCache = new Dictionary<string, Transform>();
            foreach (var entry in anchors) if (entry.reference != null) _anchorCache[entry.name] = entry.reference;

            _renderCache = new Dictionary<string, Renderer>();
            foreach (var entry in renders) if (entry.reference != null) _renderCache[entry.name] = entry.reference;

            _uiCache = new Dictionary<string, Component>();
            foreach (var entry in ui) if (entry.reference != null) _uiCache[entry.name] = entry.reference;

            _attributeCache = new Dictionary<string, string>();
            foreach (var entry in attributes) _attributeCache[entry.name] = entry.reference;
        }

        public Transform GetAnchor(string key)
        {
            if (_anchorCache == null) InitializeCaches();
            return _anchorCache.TryGetValue(key, out var t) ? t : null;
        }

        public Renderer GetRender(string key)
        {
            if (_renderCache == null) InitializeCaches();
            return _renderCache.TryGetValue(key, out var r) ? r : null;
        }

        public Component GetUI(string key)
        {
            if (_uiCache == null) InitializeCaches();
            return _uiCache.TryGetValue(key, out var c) ? c : null;
        }

        public string GetAttribute(string key)
        {
            if (_attributeCache == null) InitializeCaches();
            return _attributeCache.TryGetValue(key, out var s) ? s : null;
        }
    }
}
