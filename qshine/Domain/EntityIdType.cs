using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Domain
{
    /// <summary>
    /// Defines a common entity id type.
    /// The common id type could be a Guid, long, string or a complex data type.
    /// In a business system, each type of entity may use different type of identity.
    /// Use EntityIdType to adopt any type of entity Id.
    /// 
    /// The class only defined two common entity id type: Guid(?) and long(?) type.
    /// To use other type entity you need create a derived class.
    /// </summary>
    public class EntityIdType
    {
        //actual id value
        object _value;

        #region Ctor.

        /// <summary>
        /// Construct a default Id.
        /// </summary>
        public EntityIdType() {}

        /// <summary>
        /// Construct a long type entity id
        /// </summary>
        /// <param name="id">entity id</param>
        public EntityIdType(long id)
        {
            _value = id;
        }

        /// <summary>
        /// Construct a nullable long type id
        /// </summary>
        /// <param name="id">entity id</param>
        public EntityIdType(long? id)
        {
            _value = id;
        }

        /// <summary>
        /// Construct a Guid type id
        /// </summary>
        /// <param name="id">entity id</param>
        public EntityIdType(Guid id)
        {
            _value = id;
        }

        /// <summary>
        /// Construct a nullable Guid id
        /// </summary>
        /// <param name="id">entity id</param>
        public EntityIdType(Guid? id)
        {
            _value = id;
        }

        /// <summary>
        /// Reserved for derived class.
        /// </summary>
        /// <param name="id"></param>
        protected EntityIdType (object id)
        {
            _value = id;
        }
        #endregion

        #region GetValue
        /// <summary>
        /// Get given type id value
        /// </summary>
        public virtual T GetValue<T>()
        {
            return _value==null?default(T): (T) _value;
        }
        #endregion

        #region GetNullableValue

        /// <summary>
        /// Get given nullable type id value
        /// </summary>
        public virtual Nullable<T> GetNullableValue<T>()
            where T:struct
        {
            if (_value != null)
            {
                return (T)_value;
            }
            return null;
        }

        #endregion

        #region assignment =

        /// <summary>
        /// Overwrite operator assignment "=" for Guid type id
        /// </summary>
        /// <param name="value">Guid type of id to be assigned to EntityIdType instance </param>
        public static implicit operator EntityIdType(Guid value)
        {
            return new EntityIdType(value);
        }

        /// <summary>
        /// Overwrite operator assignment "=" for long type id
        /// </summary>
        /// <param name="value">Long type of id to be assigned to EntityIdType instance </param>
        public static implicit operator EntityIdType(long value)
        {
            return new EntityIdType(value);
        }

        /// <summary>
        /// Overwrite operator assignment "=" for Guid type id
        /// </summary>
        /// <param name="value">Guid type of id to be assigned to EntityIdType instance </param>
        public static implicit operator EntityIdType(Guid? value)
        {
            return new EntityIdType(value);
        }

        /// <summary>
        /// Overwrite operator assignment "=" for long type id
        /// </summary>
        /// <param name="value">Long type of id to be assigned to EntityIdType instance </param>
        public static implicit operator EntityIdType(long? value)
        {
            return new EntityIdType(value);
        }

        #endregion

        #region Override
        /// <summary>
        /// Compare ids
        /// </summary>
        /// <param name="value">A EntityIdType, Guid, Long or other id type instance to be compared.</param>
        /// <returns>True if two ids are identical.</returns>
        public override bool Equals(object value)
        {
            if (value == null) { return _value==null; }
            if (_value == null) { return value == null; }

            //compare same type
            if (value is EntityIdType)
            {
                return ToString().Equals(((EntityIdType)value).ToString());
            }

            //compare original type
            return _value.Equals(value);
        }

        /// <summary>
        /// Get hash code of the id.
        /// </summary>
        /// <returns>Entity id hash code.</returns>
        public override int GetHashCode()
        {
            return _value==null? -1: _value.GetHashCode();
        }

        /// <summary>
        /// Get id string value;
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _value==null?"":_value.ToString();
        }
        #endregion
    }

}
