using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDSim.Simulation;

public class StateComponent : MonoBehaviour
{
    private State _state; 
    // Start is called before the first frame update
    void Start()
    {
        _state = new State();
        _state.AddOrUpdate(new StateNode("at", bool.FalseString , StateNode.BooleanType ,new List<string> { "r1", "l1" }));
        _state.AddOrUpdate(new StateNode("at", bool.TrueString , StateNode.BooleanType, new List<string> { "r2", "l2" }));
        _state.AddOrUpdate(new StateNode("coins", "1" , StateNode.NumberType , StateNode.NoParam));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        // Draw box
        GUI.Box(new Rect(10, 10, 200, 200), "State");
        // Draw state
        var state = _state.GetState();
        for (int i = 0; i < state.Count; i++)
        {
            GUI.Label(new Rect(20, 30 + (i * 20), 150, 20), state[i].ToString());
        }
    
        if (GUI.Button(new Rect(20, 200, 80, 20), "Add Coin"))
        {
            var c = _state.Query("coins", StateNode.NoParam);
            float n = 0;
            if (c.fluentType == StateNode.NumberType)
            {
                n = float.Parse(c.fluentValue) + 1.0f;
            }
            _state.AddOrUpdate(new StateNode("coins", n.ToString() , StateNode.NumberType , StateNode.NoParam));
        }
    }
}
