using System.Collections.Generic;
using UnityEngine;

namespace PDSimSharp
{
    public class PdSimProblem : ScriptableObject
    {
        public string DomainName { get; private set; }
        public string ProblemName { get; private set; }
        public PdSimTypesDeclaration TypesDeclaration { get; private set; }
        public List<PdSimObject> Objects { get; private set; }

        public static PdSimProblem Create(byte[] problemData)
        {
            var problem = Problem.Parser.ParseFrom(problemData);

            var instance = CreateInstance<PdSimProblem>();

            instance.DomainName = problem.DomainName;

            instance.ProblemName = problem.ProblemName;

            instance.TypesDeclaration = new PdSimTypesDeclaration(problem.Types_);

            instance.Objects = new List<PdSimObject>();
            foreach (var obj in problem.Objects)
            {
                var newObject = new PdSimObject(obj.Name, obj.Type);
                instance.Objects.Add(newObject);
            }



            return instance;
        }
    }
}