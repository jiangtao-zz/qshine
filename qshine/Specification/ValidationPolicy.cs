using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Specification
{
    /// <summary>
    /// Specification validation policy.
    /// 
    /// </summary>
    public class ValidationPolicy
    {
        /// <summary>
        /// Defines validation policy how to handle validation failure.
        /// </summary>
        public ValidationErrorPolicy ErrorPolicy { get; set; }

        /// <summary>
        /// Enable/disable entity data annotation
        /// </summary>
        public bool EnableDataAnnotations { get; set; }

        /// <summary>
        /// Enable/disable child entity data annotation
        /// </summary>
        public bool EnableChildDataAnnotations { get; set; }

        int _maxErrorLimt = 100;
        /// <summary>
        /// Specified a maximum error results the ValidationResults can hold.
        /// The default number is 100
        /// The 0 value indicates unlimited.
        /// 
        /// </summary>
        public int MaxErrorLimit { get { return _maxErrorLimt; } set { _maxErrorLimt = value; } }

    }

    /// <summary>
    /// Validation failure policy
    /// The policy guide how to handle validation error.
    /// </summary>
    public enum ValidationErrorPolicy
    { 
        /// <summary>
        /// Catch all validation errors without throw exception.
        /// </summary>
        CatchAllExceptions = 0,
        /// <summary>
        /// Catch first validation error without throw exception.
        /// The validation rules stop running if one of the rule is not satisfied.
        /// </summary>
        CatchExceptionOnFirstFailure = 1,
        /// <summary>
        /// A ValidationException will be thrown if one of the rule is not satisfied.
        /// </summary>
        ThrowExceptionOnFirstFailure = 2,
        /// <summary>
        /// The validation rules continue to run if one of the rule is not satisfied. 
        /// A ValidationException will be thrown in the end of the rule validation if one of the rule is not satisfied.
        /// </summary>
        ThrowExceptionOnLastFailure = 3
    }
}
