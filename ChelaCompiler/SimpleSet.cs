using System;
using System.Collections;
using System.Collections.Generic;

namespace Chela.Compiler
{
    /// <summary>
    /// Simple set.
    /// </summary>
    public class SimpleSet<ElementType>: IEnumerable<ElementType>
    {
        private Dictionary<ElementType, ElementType> container;

        public SimpleSet()
        {
            container = new Dictionary<ElementType, ElementType> ();
        }

        /// <summary>
        /// Checks if an element is present in the set.
        /// </summary>
        public bool Contains(ElementType element)
        {
            return container.ContainsKey(element);
        }

        /// <summary>
        /// The number of elements in the set.
        /// </summary>
        public int Count {
            get {
                return container.Count;
            }
        }

        /// <summary>
        /// Add the specified element into the set..
        /// </summary>
        public void Add(ElementType element)
        {
            container.Add(element, element);
        }

        /// <summary>
        /// Gets the element specified or add it. Used to ensure unique references for types.
        /// </summary>
        public ElementType GetOrAdd(ElementType newElement)
        {
            ElementType old;
            if(container.TryGetValue(newElement, out old))
                return old;

            container.Add(newElement, newElement);
            return newElement;
        }

        /// <summary>
        /// Finds an element, returning it or null.
        /// </summary>
        public ElementType Find(ElementType element)
        {
            ElementType old;
            if(container.TryGetValue(element, out old))
                return old;
            return default(ElementType);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<ElementType> GetEnumerator()
        {
            return container.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

