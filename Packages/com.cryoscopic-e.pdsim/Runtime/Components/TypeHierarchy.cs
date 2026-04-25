using GeTPlan.Core.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Components
{
    /// <summary>
    /// Manages and provides access to the planning domain's type hierarchy.
    /// Used for type checking and finding sub-types.
    /// </summary>
    public class TypeHierarchy : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Singleton instance of the TypeHierarchy.
        /// </summary>
        public static TypeHierarchy Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<TypeHierarchy>();
                return _instance;
            }
        }

        /// <summary>
        /// The serialized model types representing the hierarchy.
        /// </summary>
        [SerializeField]
        [Tooltip("The serialized model types representing the hierarchy.")]
        public ModelTypes ModelTypes;

        /// <summary>
        /// Populates the hierarchy with type declarations from the domain.
        /// </summary>
        /// <param name="typeDeclarations">The list of types to include.</param>
        public void Populate(List<PlanType> typeDeclarations)
        {
            ModelTypes = new ModelTypes();
            ModelTypes.Populate(typeDeclarations);
        }

        /// <summary>
        /// Retrieves all children types of a given type.
        /// </summary>
        /// <param name="typeName">The parent type name.</param>
        /// <returns>A list of children type names.</returns>
        public List<string> GetChildrenTypes(string typeName)
        {
            return ModelTypes.GetChildrenTypes(typeName);
        }

        /// <summary>
        /// Retrieves all leaf types (types with no children) under a given type.
        /// </summary>
        /// <param name="type">The parent type name.</param>
        /// <returns>A list of leaf type names.</returns>
        public List<string> GetLeafNodesFromType(string type)
        {
            return ModelTypes.GetLeafNodesFromType(type);
        }

        /// <summary>
        /// Retrieves all leaf types in the entire hierarchy.
        /// </summary>
        /// <returns>A list of leaf type names.</returns>
        public List<string> GetLeafNodes()
        {
            return ModelTypes.GetLeafNodes();
        }

        /// <summary>
        /// Checks if a type is a child of another type.
        /// </summary>
        /// <param name="checkType">The type to check.</param>
        /// <param name="parentType">The potential parent type.</param>
        /// <returns>True if checkType is a descendant of parentType.</returns>
        public bool IsChildOf(string checkType, string parentType)
        {
            return ModelTypes.IsChildOf(checkType, parentType);
        }

        #endregion

        #region Private Internals

        private static TypeHierarchy _instance;

        #endregion
    }

    /// <summary>
    /// Serialized representation of the type hierarchy.
    /// </summary>
    [Serializable]
    public class ModelTypes : ISerializationCallbackReceiver
    {
        #region Data Classes

        /// <summary>
        /// A node in the type hierarchy tree.
        /// </summary>
        public class TypeNode
        {
            /// <summary>
            /// The name of the type.
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// The list of children nodes.
            /// </summary>
            public List<TypeNode> Children;

            public TypeNode(string name)
            {
                Name = name;
                Children = new List<TypeNode>();
            }
        }

        /// <summary>
        /// A serializable struct representing a type node.
        /// </summary>
        [Serializable]
        public struct SerializableTypeNode
        {
            public string name;
            public int childrenCount;
            public int indexFirstChild;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Populates the hierarchy tree from plan type declarations.
        /// </summary>
        /// <param name="typeDeclarations">The list of types from the domain.</param>
        public void Populate(List<PlanType> typeDeclarations)
        {
            // Firstpass: Add all nodes to a dictionary for easy lookup
            var allNodes = new Dictionary<string, TypeNode>();
            allNodes["object"] = GetRoot();

            foreach (var type in typeDeclarations)
            {
                allNodes[type.Name] = new TypeNode(type.Name);
            }

            // Second pass: Link children to parents
            foreach (var type in typeDeclarations)
            {
                var parentName = type.Parent?.Name ?? "object";
                if (allNodes.TryGetValue(parentName, out var parentNode))
                {
                    parentNode.Children.Add(allNodes[type.Name]);
                }
                else
                {
                    GetRoot().Children.Add(allNodes[type.Name]);
                }
            }
        }

        /// <summary>
        /// Retrieves all children types for a given type name.
        /// </summary>
        public List<string> GetChildrenTypes(string typeName)
        {
            var startNode = FindNode(typeName);
            if (startNode == null) return new List<string>() { typeName };

            var subTypes = new List<string>();
            var queue = new Queue<TypeNode>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                subTypes.Add(node.Name);
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
            return subTypes;
        }

        /// <summary>
        /// Retrieves all leaf types under a given type.
        /// </summary>
        public List<string> GetLeafNodesFromType(string type)
        {
            var startNode = FindNode(type);
            if (startNode == null) return new List<string>();

            var leaves = new List<string>();
            var queue = new Queue<TypeNode>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Children.Count == 0)
                    leaves.Add(node.Name);
                else
                {
                    foreach (var child in node.Children)
                        queue.Enqueue(child);
                }
            }
            return leaves;
        }

        /// <summary>
        /// Retrieves all leaf types in the hierarchy.
        /// </summary>
        public List<string> GetLeafNodes()
        {
            return GetLeafNodesFromType("object");
        }

        /// <summary>
        /// Checks if checkType is a descendant of parentType.
        /// </summary>
        public bool IsChildOf(string checkType, string parentType)
        {
            var parentNode = FindNode(parentType);
            if (parentNode == null) return false;

            var queue = new Queue<TypeNode>();
            foreach (var child in parentNode.Children)
                queue.Enqueue(child);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Name == checkType) return true;
                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }
            return false;
        }

        #endregion

        #region Serialization

        public void OnBeforeSerialize()
        {
            _serializableTypeNodes ??= new List<SerializableTypeNode>();
            _root ??= new TypeNode("object");

            _serializableTypeNodes.Clear();
            AddNodeSerialize(_root);
        }

        public void OnAfterDeserialize()
        {
            // Populate runtime data
            if (_serializableTypeNodes != null && _serializableTypeNodes.Count > 0)
            {
                ReadFromSerializedNodes(0, out _root);
            }
            else
            {
                _root = new TypeNode("object");
            }
        }

        #endregion

        #region Internals

        [SerializeField]
        private List<SerializableTypeNode> _serializableTypeNodes;

        private TypeNode _root = new TypeNode("object");

        public TypeNode GetRoot()
        {
            return _root ??= new TypeNode("object");
        }

        private TypeNode FindNode(string typeName)
        {
            var root = GetRoot();
            var queue = new Queue<TypeNode>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Name == typeName)
                    return node;

                foreach (var n in node.Children)
                {
                    queue.Enqueue(n);
                }
            }
            return null;
        }

        private void AddNodeSerialize(TypeNode node)
        {
            var serializedNode = new SerializableTypeNode()
            {
                name = node.Name,
                childrenCount = node.Children.Count,
                indexFirstChild = _serializableTypeNodes.Count + 1
            };
            _serializableTypeNodes.Add(serializedNode);
            foreach (var child in node.Children)
                AddNodeSerialize(child);
        }

        private int ReadFromSerializedNodes(int index, out TypeNode node)
        {
            var serializedNode = _serializableTypeNodes[index];
            var newNode = new TypeNode(serializedNode.name);

            // read tree
            for (var i = 0; i != serializedNode.childrenCount; i++)
            {
                index = ReadFromSerializedNodes(++index, out var childNode);
                newNode.Children.Add(childNode);
            }

            node = newNode;
            return index;
        }

        #endregion
    }
}
