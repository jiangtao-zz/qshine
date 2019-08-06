using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace qshine.Specification
{
    /// <summary>
    /// The business rule is a statement defines a business specification.
    /// When a business specification is satisfied by the business entity, the action will be executed.
    /// Otherwise the false action will be executed.
    /// 
    /// Example:
    /// <![CDATA[
    ///     var approvalActionRule = new BusinessActionRule("approval rule",
    ///        (spec1 & spec2 & spec3)| (spec4 & spec5), ApproveAction, null);
    ///     
    ///     approvalActionRule.Evaluate(new MyObject());
    /// ]]>
    /// 
    /// </summary>
    public class BusinessActionRule<T>:SpecificationRule<T> 
    {

        #region .ctor
        /// <summary>
        /// Creates a new instance of the <see cref="BusinessActionRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">name of the business rule</param>
        /// <param name="rule">Rule expression</param>
        /// <param name="action">rule action when the rule fullfill</param>
        /// <param name="falseAction">rule action when the rule doesn't follow</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public BusinessActionRule(string ruleName, Expression<Func<T, bool>> rule, Action<T> action, Action<T> falseAction)
            :base(ruleName,rule)
        {
            Initialize(action, falseAction);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="BusinessActionRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The specification of the rule</param>
        /// <param name="action">The rule action applied when the rule fullfill.</param>
        /// <remarks>No action if teh rule condition failed</remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public BusinessActionRule(string ruleName, Expression<Func<T, bool>> rule, Action<T> action)
            : base(ruleName, rule)
        {
            Initialize(action, null);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="BusinessActionRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">name of the business rule</param>
        /// <param name="rule">Rule specification</param>
        /// <param name="action">rule action when the rule fullfill</param>
        /// <param name="falseAction">rule action when the rule doesn't follow</param>
        public BusinessActionRule(string ruleName, ISpecification<T> rule, Action<T> action, Action<T> falseAction)
            : base(ruleName, rule)
        {
            Initialize(action, falseAction);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="BusinessActionRule{T}"/> class.
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The specification of the rule</param>
        /// <param name="action">The action applied ti the rule.</param>
        public BusinessActionRule(string ruleName, ISpecification<T> rule, Action<T> action)
            : base(ruleName, rule)
        {
            Initialize(action, null);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="BusinessActionRule{T}"/> class.
        /// </summary>
        /// <param name="rule">Instance of specification rule</param>
        /// <param name="action">rule action when the rule fullfill</param>
        /// <param name="falseAction">rule action when the rule doesn't follow</param>
        public BusinessActionRule(SpecificationRule<T> rule, Action<T> action, Action<T> falseAction)
            : base(rule.Name, rule)
        {
            Initialize(action, falseAction);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="BusinessActionRule{T}"/> class.
        /// </summary>
        /// <param name="rule">The specification rule</param>
        /// <param name="action">The action applied ti the rule.</param>
        public BusinessActionRule(SpecificationRule<T> rule, Action<T> action)
            : base(rule.Name, rule)
        {
            Initialize(action, null);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Get/Set the rule action for rule evaluation when the specification rule is not satisfied by the entity object.
        /// </summary>
        public Action<T> FalseAction
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the rule action for rule evaluation when the specification rule is satisfied by the entity object.
        /// </summary>
        public Action<T> TrueAction
        {
            get;
            set;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the rule against the business entity instance
        /// </summary>
        /// <param name="candidate">entity instance to be evaluated by the rule</param>
        public void Evaluate(T candidate)
        {
            if (Enabled)
            {
                if (IsSatisfiedBy(candidate))
                {
                    TrueAction?.Invoke(candidate);
                }
                else
                {
                    FalseAction?.Invoke(candidate);
                }
            }
        }
        #endregion

        #region Private
        
        private void Initialize(Action<T> action, Action<T> falseAction)
        {
            TrueAction = action;
            FalseAction = falseAction;
        }

        #endregion
    }

}
