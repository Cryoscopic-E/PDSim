using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PDSim.Simulation.Data
{
	/// <summary>
	///   Base class for all data classes that are created from the PDDL file.
	///   All components are treated as ScriptableObjects so that they can be serialized and loaded in the scene.
	/// </summary>
	public abstract class PdSimData : ScriptableObject
	{
		public abstract void CreateInstance(JObject parsedPddl);
	}
}