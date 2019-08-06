using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace qshine.Specification
{
    /// <summary>
    /// Define a rule Collection
    /// </summary>
    /// <typeparam name="T">type of rule</typeparam>
    public class RuleCollection<T>:Collection<T>
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public RuleCollection() : base(new List<T>())
        {
        }
        /// <summary>
        /// Remove all the elements that match the condition
        /// </summary>
        /// <param name="match">Match condition</param>
        public void RemoveAll(Predicate<T> match)
        {
            var list = (List <T>) Items;
            list.RemoveAll(match);
        }
    }
}
