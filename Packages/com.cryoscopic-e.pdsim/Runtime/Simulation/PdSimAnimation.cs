using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;

public abstract class PdSimAnimation : MonoBehaviour
{
    public List<AnimationData> animationData;

    public bool AddAnimationData(string animationName, List<string> attributes, ScriptMachine machine, int order = 0)
    {
         foreach (var data in animationData)
            {
                if (data.name == machine.name)
                {
                    return false;
                }
            }


            animationData.Add(new AnimationData()
            {
                name = animationName,
                parameters = attributes,
                machine = machine
            });
            return true;
    }


    [System.Serializable]
    public class AnimationData
    {
        public string name;
        public List<string> parameters;
        public ScriptMachine machine;
    }
}
