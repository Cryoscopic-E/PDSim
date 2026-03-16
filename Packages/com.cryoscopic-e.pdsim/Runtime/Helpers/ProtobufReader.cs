using GeTModel;
using GeTPlanFactories;
using Proto;
using System;
using System.Collections.Generic;
using UnityEditor;
namespace PDSim.Helpers
{
    /// <summary>
    /// Class to read a protobuf problem and plan and generate a PdSimProblem and PdSimInstance
    /// </summary>
    public class ProtobufReader
    {
        public static List<GeTFluent> ReadFluents(Problem problem)
        {
            var fluents = new List<GeTFluent>();
            foreach (var fluent in problem.Fluents)
            {
                var newFluent = FluentFactory.FromProto(fluent);
                fluents.Add(newFluent);
            }
            return fluents;
        }

        public static List<GeTAction> ReadActions(Problem problem)
        {
            var actions = new List<GeTAction>();
            foreach (var action in problem.Actions)
            {
                var newAction = ActionFactory.FromProto(action);
                actions.Add(newAction);
            }
            return actions;
        }

        public static List<GeTObjectDeclaration> ReadObjects(Problem problem)
        {
            var objects = new List<GeTObjectDeclaration>();
            foreach (var obj in problem.Objects)
            {
                var newObj = ObjectDeclarationFactory.FromProto(obj);
                objects.Add(newObj);
            }
            return objects;
        }

        public static List<GeTTypeDeclaration> ReadTypes(Problem problem)
        {
            var types = new List<GeTTypeDeclaration>();
            foreach (var type in problem.Types_)
            {
                var newType = TypeDeclarationFactory.FromProto(type);
                types.Add(newType);
            }
            return types;
        }

        public static List<GeTStateVariable> ReadInit(Problem problem)
        {
            var init = new List<GeTStateVariable>();
            foreach (var fluent in problem.InitialState)
            {
                var newFluent = StateVariableFactory.FromProto(fluent);
                // only add boolean fluent if is true as the CW assumption generates all fluents and the interface gets clapped
                if (newFluent.Value.Atom.BooleanValue != null)
                {
                    if (newFluent.Value.Atom.BooleanValue.Value)
                    {
                        init.Add(newFluent);
                        continue;
                    }
                }
                else
                {
                    init.Add(newFluent);
                }
            }
            return init;
        }

        public static Tuple<Problem, PlanGenerationResult> Read(byte[] problem, byte[] plan, string simulationName)
        {
            var parsedProblem = Problem.Parser.ParseFrom(problem);
            var parsedPlan = PlanGenerationResult.Parser.ParseFrom(plan);
            return new Tuple<Problem, PlanGenerationResult>(parsedProblem, parsedPlan);
        }
    }
}

