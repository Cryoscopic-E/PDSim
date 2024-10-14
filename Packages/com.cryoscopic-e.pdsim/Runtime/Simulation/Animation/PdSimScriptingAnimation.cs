using System.Collections;
using PDSim.PlanningModel;
using PDSim.Simulation;
using UnityEngine;

public abstract class PdSimScriptingAnimation : MonoBehaviour
{
    public PdSimFluent metaData;
    public string animationName;
    public abstract void AnimationStart();
    public abstract void AnimationEnd();
    public abstract IEnumerator AnimationBody();
}