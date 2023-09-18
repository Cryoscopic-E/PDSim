using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PDSim.Protobuf
{
    public class ProtobufReader
    {

        public void Read(byte[] data)
        {
            var problem = Problem.Parser.ParseFrom(data);

            var instance = ScriptableObject.CreateInstance<PdSimProblem>();

            instance.domainName = problem.DomainName;

            instance.problemName = problem.ProblemName;

            // TYPES DECLARATION
            instance.typesDeclaration = new PdSimTypesDeclaration();
            instance.typesDeclaration.Populate(problem.Types_);


            // FLUENTS
            instance.fluents = new List<PdSimFluent>();
            foreach (var fluent in problem.Fluents)
            {
                var newFluent = new PdSimFluent(fluent);
                instance.fluents.Add(newFluent);
            }

            instance.objects = new List<PdSimObject>();
            foreach (var obj in problem.Objects)
            {
                var newObject = new PdSimObject(obj.Name, obj.Type);
                instance.objects.Add(newObject);
            }

            // ACTIONS
            instance.actions = new List<PdSimAction>();
            foreach (var action in problem.Actions)
            {
                if (action.Duration == null)
                {
                    var newAction = new PdSimInstantaneousAction(action);
                    instance.actions.Add(newAction);
                }
                else
                {
                    var newAction = new PdSimDurativeAction(action);
                    instance.actions.Add(newAction);
                }
            }


            // INIT
            instance.init = new List<PdSimFluentAssignment>();
            foreach (var fluent in problem.InitialState)
            {

                var val = fluent.Value.Atom;

                if (val.ContentCase == Atom.ContentOneofCase.Boolean && val.Boolean)
                {
                    var newFluent = new PdSimFluentAssignment(fluent);
                    instance.init.Add(newFluent);
                }
            }

            //Save asset
            var path = "Assets/Testprotobuf/ProtobufProblem.asset";

            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }



        // private void ParseActions()
        // {
        //     instance.Actions = new List<PdSimAction>();
        //     foreach (var action in problem.Actions)
        //     {
        //         var newAction = ParseAction(action);
        //     }
        // }

        // private PdSimAction ParseAction(Action action)
        // {
        //     return null;
        // }

    }
}

