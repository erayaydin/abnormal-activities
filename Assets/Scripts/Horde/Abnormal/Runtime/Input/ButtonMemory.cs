using UnityEngine;

namespace Horde.Abnormal.Input
{
    public class ButtonMemory
    {
        /// <summary>
        /// The instance of MonoBehaviour that this ButtonMemory is associated with.
        /// </summary>
        public readonly MonoBehaviour Instance;

        /// <summary>
        /// The name of the action that this ButtonMemory represents.
        /// </summary>
        public readonly string ActionName;
        
        /// <summary>
        /// Initializes a new instance of the ButtonMemory class.
        /// </summary>
        /// <param name="instance">The MonoBehaviour instance that this ButtonMemory is associated with.</param>
        /// <param name="actionName">The name of the action that this ButtonMemory represents.</param>
        public ButtonMemory(MonoBehaviour instance, string actionName)
        {
            Instance = instance;
            ActionName = actionName;
        }
    }
}