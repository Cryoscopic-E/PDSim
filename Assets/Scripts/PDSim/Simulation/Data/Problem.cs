using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using PDSim.Components;
using Unity.VisualScripting;

namespace PDSim.Simulation.Data
{
	public class Problem : PdSimData
	{
		public List<PdObject> objects;
		public List<PdBooleanPredicate> initialState;
		public List<PdBooleanPredicate> goalState;

		public override void CreateInstance(JObject parsedPddl)
		{
			//Initial State
			initialState = new List<PdBooleanPredicate>();
			var init = parsedPddl["init"];
			foreach (var k in init.Children<JProperty>())
			{
				foreach (var v in k.Value.Children<JObject>())
				{
					var fluent = new PdBooleanPredicate
					{
						name = k.Name,
						attributes = new List<string>()
					};

					fluent.value = v["value"].ToObject<bool>();

					foreach (var a in v["args"])
					{
						fluent.attributes.Add(a.ToString());
					}

					initialState.Add(fluent);
				}
			}

			//Goal State
			goalState = new List<PdBooleanPredicate>();
			var goal = parsedPddl["goal"];
			foreach (var k in goal.Children<JProperty>())
			{
				foreach (var v in k.Value.Children<JObject>())
				{
					var fluent = new PdBooleanPredicate
					{
						name = k.Name,
						attributes = new List<string>()
					};

					fluent.value = v["value"].ToObject<bool>();

					foreach (var a in v["args"])
					{
						fluent.attributes.Add(a.ToString());
					}

					goalState.Add(fluent);
				}
			}

			//Objects
			var objs = parsedPddl["objects"];
			objects = objs.Children<JProperty>().Select(k => new PdObject { name = k.Name, type = k.Value.ToString() }).ToList();
		}
	}
}