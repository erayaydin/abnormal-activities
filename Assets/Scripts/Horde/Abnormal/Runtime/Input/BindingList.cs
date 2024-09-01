using System.Collections.Generic;

namespace Horde.Abnormal.Input
{
    /// <summary>
    /// Represents a collection of <see cref="CompositePart">Composite Part</see> objects.
    /// </summary>
    public class BindingList
    {
        /// <summary>
        /// Gets the list of <see cref="CompositePart">Composite Part</see> objects.
        /// </summary>
        public readonly List<CompositePart> CompositeParts = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingList">Binding List</see> class with a list of <see cref="CompositePart">Composite Part</see> objects.
        /// </summary>
        /// <param name="parts">The list of <see cref="CompositePart">Composite Part</see> objects to initialize the collection with.</param>
        public BindingList(List<CompositePart> parts)
        {
            CompositeParts = parts;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingList">Binding List</see> class with a single <see cref="CompositePart">Composite Part</see> object.
        /// </summary>
        /// <param name="part">The <see cref="CompositePart">Composite Part</see> object to add to the collection.</param>
        public BindingList(CompositePart part)
        {
            CompositeParts.Add(part);
        }
    }
}