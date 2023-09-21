using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PDSim.Protobuf
{
    public class ProtobufReader
    {

        public void Read(byte[] data)
        {
            var parsed = Problem.Parser.ParseFrom(data);

            var problem = ScriptableObject.CreateInstance<PdSimProblem>();
            var instance = ScriptableObject.CreateInstance<PdSimInstance>();

            problem.domainName = parsed.DomainName;

            problem.problemName = parsed.ProblemName;

            // TYPES DECLARATION
            problem.typesDeclaration = new PdSimTypesDeclaration();
            problem.typesDeclaration.Populate(parsed.Types_);


            // FLUENTS
            problem.fluents = new List<PdSimFluent>();
            foreach (var fluent in parsed.Fluents)
            {
                var newFluent = new PdSimFluent(fluent);
                problem.fluents.Add(newFluent);
            }



            // ACTIONS
            problem.durativeActions = new List<PdSimDurativeAction>();
            problem.instantActions = new List<PdSimInstantaneousAction>();
            foreach (var action in parsed.Actions)
            {
                if (action.Duration == null)
                {
                    var newAction = new PdSimInstantaneousAction(action);
                    problem.instantActions.Add(newAction);
                }
                else
                {
                    var newAction = new PdSimDurativeAction(action);
                    problem.durativeActions.Add(newAction);
                }
            }

            // OBJECTS
            instance.objects = new List<PdSimObject>();
            foreach (var obj in parsed.Objects)
            {
                var newObject = new PdSimObject(obj.Name, obj.Type);
                instance.objects.Add(newObject);
            }

            // INIT
            instance.init = new List<PdSimFluentAssignment>();
            foreach (var fluent in parsed.InitialState)
            {

                var val = fluent.Value.Atom;

                if (val.ContentCase == Atom.ContentOneofCase.Boolean && val.Boolean)
                {
                    var newFluent = new PdSimFluentAssignment(fluent);
                    instance.init.Add(newFluent);
                }
            }




            //Save asset
            var problemPath = "Assets/Testprotobuf/ProtobufProblem.asset";
            var instancePath = "Assets/Testprotobuf/ProtobufInstance.asset";

            AssetDatabase.CreateAsset(problem, problemPath);
            AssetDatabase.CreateAsset(instance, instancePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
    }
}

