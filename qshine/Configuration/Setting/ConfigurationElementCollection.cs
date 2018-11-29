using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Configuration element collection.
    /// </summary>
    public class ConfigurationElementCollection<T> : ConfigurationElementCollection, IEnumerable<T>
		where T : ConfigurationElement, new()
	{
        private string elementName=String.Empty;

        /// <summary>
        /// default construct for none-named element collection
        /// </summary>
        public ConfigurationElementCollection() 
        {
        }
        /// <summary>
        /// constructor for named element collection
        /// </summary>
        /// <param name="name">Element name</param>
        public ConfigurationElementCollection(string name)
        {
            elementName = name;
        }

        /// <summary>
        /// Add an instance of <typeparamref name="T"/> to the collection.
        /// </summary>
        /// <param name="element">An instance of <typeparamref name="T"/>.</param>
        public void Add(T element)
        {
            BaseAdd(element,true);
        }

        /// <summary>
        /// Remove the named element from the collection.
        /// </summary>
        /// <param name="name">The name of the element to remove.</param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }
        
        /// <summary>
        /// Clear the collection.
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

		/// <summary>
		/// Gets the named instance of <typeparamref name="T"/> from the collection.
		/// </summary>
		/// <param name="name">The name of the <typeparamref name="T"/> instance to retrieve.</param>
		/// <returns>The instance of <typeparamref name="T"/> with the specified key; otherwise, <see langword="null"/>.</returns>
		new public T this[string name]
		{
			get
			{
				return (T)BaseGet(name);
			}
		}


		/// <summary>
		/// Gets/Sets the configuration element at the specified index location. 
		/// </summary>
		/// <param name="index">The index location of the <see name="T"/> to return. </param>
		/// <returns>The <see name="T"/> at the specified index. </returns>
		public T this[int index]
		{
			get
			{
				return (T)BaseGet(index);
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		/// <summary>
		/// Retrieve the index of the configuration element
		/// </summary>
		/// <param name="element">selected configuration element</param>
		/// <returns>The index of the specified element</returns>
		public int IndexOf(T element)
		{
			return BaseIndexOf(element);
		}

		/// <summary>
		/// Determines if the name exists in the collection.
		/// </summary>
		/// <param name="name">The name to search.</param>
		/// <returns><see langword="true"/> if the name is contained in the collection; otherwise, <see langword="false"/>.</returns>
		public bool Contains(string name)
		{
			return BaseGet(name) != null;
		}

		/// <summary>
		/// Performs the specified action on each element of the collection.
		/// </summary>
		/// <param name="action">The action to perform.</param>
		public void ForEach(Action<T> action)
		{
			for (int index = 0; index < Count; index++)
			{
				action(this[index]);
			}
		}

		/// <summary>
        /// Specifies the type of a <see cref="ConfigurationElementCollectionType"/>object.
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                if (String.IsNullOrEmpty(elementName))
                {
                    return ConfigurationElementCollectionType.AddRemoveClearMap;
                }
                else
                {
                    return ConfigurationElementCollectionType.BasicMap;
                }
            }
        }

        /// <summary>
        /// override the element name
        /// </summary>
        protected override string ElementName
        {
            get
            {
                if (String.IsNullOrEmpty(elementName))
                {
                    return base.ElementName;
                }
                return elementName;
            }
        }

        #region IEnumerable<T> Members

        /// <summary>
        /// implement the Enumerator interface
        /// </summary>
        /// <returns>return a list</returns>
        public new IEnumerator<T> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
            {
                yield return base.BaseGet(index) as T;

            }
        }

		/// <summary>
		/// Creates a new instance of a <typeparamref name="T"/> object.
		/// </summary>
		/// <returns>A new <see cref="ConfigurationElement"/>.</returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new T();
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class. 
		/// </summary>
		/// <param name="element">The <see cref="ConfigurationElement"/> to return the key for. </param>
		/// <returns>An <see cref="Object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.</returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return element.GetHashCode();
		}


        #endregion
    }

}
