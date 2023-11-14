namespace PDSim.VisualScripting.Events
{
    /// <summary>
    /// A class containing all the event names used in the visual scripting system.
    /// PDSim use this to get the evnts when a effect starts and ends.
    /// </summary>
    public static class EventNames
    {
        public static string actionEffectStart = "OnEffectStart";
        public static string actionEffectEnd = "OnEffectEnd";
        public static string actionStart = "OnActionStart";
        public static string actionEnd = "OnActionEnd";
    }
}