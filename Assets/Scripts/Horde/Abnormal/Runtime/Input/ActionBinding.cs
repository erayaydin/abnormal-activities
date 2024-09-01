using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Horde.Abnormal.Input
{
    [Serializable]
    public class ActionBinding
    {
        /// <summary>
        /// Gets the input action associated with this binding.
        /// </summary>
        public InputAction InputAction
        {
            get;
            private set;
        }

        /// <summary>
        /// A list of all binding lists associated with the input action.
        /// </summary>
        public readonly List<BindingList> BindingLists = new();

        /// <summary>
        /// Indicates whether this binding is not allowed to be re-bound.
        /// </summary>
        public readonly bool IsNotReBindable;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionBinding"/> with a specified input action.
        /// This constructor parses the input action's bindings, organizing them into binding lists and determining if the action is re-bindable.
        /// </summary>
        /// <param name="inputAction">The input action to be associated with this binding.</param>
        public ActionBinding(InputAction inputAction)
        {
            InputAction = inputAction;

            var bindings = inputAction.bindings;
            var totalBindings = bindings.Count;

            // Determines if the action is not re-bindable based on its interactions.
            IsNotReBindable = inputAction.interactions.Contains("NotReBindable");

            for (var bindingIndex = 0; bindingIndex < totalBindings; bindingIndex++)
            {
                var isComposite = inputAction.bindings[bindingIndex].isComposite;

                if (isComposite)
                {
                    var firstPartIndex = bindingIndex + 1;
                    var lastPartIndex = firstPartIndex;
                    // Identifies the range of bindings that are part of the composite.
                    while (lastPartIndex < bindings.Count && bindings[lastPartIndex].isPartOfComposite)
                        ++lastPartIndex;
                    var totalParts = lastPartIndex - firstPartIndex;

                    var parts = new List<CompositePart>();
                    // Creates composite parts for each binding in the composite.
                    for (var i = 0; i < totalParts; i++)
                    {
                        parts.Add(CompositePart.CreateFromBinding(bindings[firstPartIndex + i], inputAction, firstPartIndex + i, IsNotReBindable));
                    }

                    // Adds the composite binding to the list of binding lists.
                    BindingLists.Add(new BindingList(parts));
                    bindingIndex += totalParts;
                    continue;
                }
                
                // Adds non-composite bindings directly to the list of binding lists.
                BindingLists.Add(new BindingList(CompositePart.CreateFromBinding(bindings[bindingIndex], inputAction, bindingIndex, IsNotReBindable)));
            }
        }
    }
}