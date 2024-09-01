using System.Collections.Generic;

namespace Horde.Abnormal.Input
{
    /// <summary>
    /// Represents a scheme of actions.
    /// </summary>
    public class ActionScheme
    {
        /// <summary>
        /// The name of the action scheme.
        /// </summary>
        public readonly string Name;
        
        /// <summary>
        /// The list of action bindings associated with this action scheme.
        /// </summary>
        public readonly List<ActionBinding> ActionBindings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionScheme">Action Scheme</see> class with a specified name and list of action bindings.
        /// </summary>
        /// <param name="name">The name of the action scheme.</param>
        /// <param name="actionBindings">The list of action bindings to be associated with this action scheme.</param>
        public ActionScheme(string name, List<ActionBinding> actionBindings)
        {
            Name = name;
            ActionBindings = actionBindings;
        }
    }
}