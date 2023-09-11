using System;
using System.Collections.Generic;
using UnityEngine;
using pbc = global::Google.Protobuf.Collections;

namespace PDSim.Protobuf
{
    [Serializable]
    public class PdSimTypesDeclaration : ISerializationCallbackReceiver
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

        public void Populate(pbc::RepeatedField<TypeDeclaration> typeDeclarations)
        {
            // From typeDeclarations list of type-parent pairs, populate tree from _root
            var node = GetRoot();
            foreach (var typeDeclaration in typeDeclarations)
            {
                var parentType = typeDeclaration.ParentType;
                var currentType = typeDeclaration.TypeName;

                // if (parentType == null)
                // {
                //     // root node
                //     node = new TypeNode(currentType);
                //     _root = node;
                // }
                if (parentType == null || parentType == string.Empty)
                {
                    // if parent is null or empty, add as child of 'object'
                    var root = GetRoot();
                    root.children.Add(new TypeNode(currentType));
                }
                else
                {
                    // find parent node
                    var queue = new Queue<TypeNode>();
                    queue.Enqueue(node);
                    while (queue.Count > 0)
                    {
                        var n = queue.Dequeue();
                        if (n.Name == parentType)
                        {
                            // found parent node
                            var childNode = new TypeNode(currentType);
                            n.children.Add(childNode);
                            node = childNode;
                            break;
                        }
                        else
                        {
                            foreach (var c in n.children)
                            {
                                queue.Enqueue(c);
                            }
                        }
                    }
                }
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
