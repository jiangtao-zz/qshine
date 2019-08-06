using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace qshine.Specification
{
    /// <summary>
    /// It provides a contract to implement a re-usable business logic satisfaction based on certain business conditions.
    /// It is a common <see href="http://en.wikipedia.org/wiki/Specification_pattern">Specification Pattern</see> used to define domain validation and business rules.
    /// </summary>
    /// <typeparam name="T">Type of object to be validated. It usually is a </typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Validates against a candidate object to satisfy the specification.
        /// </summary>
        bool IsSatisfiedBy(T entity);

        /// <summary>
        /// Combine current specification with other specification togather in boolean AND logic
        /// </summary>
        /// <param name="other">Other specification to be combined</param>
        /// <returns>A new specifiction which combines with other specification.</returns>
        ISpecification<T> And(ISpecification<T> other);

        /// <summary>
        /// Combine current specification with other specification togather in boolean AND NOT logic
        /// </summary>
        /// <param name="other">Other specification to be combined AND NOT logic</param>
        /// <returns>A new specifiction which combines with other specification.</returns>
        ISpecification<T> AndNot(ISpecification<T> other);

        /// <summary>
        /// Combine current specification with other specification togather in boolean OR logic
        /// </summary>
        /// <param name="other">Other specification to be combined</param>
        /// <returns>A new specifiction which combines with other specification.</returns>
        ISpecification<T> Or(ISpecification<T> other);

        /// <summary>
        /// Combine current specification with other specification togather in boolean OR NOT logic
        /// </summary>
        /// <param name="other">Other specification to be combined OR NOT logic</param>
        /// <returns>A new specifiction which combines with other specification.</returns>
        ISpecification<T> OrNot(ISpecification<T> other);

        /// <summary>
        /// Get opposite specification
        /// </summary>
        /// <returns>A new specifiction which its opposite logic.</returns>
        ISpecification<T> Not();

        /// <summary>
        /// Gets the specification condition expression.
        /// </summary>
        Expression<Func<T, bool>> ConditionExpression { get; }
    }
}
