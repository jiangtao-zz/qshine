using System;
using System.Collections.Generic;
using System.Linq;

namespace qshine.Configuration
{
    /// <summary>
    /// Connection string element class
    /// </summary>
    public class ConnectionStringElement
    {
        /// <summary>
        /// Connection string setting name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString { get; private set; }
        /// <summary>
        /// Connection string provider
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// Ctor::
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connectionString"></param>
        public ConnectionStringElement(String name, String connectionString)
            : this(name, connectionString, null)
        {
        }

        /// <summary>
        /// Ctor::
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        public ConnectionStringElement(String name, String connectionString, String providerName)
        {
            Name = name;
            ConnectionString = connectionString;
            ProviderName = providerName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected bool Equals(ConnectionStringElement other)
        {
            return String.Equals(Name, other.Name) && String.Equals(ConnectionString, other.ConnectionString) && String.Equals(ProviderName, other.ProviderName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            ConnectionStringElement c = obj as ConnectionStringElement;
            if (c == null) return false;

            return Equals(c);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConnectionString != null ? ConnectionString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ProviderName != null ? ProviderName.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Compare two string elements
        /// </summary>
        /// <param name="left">left connection string element</param>
        /// <param name="right">right connection string element</param>
        /// <returns></returns>
        public static bool operator ==(ConnectionStringElement left, ConnectionStringElement right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ConnectionStringElement left, ConnectionStringElement right)
        {
            return !Equals(left, right);
        }

    }

    /// <summary>
    /// Connection string element collection class
    /// </summary>
    public class ConnectionStrings
    {
        List<ConnectionStringElement> _connectionStrings = new List<ConnectionStringElement>();
        
        /// <summary>
        /// Add or replace connection string element
        /// </summary>
        /// <param name="element"></param>
        public void AddOrUpdate(ConnectionStringElement element)
        {
            ConnectionStringElement anyElement = _connectionStrings.SingleOrDefault(x => x.Name == element.Name);
            if (anyElement!=null)
            {
                _connectionStrings.Remove(anyElement);
            }
            _connectionStrings.Add(element);
        }

        /// <summary>
        /// Check a named connection exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsKey(string name)
        {
            return _connectionStrings.Any(x=>x.Name == name);
        }

        /// <summary>
        /// Remove a named connection element
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(string name)
        {
            var element = this[name];
            if(element!=null)
            {
                return _connectionStrings.Remove(element);
            }
            return false;
        }

        /// <summary>
        /// Get named connection string element.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>a named connection string element.
        /// return null if named connection string element is not found.</returns>
        public ConnectionStringElement this[string name]
        {
            get
            {
                return _connectionStrings.SingleOrDefault(x => x.Name==name);
            }
        }

        /// <summary>
        /// Get connection string element by index order.
        /// </summary>
        /// <param name="index">index order</param>
        /// <returns>a indexed connection string element.
        /// return null if connection string element is not existing.</returns>
        public ConnectionStringElement this[int index]
        {
            get
            {
                return _connectionStrings.ElementAtOrDefault(index); ;
            }
        }

        /// <summary>
        /// Get number of connection string elements 
        /// </summary>
        public int Count
        {
            get
            {
                return _connectionStrings.Count;
            }
        }
    }
}