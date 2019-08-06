using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace qshine.Specification
{
    /// <summary>
    /// Represents a specification based validation rule.
    /// Example:
    ///     Defines all entity rules in validator and apply selected rules to corresponding action.
    /// <![CDATA[
    ///     public class AccountValidator:Validator<Account>
    ///     {
    ///         BusinessValidationRule[] = {
    ///             {
    ///             new BusinessValidationRule(
    ///                 "rule1", x=>x.Name.Contains("Special"),
    ///                 (x)=>{return string.Format("Validation rule 1 failed {0}",x.MyProperty);},
    ///                 1
    ///                 )
    ///             },
    ///             {}
    ///         };
    ///     }
    /// 
    /// ]]>    
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BusinessValidationRule<T>:SpecificationRule<T>,IValidationRule
        where T:class
    {
        #region fields
        private readonly Func<T, string> _errorMessageDelegator;
        private readonly string _targetProperty;
        const int DEFAULT_RANK = 100;
        #endregion

        #region .ctor
        /// <summary>
        /// Initialize a business validation rule instance of the <see cref="BusinessValidationRule{T}"/> class.
        /// </summary>
        /// <param name="rule">Specifies rule name.</param>
        /// <param name="validationMessage">A delegator to generate a validation error message.</param>
        /// <param name="rank">Rank of the rule.</param>
        /// <param name="property">validation rule asociated property.</param>
        public BusinessValidationRule(SpecificationRule<T> rule, Func<T, string> validationMessage, int rank, string property)
            : base(rule.Name, rule)
        {
            _errorMessageDelegator = validationMessage;
            _rank = rank;
            _targetProperty = property;
        }

        /// <summary>
        /// Initialize a business validation rule instance of the <see cref="BusinessValidationRule{T}"/> class.
        /// </summary>
        /// <param name="rule">Specifies rule name.</param>
        /// <param name="validationMessage">A delegator to generate a validation error message.</param>
        /// <param name="rank">Rank of the rule.</param>
        public BusinessValidationRule(SpecificationRule<T> rule, Func<T, string> validationMessage, int rank)
            : this(rule, validationMessage, rank, "")
        {
        }

        /// <summary>
        /// Initialize a business validation rule instance of the <see cref="BusinessValidationRule{T}"/> class.
        /// </summary>
        /// <param name="rule">Specifies rule name.</param>
        /// <param name="validationMessage">A delegator to generate a validation error message.</param>
        public BusinessValidationRule(SpecificationRule<T> rule, Func<T, string> validationMessage)
            : this(rule, validationMessage, DEFAULT_RANK)
        {
        }

        /// <summary>
        /// Defines a new business validation rule instance of the <see cref="BusinessValidationRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The specification of the rule using Lambda expression</param>
        /// <param name="validationMessage">A delegator to generate a validation error message.</param>
        /// <param name="rank">Rank of the rule.</param>
        /// <param name="property">validation rule asociated property.</param>
        public BusinessValidationRule(string ruleName, Expression<Func<T, bool>> rule, Func<T, string> validationMessage, int rank, string property)
            : this(new SpecificationRule<T>(ruleName, rule), validationMessage, rank, property)
        {
        }


        /// <summary>
        /// Defines a new business validation rule instance of the <see cref="BusinessValidationRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The specification of the rule using Lambda expression</param>
        /// <param name="validationMessage">A delegator to generate a validation error message.</param>
        /// <param name="rank">Rank of the rule.</param>
        public BusinessValidationRule(string ruleName, Expression<Func<T, bool>> rule, Func<T, string> validationMessage, int rank)
            : this(new SpecificationRule<T>(ruleName, rule), validationMessage, rank)
        {
        }

        /// <summary>
        /// Defines a new business validation rule instance of the <see cref="BusinessValidationRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The specification of the rule using Lambda expression</param>
        /// <param name="validationMessage">A delegator to generate a validation error message.</param>
        public BusinessValidationRule(string ruleName, Expression<Func<T, bool>> rule, Func<T, string> validationMessage)
            : this(new SpecificationRule<T>(ruleName, rule), validationMessage)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="rule"></param>
        /// <param name="validationMessage">A delegator to generate a validation error message.</param>
        /// <param name="property"></param>
        public BusinessValidationRule(string ruleName, Expression<Func<T, bool>> rule, Func<T, string> validationMessage, string property)
            : this(new SpecificationRule<T>(ruleName, rule), validationMessage, DEFAULT_RANK, property)
        {
        }

        /// <summary>
        /// Defines a new business validation rule instance of the <see cref="BusinessValidationRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The specification of the rule using Lambda expression</param>
        /// <param name="validationErrorMessage">The validation message associated with the rule.</param>
        /// <param name="property">The name of the validation target object.</param>
        public BusinessValidationRule(string ruleName, Expression<Func<T, bool>> rule, string validationErrorMessage, string property)
            : this(new SpecificationRule<T>(ruleName, rule),(x) => { return validationErrorMessage; }, DEFAULT_RANK, property)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BusinessValidationRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The specification of the rule</param>
        /// <param name="validationErrorMessage">The validation message associated with the rule.</param>
        /// <param name="property">The name of the candidate.</param>
        public BusinessValidationRule(string ruleName, ISpecification<T> rule, string validationErrorMessage, string property)
            : this(new SpecificationRule<T>(ruleName, rule), (x) => { return validationErrorMessage; }, DEFAULT_RANK, property)
        {
        }


        #endregion

        #region Properties
        /// <summary>
        /// The property associate to the rule. It usually is an entity property name, 
        /// but it could be any meaningful value that user could identify which rule is applied. 
        /// </summary>
        public string TargetPropertyName
        {
            get { return _targetProperty; }
        }


        #endregion

        #region Methods
        /// <summary>
        /// Validates a candidate based on rule
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        public ValidationResult Validate(T candidate)
        {
            bool isValid = IsSatisfiedBy(candidate);
            if (!isValid)
            {
                return new ValidationResult(_errorMessageDelegator(candidate));
            }

            return new ValidationResult();
        }

        /// <summary>
        /// Implement the IsSatisfiedBy interface
        /// </summary>
        /// <param name="candidate">candidate to be evaluated</param>
        /// <returns>return of the validation</returns>
        public bool IsSatisfiedBy(object candidate)
        {
            if (candidate is T)
            {
                return base.IsSatisfiedBy(candidate as T);
            }
            return false;
        }

        #endregion

        private int _rank;

        /// <summary>
        /// 
        /// </summary>
        public int Rank
        {
            get { return _rank; }
            set { _rank = value; }
        }
    }
}
