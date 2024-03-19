using System.Collections.Generic;
using PDSim.Utils;
using UnityEditor;
using UnityEngine;

namespace PDSim.Protobuf
{
    /// <summary>
    /// Class to read a protobuf problem and plan and generate a PdSimProblem and PdSimInstance
    /// </summary>
    public class ProtobufReader
    {

        public void Read(byte[] problem, byte[] plan, string simulationName)
        {
            var parsedProblem = Problem.Parser.ParseFrom(problem);
            var parsedPlan = PlanGenerationResult.Parser.ParseFrom(plan);

            var pdSimProblem = ScriptableObject.CreateInstance<PdSimProblem>();
            var instance = ScriptableObject.CreateInstance<PdSimInstance>();

            pdSimProblem.domainName = parsedProblem.DomainName;

            pdSimProblem.problemName = parsedProblem.ProblemName;

            // FEATURES
            pdSimProblem.features = new List<string>();
            foreach (var feature in parsedProblem.Features)
            {
                pdSimProblem.features.Add(feature.ToString());
            }


            // TYPES DECLARATION
            pdSimProblem.typesDeclaration = new PdSimTypesDeclaration();
            pdSimProblem.typesDeclaration.Populate(parsedProblem.Types_);


            // FLUENTS
            pdSimProblem.fluents = new List<PdSimFluent>();
            foreach (var fluent in parsedProblem.Fluents)
            {
                var newFluent = new PdSimFluent(fluent);
                pdSimProblem.fluents.Add(newFluent);
            }



            // ACTIONS
            pdSimProblem.durativeActions = new List<PdSimDurativeAction>();
            pdSimProblem.instantaneousActions = new List<PdSimInstantaneousAction>();
            foreach (var action in parsedProblem.Actions)
            {
                if (action.Duration == null)
                {
                    var newAction = new PdSimInstantaneousAction(action);
                    pdSimProblem.instantaneousActions.Add(newAction);
                }
                else
                {
                    Debug.Log(action);
                    var newAction = new PdSimDurativeAction(action);
                    pdSimProblem.durativeActions.Add(newAction);
                }
            }

            // OBJECTS
            instance.objects = new List<PdSimObject>();
            foreach (var obj in parsedProblem.Objects)
            {
                var newObject = new PdSimObject(obj.Name, obj.Type);
                instance.objects.Add(newObject);
            }

            // INIT
            instance.init = new List<PdSimFluentAssignment>();
            foreach (var fluent in parsedProblem.InitialState)
            {
                var newFluent = new PdSimFluentAssignment(fluent);
                // only add boolean fluent if is true
                if (newFluent.value.contentCase == Atom.ContentOneofCase.Boolean)
                {
                    if (newFluent.value.IsTrue())
                        instance.init.Add(newFluent);
                    else
                        continue;
                }
                // default add symbolic and numeric fluents
                instance.init.Add(newFluent);
            }

            // PLAN
            instance.plan = new List<PdSimActionInstance>();
            foreach (var action in parsedPlan.Plan.Actions)
            {
                var newAction = new PdSimActionInstance(action);
                instance.plan.Add(newAction);
            }


            //Save asset
            var simulationDataRoot = AssetUtils.GetSimulationDataPath(simulationName);
            var problemPath = simulationDataRoot + "/PdSimProblem.asset";
            var instancePath = simulationDataRoot + "/PdSimInstance.asset";

            AssetDatabase.CreateAsset(pdSimProblem, problemPath);
            AssetDatabase.CreateAsset(instance, instancePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
    }
}

