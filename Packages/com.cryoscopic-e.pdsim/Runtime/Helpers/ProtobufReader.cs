using GeTPlan.Core.Models;
using GeTPlan.Core.Models.Expressions;
using GeTPlan.Protobuf.Mappers;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDSim.Helpers
{
    /// <summary>
    /// Utility class to read a protobuf problem and plan and generate PDSim models.
    /// Uses GeTPlan.Protobuf mappers for conversion.
    /// </summary>
    public class ProtobufReader
    {
        #region Public API

        /// <summary>
        /// Reads the fluent definitions from the protobuf problem.
        /// </summary>
        /// <param name="problem">The protobuf problem.</param>
        /// <returns>A list of <see cref="PredicateDefinition"/>.</returns>
        public static List<PredicateDefinition> ReadFluents(Problem problem)
        {
            // The PDSim 'Animations' class expects PredicateDefinitions to build its metadata.
            var types = ReadTypes(problem).ToDictionary(t => t.Name);
            return problem.Fluents.Select(f => f.FromProto(types)).ToList();
        }

        /// <summary>
        /// Reads the action definitions from the protobuf problem.
        /// </summary>
        /// <param name="problem">The protobuf problem.</param>
        /// <returns>A list of <see cref="ActionDefinition"/>.</returns>
        public static List<ActionDefinition> ReadActions(Problem problem)
        {
            var types = ReadTypes(problem).ToDictionary(t => t.Name);
            return problem.Actions.Select(a => a.FromProto(types)).ToList();
        }

        /// <summary>
        /// Reads the plan objects from the protobuf problem.
        /// </summary>
        /// <param name="problem">The protobuf problem.</param>
        /// <returns>A list of <see cref="PlanObject"/>.</returns>
        public static List<PlanObject> ReadObjects(Problem problem)
        {
            var types = ReadTypes(problem).ToDictionary(t => t.Name);
            return problem.Objects.Select(o => o.FromProto(types)).ToList();
        }

        /// <summary>
        /// Reads the plan types from the protobuf problem.
        /// </summary>
        /// <param name="problem">The protobuf problem.</param>
        /// <returns>A list of <see cref="PlanType"/>.</returns>
        public static List<PlanType> ReadTypes(Problem problem)
        {
            // Build the full type hierarchy
            var allTypes = new Dictionary<string, PlanType>();

            // First pass: create all types
            foreach (var protoType in problem.Types_)
            {
                if (!allTypes.ContainsKey(protoType.TypeName))
                {
                    allTypes[protoType.TypeName] = new PlanType(protoType.TypeName);
                }
            }

            // Second pass: set parents (Handled by TypeMapper)
            return GeTPlan.Protobuf.Mappers.TypeMapper.FromProtoCollection(problem.Types_).Values.ToList();
        }

        /// <summary>
        /// Reads the initial state from the protobuf problem.
        /// </summary>
        /// <param name="problem">The protobuf problem.</param>
        /// <returns>A list of fluent expressions and their values representing the initial state.</returns>
        public static List<(FluentExpression Fluent, object Value)> ReadInit(Problem problem)
        {
            var coreProblem = problem.FromProto();
            return coreProblem.InitialState.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        /// <summary>
        /// Reads the protobuf problem and plan from byte arrays.
        /// </summary>
        /// <param name="problem">The protobuf problem bytes.</param>
        /// <param name="plan">The protobuf plan bytes.</param>
        /// <returns>A tuple containing the parsed <see cref="Problem"/> and <see cref="PlanGenerationResult"/>.</returns>
        public static Tuple<Problem, PlanGenerationResult> Read(byte[] problem, byte[] plan)
        {
            var parsedProblem = Problem.Parser.ParseFrom(problem);
            var parsedPlan = PlanGenerationResult.Parser.ParseFrom(plan);
            return new Tuple<Problem, PlanGenerationResult>(parsedProblem, parsedPlan);
        }

        #endregion
    }
}
