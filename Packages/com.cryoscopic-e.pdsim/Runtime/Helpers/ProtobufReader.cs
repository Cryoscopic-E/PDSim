using GeTPlan.Core.Models;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models.Expressions;
using GeTPlan.Protobuf.Mappers;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDSim.Helpers
{
    /// <summary>
    /// Class to read a protobuf problem and plan and generate PdSim models.
    /// Updated to use GeTPlan.Protobuf mappers.
    /// </summary>
    public class ProtobufReader
    {
        public static List<PredicateDefinition> ReadFluents(Problem problem)
        {
            // The PDSim 'Animations' class expects PredicateDefinitions to build its metadata.
            var types = ReadTypes(problem).ToDictionary(t => t.Name);
            return problem.Fluents.Select(f => f.FromProto(types)).ToList();
        }

        public static List<ActionDefinition> ReadActions(Problem problem)
        {
            var types = ReadTypes(problem).ToDictionary(t => t.Name);
            return problem.Actions.Select(a => a.FromProto(types)).ToList();
        }

        public static List<PlanObject> ReadObjects(Problem problem)
        {
            var types = ReadTypes(problem).ToDictionary(t => t.Name);
            return problem.Objects.Select(o => o.FromProto(types)).ToList();
        }

        public static List<PlanType> ReadTypes(Problem problem)
        {
            // Build the full type hierarchy
            var allTypes = new Dictionary<string, PlanType>();
            
            // First pass: create all types
            foreach (var protoType in problem.Types_)
            {
                if (!allTypes.ContainsKey(protoType.TypeName))
                    allTypes[protoType.TypeName] = new PlanType(protoType.TypeName);
            }

            // Second pass: set parents
            foreach (var protoType in problem.Types_)
            {
                if (!string.IsNullOrEmpty(protoType.ParentType) && allTypes.ContainsKey(protoType.ParentType))
                {
                    // Update the type with its parent. 
                    // Since PlanType is an immutable record-like structure in some versions but here we might need to recreate.
                    // Actually TypeMapper.FromProto handles this correctly if given the context.
                }
            }

            return GeTPlan.Protobuf.Mappers.TypeMapper.FromProtoCollection(problem.Types_).Values.ToList();
        }

        public static List<(FluentExpression Fluent, object Value)> ReadInit(Problem problem)
        {
            var coreProblem = problem.FromProto();
            return coreProblem.InitialState.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        public static Tuple<Problem, PlanGenerationResult> Read(byte[] problem, byte[] plan)
        {
            var parsedProblem = Problem.Parser.ParseFrom(problem);
            var parsedPlan = PlanGenerationResult.Parser.ParseFrom(plan);
            return new Tuple<Problem, PlanGenerationResult>(parsedProblem, parsedPlan);
        }
    }
}
