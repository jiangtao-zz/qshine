using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace qshine.Specification
{
    /// <summary>
    /// Specification Rule of Entity used by validation rule and business rule.
    /// </summary>
    /// <typeparam name="T">The entity to be evaluated</typeparam>
    /// <example>
    /// <![CDATA[
    /// 
    ///     public class MyOfficeAreaRule:SpecificationRule<Custom>
    ///     {
    ///         public MyOfficeAreaRule()
    ///         {
    ///             Name ="MyOffliceAreaRule";
    ///             Priority = 10;
    ///             ConditionExpression = c=>c.PostCode.Contains("L3M");
    ///             AddTags("Test","Demo");
    ///         }
    ///     }
    /// 
    /// ]]>
    /// 
    /// </example>
    public class SpecificationRule<T>:Specification<T>
    {
        #region fields
        private Collection<string> _tags;
        const int NORMAL_PRIORITY = 0;
        #endregion

        #region .ctor

        /// <summary>
        /// Ctro. with default rule name.
        /// </summary>
        /// <remarks>The default constructor is useful when override the Expression() by the concrete class</remarks>
        public SpecificationRule()
            :this("AnonymousRule", null, NORMAL_PRIORITY)
        {
        }

        /// <summary>
        /// Ctro a named rule using expression
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The rule specification</param>
        /// <param name="priority">The rule priority. The higher rule priority get executed first from rule set.</param>
        public SpecificationRule(string ruleName, Expression<Func<T, bool>> rule, int priority)
            :base(rule)
        {
            Name = ruleName;
            Priority = priority;
            Enabled = true;
            _tags = new Collection<string>();
        }

        /// <summary>
        /// Initialize a named rule using specification
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The rule specification</param>
        public SpecificationRule(string ruleName, Expression<Func<T, bool>> rule)
            : this(ruleName, rule, NORMAL_PRIORITY)
        {

        }

        /// <summary>
        /// Initialize a named rule using specification
        /// </summary>
        /// <param name="ruleName">The name of the rule</param>
        /// <param name="rule">The rule specification</param>
        public SpecificationRule(string ruleName, ISpecification<T> rule)
            : this(ruleName, rule.ConditionExpression, NORMAL_PRIORITY)
        {

        }

        /// <summary>
        /// Initialize a rule
        /// </summary>
        /// <param name="rule">The rule specification</param>
        public SpecificationRule(Expression<Func<T, bool>> rule)
            : this(GetAutoRuleName(rule), rule, NORMAL_PRIORITY)
        {
        }

        private static string GetAutoRuleName(object rule)
        {
            return "R" + rule.GetHashCode();
        }

        #endregion

        #region properties

        /// <summary>
        /// The specification rule name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Rule priority. The higher rule priority get executed first from the rule set.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Sets/Gets specification rule active inactive flag
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets tags associated with rule.
        /// A tag is an arbitrary value assicated with the rule that can be used to categorize or filter the business rules.
        /// </summary>
        public Collection<string> Tags {
            get
            {
                return _tags;
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Add tags
        /// </summary>
        /// <param name="ruleTags">Tags to be attached</param>
        /// <returns>this instance</returns>
        public SpecificationRule<T> AddTags (params string[] ruleTags)
        {
            foreach (var tag in ruleTags)
            {
                if (!Tags.Contains(tag))
                {
                    Tags.Add(tag);
                }
            }
            return this;
        }

        /// <summary>
        /// Check the rule whether it contains required tag.
        /// </summary>
        /// <param name="tagsArray">Tag array to be searching for</param>
        /// <returns>Whether the specific tags associated with the rule.</returns>
        public bool HasTag(params string[] tagsArray)
        {
            if (!Tags.Any() || !tagsArray.Any())
            {
                return false;
            }

            foreach (var s in tagsArray)
            {
                if (Tags.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
