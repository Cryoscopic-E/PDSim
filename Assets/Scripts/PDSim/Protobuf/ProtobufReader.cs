using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using UnityEngine;

namespace PDSim.Protobuf
{
    public class ProtobufReader
    {
        private Problem problem;
        private PdSimProblem instance;

        public PdSimProblem Read(byte[] data)
        {
            problem = Problem.Parser.ParseFrom(data);

            instance = ScriptableObject.CreateInstance<PdSimProblem>();

            instance.DomainName = problem.DomainName;

            instance.ProblemName = problem.ProblemName;

            instance.TypesDeclaration = new PdSimTypesDeclaration();
            instance.TypesDeclaration.TypeTree.Populate(problem.Types_);

            ParseFluents();

            ParseObjects();

            ParseActions();



            return instance;
        }

        private void ParseFluents()
        {
            instance.Fluents = new List<PdSimFluent>();
            foreach (var fluent in problem.Fluents)
            {
                var newFluent = ParseFluent(fluent);
                instance.Fluents.Add(newFluent);
            }
        }

        private PdSimFluent ParseFluent(Fluent fluent)
        {
            return new PdSimFluent(fluent);
        }


        private void ParseActions()
        {
            instance.Actions = new List<PdSimAction>();
            foreach (var action in problem.Actions)
            {
                var newAction = ParseAction(action);
            }
        }

        private PdSimAction ParseAction(Action action)
        {
            return null;
        }

        private PdSimParameter ParseParameter(Parameter parameter)
        {
            return null;
        }

        private void ParseObjects()
        {
            instance.Objects = new List<PdSimObject>();
            foreach (var obj in problem.Objects)
            {
                var newObject = ParseObject(obj);
                instance.Objects.Add(newObject);
            }
        }

        private PdSimObject ParseObject(ObjectDeclaration obj)
        {
            return new PdSimObject(obj.Name, obj.Type);
        }
    }
}

