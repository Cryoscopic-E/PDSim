namespace PDSim.Utils
{
    /// <summary>
    /// Contains common file paths used throughout the PDSim package.
    /// </summary>
    public static class CommonPaths
    {
        #region Public API

        /// <summary>
        /// Path to the Create Animation Dialog UXML.
        /// </summary>
        public static readonly string AnimationDialogUI = "Packages/com.cryoscopic-e.pdsim/Resources/EditorUI/CreateAnimationDialog.uxml";

        /// <summary>
        /// Path to the Create Simulation Window UXML.
        /// </summary>
        public static readonly string CreateSimWindowUI = "Packages/com.cryoscopic-e.pdsim/Resources/EditorUI/CreateSimulationWindow.uxml";

        /// <summary>
        /// Path to the Duplicate Simulation Window UXML.
        /// </summary>
        public static readonly string DuplicateWindowUI = "Packages/com.cryoscopic-e.pdsim/Resources/EditorUI/DuplicateSimulationWindow.uxml";

        /// <summary>
        /// Path to the Predicate Animation Attribute UXML.
        /// </summary>
        public static readonly string PredicateAnimationAttributeUI = "Packages/com.cryoscopic-e.pdsim/Resources/EditorUI/PredicateAnimationAttribute.uxml";

        /// <summary>
        /// Path to the Template Visualisation Scene.
        /// </summary>
        public static readonly string TemplateVisualisationScene = "Packages/com.cryoscopic-e.pdsim/Editor/VisualisationTemplate/VisualisationScene.scenetemplate";

        /// <summary>
        /// Path to the Fluent Animation Prefab.
        /// </summary>
        public static readonly string FluentAnimationPrefab = "Packages/com.cryoscopic-e.pdsim/Runtime/Prefabs/FluentAnimation.prefab";

        /// <summary>
        /// Path to the PDSim Object Prefab.
        /// </summary>
        public static readonly string PdsimObjectPrefab = "Packages/com.cryoscopic-e.pdsim/Runtime/Prefabs/VisualisationObject.prefab";

        /// <summary>
        /// Root folder for all simulations in the project.
        /// </summary>
        public static readonly string SimulationsRootFolder = "Assets/Scenes/";

        #endregion
    }
}
