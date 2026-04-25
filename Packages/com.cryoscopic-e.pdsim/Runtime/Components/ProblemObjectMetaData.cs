using System.Collections.Generic;
using UnityEngine;
using GeTPlan.Core.Models;

namespace PDSim.Components
{
    /// <summary>
    /// Functional metadata for PDSim objects.
    /// Provides a way to identify specific points or parts of a model for planning and visualization.
    /// </summary>
    public class ProblemObjectMetaData : MonoBehaviour
    {
        #region Data Classes

        /// <summary>
        /// A generic metadata entry mapping a name to a reference.
        /// </summary>
        /// <typeparam name="T">The type of the reference.</typeparam>
        [System.Serializable]
        public class Entry<T>
        {
            /// <summary>
            /// The unique name of the metadata entry.
            /// </summary>
            public string Name;
            /// <summary>
            /// The reference associated with the name.
            /// </summary>
            public T Reference;
        }

        #endregion

        #region Public API

        /// <summary>
        /// The type name used in the planning domain (e.g., 'robot', 'location').
        /// </summary>
        public string PlanTypeName => _planTypeName;

        /// <summary>
        /// List of anchor transforms for the object (e.g., grab points).
        /// </summary>
        [Header("Functional Categories")]
        [Tooltip("Anchor transforms for the object.")]
        public List<Entry<Transform>> Anchors = new List<Entry<Transform>>();

        /// <summary>
        /// List of renderers associated with this object.
        /// </summary>
        [Tooltip("Renderers associated with this object.")]
        public List<Entry<Renderer>> Renders = new List<Entry<Renderer>>();

        /// <summary>
        /// List of UI components associated with this object.
        /// </summary>
        [Tooltip("UI components associated with this object.")]
        public List<Entry<Component>> UI = new List<Entry<Component>>();

        /// <summary>
        /// List of custom attributes for the object.
        /// </summary>
        [Tooltip("Custom string attributes for the object.")]
        public List<Entry<string>> Attributes = new List<Entry<string>>();

        /// <summary>
        /// Converts the metadata into a PlanObject for use in the planning system.
        /// </summary>
        /// <returns>A new PlanObject instance.</returns>
        public PlanObject ToPlanObject()
        {
            return new PlanObject(gameObject.name, new PlanType(_planTypeName));
        }

        /// <summary>
        /// Retrieves an anchor transform by its name.
        /// </summary>
        /// <param name="key">The name of the anchor.</param>
        /// <returns>The transform, or null if not found.</returns>
        public Transform GetAnchor(string key)
        {
            if (_anchorCache == null) InitializeCaches();
            return _anchorCache.TryGetValue(key, out var t) ? t : null;
        }

        /// <summary>
        /// Retrieves a renderer by its name.
        /// </summary>
        /// <param name="key">The name of the renderer.</param>
        /// <returns>The renderer, or null if not found.</returns>
        public Renderer GetRender(string key)
        {
            if (_renderCache == null) InitializeCaches();
            return _renderCache.TryGetValue(key, out var r) ? r : null;
        }

        /// <summary>
        /// Retrieves a UI component by its name.
        /// </summary>
        /// <param name="key">The name of the UI component.</param>
        /// <returns>The component, or null if not found.</returns>
        public Component GetUI(string key)
        {
            if (_uiCache == null) InitializeCaches();
            return _uiCache.TryGetValue(key, out var c) ? c : null;
        }

        /// <summary>
        /// Retrieves a custom attribute by its name.
        /// </summary>
        /// <param name="key">The name of the attribute.</param>
        /// <returns>The attribute string, or null if not found.</returns>
        public string GetAttribute(string key)
        {
            if (_attributeCache == null) InitializeCaches();
            return _attributeCache.TryGetValue(key, out var s) ? s : null;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeCaches();
        }

        #endregion

        #region Private Internals

        [Header("Planning")]
        [SerializeField, Tooltip("The type name used in the planning domain (e.g., 'robot', 'location').")]
        private string _planTypeName = "object";

        // Runtime Caches
        private Dictionary<string, Transform> _anchorCache;
        private Dictionary<string, Renderer> _renderCache;
        private Dictionary<string, Component> _uiCache;
        private Dictionary<string, string> _attributeCache;

        private void InitializeCaches()
        {
            _anchorCache = new Dictionary<string, Transform>();
            foreach (var entry in Anchors) if (entry.Reference != null) _anchorCache[entry.Name] = entry.Reference;

            _renderCache = new Dictionary<string, Renderer>();
            foreach (var entry in Renders) if (entry.Reference != null) _renderCache[entry.Name] = entry.Reference;

            _uiCache = new Dictionary<string, Component>();
            foreach (var entry in UI) if (entry.Reference != null) _uiCache[entry.Name] = entry.Reference;

            _attributeCache = new Dictionary<string, string>();
            foreach (var entry in Attributes) _attributeCache[entry.Name] = entry.Reference;
        }

        #endregion
    }
}
