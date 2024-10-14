using System.Collections.Generic;
using PDSim.Simulation;
using UnityEngine;

public class PdSimEventManager : MonoBehaviour
{
    // Singleton instance
    public static PdSimEventManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public delegate void EffectEventDelegate(string effectName, Dictionary<string, PdSimSimulationObject> parameters);

    private Dictionary<string, EffectEventDelegate> effectEventSubscribers = new Dictionary<string, EffectEventDelegate>();

    /// <summary>
    /// Subscribe to an effect event.
    /// </summary>
    public void SubscribeToEffect(string effectName, EffectEventDelegate listener)
    {
        if (!effectEventSubscribers.ContainsKey(effectName))
        {
            effectEventSubscribers[effectName] = null;
        }
        effectEventSubscribers[effectName] += listener;
    }

    /// <summary>
    /// Unsubscribe from an effect event.
    /// </summary>
    public void UnsubscribeFromEffect(string effectName, EffectEventDelegate listener)
    {
        if (effectEventSubscribers.ContainsKey(effectName))
        {
            effectEventSubscribers[effectName] -= listener;
            if (effectEventSubscribers[effectName] == null)
            {
                effectEventSubscribers.Remove(effectName);
            }
        }
    }

    /// <summary>
    /// Trigger an effect event.
    /// </summary>
    public void TriggerEffect(string effectName, Dictionary<string, PdSimSimulationObject> parameters = null)
    {
        if (effectEventSubscribers.ContainsKey(effectName))
        {
            effectEventSubscribers[effectName]?.Invoke(effectName, parameters);
        }
    }
}