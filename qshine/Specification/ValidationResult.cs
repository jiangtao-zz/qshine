using System;

namespace qshine.Specification
{
    /// <summary>
    /// Represents the result of an atomic validation rule.
    /// </summary>
    public class ValidationResult
    {
        #region fields
        private readonly Exception _error;
        #endregion

        /// <summary>
        /// Initializes a new instance of the ValidationResult class by using a rule name, error message and associated proeprty name
        /// </summary>
        /// <param name="source">source of the validation error message</param>
        /// <param name="message">error message</param>
        /// <param name="property">Error associated property</param>
        public ValidationResult(string message, string source, object property)
        {
            if (!string.IsNullOrEmpty(message))
            {
                _error = new Exception(message);
                _error.Source = source;
                _error.Data.Add("property", property);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ValidationResult class by error message
        /// </summary>
        /// <param name="message">validation error message</param>
        public ValidationResult(string message)
            :this(message,"",null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ValidationResult class by error exception
        /// </summary>
        /// <param name="ex">Error exception</param>
        public ValidationResult(Exception ex)
        {
            _error = ex;
        }

        /// <summary>
        /// Initializes a new instance of the ValidationResult class without error.
        /// </summary>
        public ValidationResult()
        {
        }

        /// <summary>
        /// Gets the indication of whether the validation success or failure
        /// </summary>
        public bool Success { get { return _error == null; } }
        
        /// <summary>
        /// Gets a message describing the failure.
        /// </summary>
        public Exception Error { get { return _error; } }
    }
}
