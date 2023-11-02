using PDSim.Protobuf;
using PDSim.Simulation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PDSim.Simulation
{
    public class State
    {
        private HashSet<StateNode> _state;

        public State()
        {
            //_state = new HashSet<StateNode>(new StateNodeEqualityComparer());
            _state = new HashSet<StateNode>();
        }

        public List<StateNode> GetState()
        {
            return _state.ToList();
        }


        public bool Holds(List<StateNode> nodes, string functor = "AND")
        {
            if (functor == "AND")
            {
                return nodes.All(nodeToCheck => _state.Any(node =>
                                 node.fluentName == nodeToCheck.fluentName &&
                                 node.parameters.SequenceEqual(nodeToCheck.parameters) &&
                                 node.fluentType == nodeToCheck.fluentType &&
                                 node.fluentValue == nodeToCheck.fluentValue));
            }
            else if (functor == "OR")
            {
                return nodes.Any(nodeToCheck => _state.Any(node =>
                                 node.fluentName == nodeToCheck.fluentName &&
                                 node.parameters.SequenceEqual(nodeToCheck.parameters) &&
                                 node.fluentType == nodeToCheck.fluentType &&
                                 node.fluentValue == nodeToCheck.fluentValue));
            }
            else
            {
                return false;
            }
        }



        public bool AddOrUpdate(StateNode node)
        {
            var existingNode = _state.FirstOrDefault(n => n.fluentName == node.fluentName && n.fluentType == node.fluentType && n.parameters.SequenceEqual(node.parameters));

            if (existingNode != null)
            {
                // If the node exists, check if the value is the same
                if (existingNode.fluentValue == node.fluentValue)
                {
                    // Node with the same name, parameters, and value type exists, do nothing.
                    return false;
                }
                else
                {
                    // Node with the same name, parameters, and value type exists, update its value.
                    existingNode.fluentValue = node.fluentValue;
                    return true;
                }
            }
            else
            {
                // Node doesn't exist, add it.
                _state.Add(node);
                return true;
            }
        }

        public bool AddOrUpdate(PdSimFluentAssignment assignment)
        {
            var node = StateNode.FromAssignment(assignment);
            return AddOrUpdate(node);
        }

        public bool Remove(StateNode node)
        {
            return _state.Remove(node);
        }

        public StateNode Query (string fluentName, List<string> parameters)
        {
            return _state.FirstOrDefault(n => n.fluentName == fluentName && n.parameters.SequenceEqual(parameters));
        }
    }


    public class StateNode
    {
        // Statics
        public static readonly string BooleanType = "Boolean";
        public static readonly string NumberType = "Number";
        public static readonly string SymbolType = "Symbol";
        public static readonly List<string> NoParam = new List<string> { };

        public string fluentName;
        public string fluentValue;
        public string fluentType;
        public List<string> parameters;


        public StateNode(string fluentName, string fluentValue, string fluentType, List<string> parameters)
        {
            this.fluentName = fluentName;
            this.fluentValue = fluentValue;
            this.fluentType = fluentType;
            this.parameters = parameters;
        }

        public StateNode(string fluentName, string fluentValue, string fluentType)
        {
            this.fluentName = fluentName;
            this.fluentValue = fluentValue;
            this.fluentType = fluentType;
            parameters = new List<string>();
        }

        public static StateNode FromAssignment(PdSimFluentAssignment assignment)
        {
            return new StateNode(assignment.fluentName, assignment.value.ToString(), assignment.value.contentCase.ToString(), assignment.parameters);
        }

        public override string ToString()
        {
            // Format : (fluentName ?p1 ?p2 ... ?pn) := fluentValue
            var fluentString = "(" + fluentName;
            foreach (var p in parameters)
            {
                fluentString += " " + p;
            }
            fluentString += ") := " + fluentValue;
            return fluentString;
        }
    }
}