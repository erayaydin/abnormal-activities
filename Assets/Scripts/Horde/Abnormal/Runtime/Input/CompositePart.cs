using System;
using System.Linq;
using Horde.Abnormal.Shared;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Horde.Abnormal.Input
{
    [Serializable]
    public class CompositePart : ICloneable
    {
        /// <summary>
        /// The name of the input action associated with this composite part.
        /// </summary>
        public string inputActionName;

        /// <summary>
        /// The original binding path for this part.
        /// </summary>
        public string bindingPath;

        /// <summary>
        /// The override path for this binding, if any.
        /// </summary>
        public string overridePath;

        /// <summary>
        /// The display string for this part, typically used for UI display.
        /// </summary>
        public string displayString;

        /// <summary>
        /// A string representation of the part, often used for identifying the part within a composite.
        /// </summary>
        public string partString;

        /// <summary>
        /// The groups this binding is part of, split by the InputBinding separator.
        /// </summary>
        public string[] groups;

        /// <summary>
        /// Indicates whether this part is not re-bindable.
        /// </summary>
        public bool isNotReBindable;

        /// <summary>
        /// The index of this binding within the action's binding list.
        /// </summary>
        public int bindingIndex;

        /// <summary>
        /// Creates a deep copy of a <see cref="CompositePart"/> instance.
        /// </summary>
        /// <returns>A new <see cref="CompositePart"/> instance with the same values as the original.</returns>
        public object Clone() =>
            new CompositePart
            {
                inputActionName = inputActionName,
                bindingPath = bindingPath,
                overridePath = overridePath,
                displayString = displayString,
                partString = partString,
                groups = groups,
                bindingIndex = bindingIndex,
                isNotReBindable = isNotReBindable
            };

        /// <summary>
        /// Generates a more readable version of the current binding path.
        /// </summary>
        /// <returns>A string representing the pretty path of the binding.</returns>
        public string GetPrettyPath()
        {
            var path = string.IsNullOrEmpty(overridePath) ? bindingPath : overridePath;
            
            return path
                .Replace("<", "")
                .Replace(">", "")
                .Replace("/", ".");
        }
        
        /// <summary>
        /// Creates a <see cref="CompositePart">Composite Part</see> instance from an <see cref="InputBinding">Input Binding</see> and an <see cref="InputAction">Input Action</see>.
        /// </summary>
        /// <param name="binding">The input binding from which to create the part.</param>
        /// <param name="inputAction">The input action associated with the binding.</param>
        /// <param name="index">The index of the binding within the action's binding list.</param>
        /// <param name="notReBindable">Indicates whether the binding is rebindable or not.</param>
        /// <returns>A new <see cref="CompositePart"/> instance based on the provided binding and action.</returns>
        public static CompositePart CreateFromBinding(InputBinding binding, InputAction inputAction, int index, bool notReBindable = false)
        {
            var groups = binding.groups.Split(InputBinding.Separator).ToArray();
            var bindingPath = binding.path;
            var displayString = binding.PathDisplay();

            var partString = string.Empty;
            var bindingName = binding.name;
            if (!string.IsNullOrEmpty(bindingName))
            {
                var nameParameters = NameAndParameters.Parse(bindingName);
                partString = nameParameters.name.ToTitleCase();
            }

            var isNotReBindable = notReBindable || binding.interactions.Contains("NotReBindable");

            return new CompositePart
            {
                inputActionName = inputAction.name,
                bindingPath = bindingPath,
                overridePath = null,
                displayString = displayString,
                partString = partString,
                groups = groups,
                isNotReBindable = isNotReBindable,
                bindingIndex = index
            };
        }
    }
}