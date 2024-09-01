using System;
using UnityEngine.InputSystem;

namespace Horde.Abnormal.Input
{
    /// <summary>
    /// Represents the context for rebinding input actions, including details about the action to be rebound,
    /// the action map it belongs to, the specific binding index, and any override paths or display names.
    /// </summary>
    [Serializable]
    public class RebindContext
    {
        /// <summary>
        /// The input action to be rebound.
        /// </summary>
        public InputAction action;
        
        /// <summary>
        /// The name of the action map that the action belongs to.
        /// </summary>
        public string actionMap;
        
        /// <summary>
        /// The index of the binding within the action to be rebound.
        /// </summary>
        public int bindingIndex;
        
        /// <summary>
        /// The override path for the binding, specifying a new control with binding path.
        /// </summary>
        public string overridePath;
        
        /// <summary>
        /// The display representation for the binding, used for UI or informational purposes.
        /// </summary>
        public string display;
    }
}