using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PDSim.Components
{
    [Serializable, Inspectable]
    public class PdTypeTree : ISerializationCallbackReceiver
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

        public void Populate(JObject types, TypeNode node = null)
        {
            node ??= GetRoot();

            if (types == null)
                return;

            // if node name is a leaf node
            if (!types.ContainsKey(node.Name))
                return;

            // create note list
            var children = types[node.Name];
            var childrenNodes = new List<TypeNode>();
            foreach (var c in children)
            {
                childrenNodes.Add(new TypeNode(c.ToString()));
            }

            node.children = childrenNodes;
            foreach (var n in childrenNodes)
            {
                Populate(types, n);
            }
        }

        public List<string> GetChildrenTypes(string typeName)
        {
            var root = GetRoot();
            var queue = new Queue<TypeNode>();
            queue.Enqueue(root);
            var subTypes = new List<string>() { typeName };
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (var n in node.children)
                {
                    queue.Enqueue(n);
                }

                if (node.Name != typeName) continue;

                var childrenQueue = new Queue<TypeNode>();
                childrenQueue.Enqueue(node);
                while (childrenQueue.Count > 0)
                {
                    var child = childrenQueue.Dequeue();
                    foreach (var n in child.children)
                    {
                        subTypes.Add(n.Name);
                        childrenQueue.Enqueue(n);
                    }
                }
                return subTypes;

            }
            return subTypes;
        }
        public List<string> GetLeafNodesFromType(string type)
        {
            var root = GetRoot();
            var queue = new Queue<TypeNode>();
            queue.Enqueue(root);
            var directLeafs = new List<string>();
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (var n in node.children)
                {
                    queue.Enqueue(n);
                }

                if (node.Name != type) continue;


                // is type
                // loop through all children nodes
                var childrenQueue = new Queue<TypeNode>();
                childrenQueue.Enqueue(node);
                while (childrenQueue.Count > 0)
                {
                    var child = childrenQueue.Dequeue();
                    if (child.children.Count == 0)
                        directLeafs.Add(child.Name);
                    else
                    {
                        foreach (var n in child.children)
                        {
                            childrenQueue.Enqueue(n);
                        }
                    }
                }
                return directLeafs;
            }
            return directLeafs;
        }
        public List<string> GetLeafNodes()
        {
            var root = GetRoot();
            var queue = new Queue<TypeNode>();
            queue.Enqueue(root);
            var leaves = new List<string>();
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.children.Count == 0)
                    leaves.Add(node.Name);
                else
                {
                    foreach (var n in node.children)
                    {
                        queue.Enqueue(n);
                    }
                }
            }
            return leaves;
        }

        public bool IsChildOf(string checkType, string parentType)
        {
            var root = GetRoot();
            var queue = new Queue<TypeNode>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (var n in node.children)
                {
                    queue.Enqueue(n);
                }

                if (node.Name != parentType) continue;

                var childrenQueue = new Queue<TypeNode>();
                childrenQueue.Enqueue(node);
                while (childrenQueue.Count > 0)
                {
                    var child = childrenQueue.Dequeue();
                    foreach (var n in child.children)
                    {
                        if (n.Name.Equals(checkType))
                            return true;
                        childrenQueue.Enqueue(n);
                    }
                }
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
            if (serializableTypeNodes.Count > 0)
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