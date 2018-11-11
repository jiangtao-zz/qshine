using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Represents an object in different state.
    /// The state could be a boolean, enum or others to indicate different state of the object.
    /// The object could be any type object.
    /// </summary>
    public class StateObject<TS,TO>
    {
        /// <summary>
        /// Ctor::
        /// </summary>
        /// <param name="state"></param>
        /// <param name="objectData"></param>
        public StateObject(TS state, TO objectData)
        {
            State = state;
            ObjectData = objectData;
        }

        /// <summary>
        /// Presents a state
        /// </summary>
        public TS State { get; set; }

        /// <summary>
        /// Presents a state object
        /// </summary>
        public TO ObjectData { get; set; }
    }
}
