using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using qshine.Globalization;

namespace qshine.Specification
{
    /// <summary>
    /// A helper class for entity validation.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <remarks>
    /// The entity validator is used to validate an entity rules.
    /// The entity validator contains all specifications of an entity.
    /// Use rule tags to categorize the different rule set.
    /// 
    /// The service create a business entity validator and apply the the entity object to the validator before perform the action.
    /// The validator may raise exception or returns ValidationResults based on validator policy.
    /// Default validator will raise exception on last failed rule.
    /// </remarks>
    /// <example>
    /// For example, 
    ///     When perform Account entity method ChangeAddress() we need apply AccountValidator to the entity.
    /// <![CDATA[
    /// 
    ///  public class AccountValidator():Validator<Account>
    ///  {
    ///     public AccountValidator()
    ///     {
    ///     
    ///         Policy = new ValidationPolicy {
    ///             ErrorPolicy = ErrorPolicy.ThrowExceptionOnFirstFailure,
    ///             EnableDataAnnotation = true
    ///         }();
    ///         
    ///     AddRule ("Base Rules", Rule1 & Rule2 & Rule3);
    ///     AddRule ("Address Rules", Rule4);
    ///  }
    ///...
    /// void ChangeAddress(Account account)
    /// {
    ///     var validator = new AccountValidator();
    ///     var repository = new AccountRepository();
    ///
    ///     validator.Validate(account, repository.Save);
    ///     
    /// }
    /// ]]>
    /// </example>
    public class Validator<T> : Validator
        where T : class
    {
        #region fields
        private readonly RuleCollection<BusinessValidationRule<T>> _ruleSet = new RuleCollection<BusinessValidationRule<T>>();
        private int anonymousRuleIndex;
        #endregion

        #region Get RuleSet

        /// <summary>
        /// Get a collection of validation rules
        /// </summary>
        public RuleCollection<BusinessValidationRule<T>> RuleSet
        {
            get { return _ruleSet; }
        }

        #endregion

        #region AddRule
        /// <summary>
        /// Add validation rule by expression
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="rule"></param>
        /// <param name="message"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public Validator<T> AddRule(string ruleName, Expression<Func<T, bool>> rule, string message, string property)
        {
            return AddRule(new BusinessValidationRule<T>(ruleName, rule, message, property));
        }

        /// <summary>
        /// Add anoymous rule
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="message"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public Validator<T> AddRule(Expression<Func<T, bool>> rule, string message, string property)
        {
            return AddRule(new BusinessValidationRule<T>(GetAnonymouseName(), rule, message, property));
        }

        private string GetAnonymouseName()
        {
            return string.Format("R{0}", ++anonymousRuleIndex);
        }

        /// <summary>
        /// Add anonymous rule
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Validator<T> AddRule(Expression<Func<T, bool>> rule, string message)
        {
            return AddRule(new BusinessValidationRule<T>(GetAnonymouseName(), rule, message, rule.Body.ToString()));
        }

        /// <summary>
        /// Add a validation rule for specific entity
        /// </summary>
        /// <param name="validationRule">validation rule</param>
        /// <returns>return instance self</returns>
        public Validator<T> AddRule(BusinessValidationRule<T> validationRule)
        {
            RuleSet.Add(validationRule);
            return this;
        }

        /// <summary>
        /// Add a validator in
        /// </summary>
        /// <param name="validator">validator</param>
        /// <returns>return instance self</returns>
        public Validator<T> AddRule(Validator<T> validator)
        {
            for (var i = 0; i < validator.RuleSet.Count; i++)
            {
                AddRule(RuleSet[i]);
            }
            return this;
        }
        #endregion

        #region AddRules

        /// <summary>
        /// Add multiple validation rules for specific entity
        /// </summary>
        /// <param name="rules">ValidationRules to be added into validator</param>
        /// <returns>return instance self</returns>
        public Validator<T> AddRules(params BusinessValidationRule<T>[] rules)
        {
            foreach (var t in rules)
            {
                AddRule(t);
            }
            return this;
        }
        #endregion

        #region RemoveRule
        /// <summary>
        /// Remove one validation rule from the validator
        /// </summary>
        /// <param name="rule">The validation rule to be removed</param>
        public Validator<T> RemoveRule(BusinessValidationRule<T> rule)
        {
            RuleSet.Remove(rule);
            return this;
        }
        #endregion

        #region RemoveRules

        /// <summary>
        /// Remove validation rules by a specific rule name
        /// </summary>
        /// <param name="ruleName">The name of the validation rule</param>
        public Validator<T> RemoveRules(string ruleName = null)
        {
            RuleSet.RemoveAll(x => string.IsNullOrEmpty(ruleName) || x.Name == ruleName);
            return this;
        }
        #endregion

        #region Validate
        /// <summary>
        /// Perform validation against an entity instance.
        /// On ExceptionOnValidationError.ExceptionOnFirstError and  ExceptionOnValidationError.ExceptionOnLastError
        /// it throw a validation exception when the validation failure.
        /// </summary>
        /// <param name="entity">Entity instance to be validated</param>
        /// <returns>validation results</returns>
        public virtual ValidationResults Validate(T entity)
        {
            return Validate(entity, RuleSet.Where(x => x.Enabled).OrderByDescending(x => x.Priority));
        }

        /// <summary>
        /// Perform validation rules against an entity instance.Only the given tagged rules will be evaluated.
        /// </summary>
        /// <param name="entity">Entity instance to be validated</param>
        /// <param name="tags">Expected tagged rules</param>
        /// <returns>validation results</returns>
        public ValidationResults Validate(T entity, params string[] tags)
        {
            if (tags.Length == 0)
                return Validate(entity);

            return Validate(entity, RuleSet.Where(
                x => x.Enabled && x.HasTag(tags)
                ).OrderByDescending(x => x.Priority));
        }
        #endregion

        #region private

        /// <summary>
        /// perform all entity rules validation
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="activeRules"></param>
        /// <returns></returns>
        private ValidationResults Validate(T entity, IOrderedEnumerable<BusinessValidationRule<T>> activeRules)
        {
            //Clear validation result queue
            ValidationResults.Clear();

            //Set entity object
            ValidationResults.ObjectInstance = entity;

            //Validate all valid rules against the entity
            foreach (var rule in activeRules)
            {
                var validationResult = rule.Validate(entity);

                if (validationResult!=null && !validationResult.Success)
                {
                    if (!AddValidationResult(validationResult))
                    {
                        break;
                    }
                }
            }

            
            if (ValidationErrorPolicy.CatchExceptionOnFirstFailure == Policy.ErrorPolicy && !ValidationResults.IsValid)
            {
                return ValidationResults;
            }

            if (Policy.EnableDataAnnotations)
            {
                ValidateDataAnnotations(entity);
            }

            if (!ValidationResults.IsValid && (
                Policy.ErrorPolicy == ValidationErrorPolicy.ThrowExceptionOnLastFailure
                || Policy.ErrorPolicy == ValidationErrorPolicy.ThrowExceptionOnFirstFailure))
            {
                ValidationResults.ThrowExceptionOnError();
            }

            return ValidationResults;
        }

        /// <summary>
        /// Validate using data annotation
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private void ValidateDataAnnotations(object entity)
        {
            var ctx = new ValidationContext(entity, null, null);
            var annotationErrors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entity, ctx, annotationErrors, true);

            if (!isValid)
            {
                foreach (var error in annotationErrors)
                {
                    var validationResult = new ValidationResult(error.ErrorMessage, GetAnonymouseName(), string.Concat(error.MemberNames));

                    if (AddValidationResult(validationResult) == false) { 
                        return;
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// A validator validates a object using business rules and validation annotation based on validation policy.
    /// Each validation rule result will be added in result queue.
    /// </summary>
    public class Validator
    {
        private readonly ValidationResults _validationResults;
        private readonly ValidationPolicy _policy;


        /// <summary>
        /// Ctor::
        /// </summary>
        public Validator()
            : this(ValidationErrorPolicy.CatchAllExceptions)
        {
        }

        /// <summary>
        /// Create a validator by given error policy 
        /// </summary>
        /// <param name="errorPolicy"></param>
        public Validator(ValidationErrorPolicy errorPolicy)
            :this (new ValidationPolicy { ErrorPolicy = errorPolicy})
        {
        }

        /// <summary>
        /// Ctor::
        /// </summary>
        public Validator(ValidationPolicy policy)
        {
            _policy = policy;
            _validationResults = new ValidationResults();
        }

        /// <summary>
        /// Gets validation results which hold a collection of failed validation results.
        /// </summary>
        public ValidationResults ValidationResults
        {
            get { return _validationResults; }
        }

        /// <summary>
        /// Get validation policy
        /// </summary>
        public ValidationPolicy Policy { get { return _policy; } }

        /// <summary>
        /// Add a validation result into the error result collection.
        /// It returns false if maximum number of errors reached.
        /// </summary>
        /// <param name="validationResult"></param>
        private bool TryAddValidationResult(ValidationResult validationResult)
        {
            if (ValidationResults.Count < Policy.MaxErrorLimit)
            {
                ValidationResults.Add(validationResult);
                if (ValidationResults.Count == Policy.MaxErrorLimit)
                {
                    ValidationResults.Add(
                        new ValidationResult("Maximum number of errors reached."._G())
                        );
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Add a validation error into validation result collection.
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <returns>Returns true if the error message can be added into collection.
        /// Returns false if the error reach to maximum number of errors.</returns>
        public bool AddValidationError(string errorMessage)
        {
            var validationResult = new ValidationResult(errorMessage);

            return AddValidationResult(validationResult);
        }

        /// <summary>
        /// Add validation exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool AddValidationError(Exception ex)
        {
            var validationResult = new ValidationResult(ex);

            return AddValidationResult(validationResult);
        }

        /// <summary>
        /// Add a validation result into validation result collection.
        /// </summary>
        /// <param name="validationResult">validation result</param>
        /// <returns>return false when error number reached to teh limitation.</returns>
        public bool AddValidationResult(ValidationResult validationResult)
        { 
            var reachMaxLimt = !TryAddValidationResult(validationResult);

            switch (Policy.ErrorPolicy)
            {
                case ValidationErrorPolicy.ThrowExceptionOnLastFailure:
                    if (reachMaxLimt)
                    {
                        ValidationResults.ThrowExceptionOnError();
                    }
                    break;

                case ValidationErrorPolicy.ThrowExceptionOnFirstFailure:
                    ValidationResults.ThrowExceptionOnError();
                    break;

                case ValidationErrorPolicy.CatchAllExceptions:
                    if (reachMaxLimt)
                    {
                        return false;
                    }
                    break;

                case ValidationErrorPolicy.CatchExceptionOnFirstFailure:
                    return false;
            }
            return true;
        }
    }
}
