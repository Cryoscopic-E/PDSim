using GeTPlan.Core.Models;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models.Expressions;
using PDSimAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Components
{
    public class TypeHierarchy : MonoBehaviour
    {
        // Singleton Instance
        private static TypeHierarchy _instance;
        public static TypeHierarchy Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<TypeHierarchy>();
                return _instance;
            }
        }

        [SerializeField]
        public ModelTypes modelTypes;

        public void Populate(List<PlanType> typeDeclarations)
        {
            modelTypes = new ModelTypes();
            modelTypes.Populate(typeDeclarations);
        }
        public List<string> GetChildrenTypes(string typeName)
        {
            return modelTypes.GetChildrenTypes(typeName);
        }

        public List<string> GetLeafNodesFromType(string type)
        {
            return modelTypes.GetLeafNodesFromType(type);
        }
        public List<string> GetLeafNodes()
        {
            return modelTypes.GetLeafNodes();
        }
        public bool IsChildOf(string checkType, string parentType)
        {
            return modelTypes.IsChildOf(checkType, parentType);
        }
    }



    [Serializable]
    public class ModelTypes : ISerializationCallbackReceiver
    {
        public class TypeNode
        {
            public string Name { get; }
            public List<TypeNode> children;

            public TypeNode(string name)
            {
                Name = name;
                children = new List<TypeNode>();
            }
        }

        [Serializable]
        public struct SerializableTypeNode
        {
            public string name;
            public int childrenCount;
            public int indexFirstChild;
        }

        // Root node for runtime
        private TypeNode _root = new TypeNode("object");

        // List to serialize
        public List<SerializableTypeNode> serializableTypeNodes;

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
                    parentNode.children.Add(allNodes[type.Name]);
                }
                else
                {
                    GetRoot().children.Add(allNodes[type.Name]);
                }
            }
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

                foreach (var n in node.children)
                {
                    queue.Enqueue(n);
                }
            }
            return null;
        }

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
                foreach (var child in node.children)
                {
                    queue.Enqueue(child);
                }
            }
            return subTypes;
        }
        
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
                if (node.children.Count == 0)
                    leaves.Add(node.Name);
                else
                {
                    foreach (var child in node.children)
                        queue.Enqueue(child);
                }
            }
            return leaves;
        }
        
        public List<string> GetLeafNodes()
        {
            return GetLeafNodesFromType("object");
        }

        public bool IsChildOf(string checkType, string parentType)
        {
            var parentNode = FindNode(parentType);
            if (parentNode == null) return false;

            var queue = new Queue<TypeNode>();
            foreach (var child in parentNode.children)
                queue.Enqueue(child);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Name == checkType) return true;
                foreach (var child in node.children)
                    queue.Enqueue(child);
            }
            return false;
        }

        public void OnBeforeSerialize()
        {
            serializableTypeNodes ??= new List<SerializableTypeNode>();
            _root ??= new TypeNode("object");

            serializableTypeNodes.Clear();
            AddNodeSerialize(_root);
        }

        private void AddNodeSerialize(TypeNode node)
        {
            var serializedNode = new SerializableTypeNode()
            {
                name = node.Name,
                childrenCount = node.children.Count,
                indexFirstChild = serializableTypeNodes.Count + 1
            };
            serializableTypeNodes.Add(serializedNode);
            foreach (var child in node.children)
                AddNodeSerialize(child);
        }

        private int ReadFromSerializedNodes(int index, out TypeNode node)
        {
            var serializedNode = serializableTypeNodes[index];
            var newNode = new TypeNode(serializedNode.name);

            // read tree
            for (var i = 0; i != serializedNode.childrenCount; i++)
            {
                index = ReadFromSerializedNodes(++index, out var childNode);
                newNode.children.Add(childNode);
            }

            node = newNode;
            return index;
        }

        public void OnAfterDeserialize()
        {
            // Populate runtime data
            if (serializableTypeNodes != null && serializableTypeNodes.Count > 0)
            {
                ReadFromSerializedNodes(0, out _root);
            }
            else
            {
                _root = new TypeNode("object");
            }
        }

        public TypeNode GetRoot()
        {
            return _root ??= new TypeNode("object");
        }

    }
}
