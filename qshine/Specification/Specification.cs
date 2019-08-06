using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace qshine.Specification
{
    /// <summary>
    /// Provides an implementation for common specification and composite specification.
    /// </summary>
    /// <typeparam name="T">Type of object to be validated</typeparam>
    public class Specification<T> : ISpecification<T>
    {
        #region Fields
        private Expression<Func<T, bool>> _innerExpression;
        private Func<T, bool> _compiledExpression;
        #endregion

        #region ctor
        /// <summary>
        /// Default constractor for the concrete class that overrides the specification condition expression
        /// </summary>
        public Specification() {}

        /// <summary>
        /// Constructor with a validation condition expression
        /// </summary>
        /// <param name="conditionExpression">validation condition expression against a specific class.</param>
        public Specification(Expression<Func<T, bool>> conditionExpression)
        {
            _innerExpression = conditionExpression;
        }
        #endregion

        #region properties
        /// <summary>
        /// Get/Set specification condition expression
        /// </summary>
        public Expression<Func<T, bool>> ConditionExpression
        {
            get
            {
                return _innerExpression;
            }
            set
            {
                _innerExpression = value;
                _compiledExpression = null;
            }
        }

        #endregion

        #region methods
        /// <summary>
        /// Test against a candidate object based on speification.
        /// </summary>
        /// <param name="entity">The object to be tested</param>
        /// <returns>Returns true if the object is satisfied by the specification</returns>
        /// <remarks>
        /// The specification will compile conditional expression and evaulate the expression.
        /// </remarks>
        public virtual bool IsSatisfiedBy(T entity)
        {
            if (_compiledExpression == null && ConditionExpression!=null)
            {
                _compiledExpression = _innerExpression.Compile();
            }

            return _compiledExpression.Invoke(entity);
        }
        #endregion

        #region Combined Specification

        /// <summary>
        /// Combine current specification with other specification togather in boolean AND logic
        /// </summary>
        /// <param name="other">Other specification to be combined</param>
        /// <returns>A new specifiction which combines with other specification.</returns>
        public ISpecification<T> And(ISpecification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }

        /// <summary>
        /// Combine current specification with other specification togather in boolean AND NOT logic
        /// </summary>
        /// <param name="other">Other specification to be combined AND NOT logic</param>
        /// <returns>A new specifiction which combines with other specification.</returns>
        public ISpecification<T> AndNot(ISpecification<T> other)
        {
            return new AndNotSpecification<T>(this, other);
        }

        /// <summary>
        /// Combine current specification with other specification togather in boolean OR logic
        /// </summary>
        /// <param name="other">Other specification to be combined</param>
        /// <returns>A new specifiction which combines with other specification.</returns>
        public ISpecification<T> Or(ISpecification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }

        /// <summary>
        /// Combine current specification with other specification togather in boolean OR NOT logic
        /// </summary>
        /// <param name="other">Other specification to be combined OR NOT logic</param>
        /// <returns>A new specifiction which combines with other specification.</returns>
        public ISpecification<T> OrNot(ISpecification<T> other)
        {
            return new OrNotSpecification<T>(this, other);
        }

        /// <summary>
        /// Get opposite specification
        /// </summary>
        /// <returns>A new specifiction which its opposite logic.</returns>
        public ISpecification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
        #endregion

        #region Operator overloads
        /// <summary>
        /// overload operator &amp; to combine two specification togather in a boolean AND logic.
        /// </summary>
        /// <param name="left">The left hand specification to be combined</param>
        /// <param name="right">The right hand specification to be combined</param>
        /// <returns>A new specification combined both left and right hand specification.</returns>
        public static Specification<T> operator &(Specification<T> left, Specification<T> right)
        {
            return new AndSpecification<T>(left, right);
        }
        /// <summary>
        /// overload operator | to combine two specification togather in a boolean OR logic.
        /// </summary>
        /// <param name="left">The left hand specification to be combined</param>
        /// <param name="right">The right hand specification to be combined</param>
        /// <returns>A new specification combined both left and right hand specification.</returns>
        public static Specification<T> operator |(Specification<T> left, Specification<T> right)
        {
            return new OrSpecification<T>(left, right);
        }
        /// <summary>
        /// overload operator ! to change the specification to its opposite logic.
        /// </summary>
        /// <param name="current">The specification to be change</param>
        /// <returns>A new specification with opposite logic</returns>
        public static Specification<T> operator !(Specification<T> current)
        {
            return new NotSpecification<T>(current);
        }
        #endregion
    }

    #region private class combined specification
    /// <summary>
    /// Implement the combined Composite Specification Pattern with AND expression
    /// </summary>
    /// <typeparam name="T">Type of object to against the specification</typeparam>
    class AndSpecification<T> : Specification<T>
    {
        #region fields
        private readonly ISpecification<T> leftHand;
        private readonly ISpecification<T> rightHand;
        #endregion

        #region ctor
        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            leftHand = left;
            rightHand = right;
        }
        #endregion

        #region methods
        public override bool IsSatisfiedBy(T entity)
        {
            return leftHand.IsSatisfiedBy(entity) && rightHand.IsSatisfiedBy(entity);
        }
        #endregion
    }

    /// <summary>
    /// Implement the combined Composite Specification Pattern with AND NOT expression
    /// </summary>
    /// <typeparam name="T">Type of object to against the specification</typeparam>
    class AndNotSpecification<T> : Specification<T>
    {
        ISpecification<T> left;
        ISpecification<T> right;

        public AndNotSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            this.left = left;
            this.right = right;
        }

        public override bool IsSatisfiedBy(T candidate)
        {
            return left.IsSatisfiedBy(candidate) && right.IsSatisfiedBy(candidate) != true;
        }
    }


    /// <summary>
    /// Implement Composite Specification Pattern OR specification
    /// </summary>
    /// <typeparam name="T">Type of object to against the specification</typeparam>
    class OrSpecification<T> : Specification<T>
    {
        #region fields
        private readonly ISpecification<T> leftHand;
        private readonly ISpecification<T> rightHand;
        #endregion

        #region ctor
        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            leftHand = left;
            rightHand = right;
        }
        #endregion

        #region methods
        public override bool IsSatisfiedBy(T entity)
        {
            return leftHand.IsSatisfiedBy(entity) || rightHand.IsSatisfiedBy(entity);
        }
        #endregion
    }

    /// <summary>
    /// Implement Composite Specification Pattern OR NOT specification
    /// </summary>
    /// <typeparam name="T">Type of object to against the specification</typeparam>
    class OrNotSpecification<T> : Specification<T>
    {
        ISpecification<T> left;
        ISpecification<T> right;

        public OrNotSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            this.left = left;
            this.right = right;
        }

        public override bool IsSatisfiedBy(T candidate)
        {
            return left.IsSatisfiedBy(candidate) || right.IsSatisfiedBy(candidate) != true;
        }
    }

    /// <summary>
    /// Implement Composite Specification Pattern NOT specification
    /// </summary>
    /// <typeparam name="T">Type of object to against the specification</typeparam>
    class NotSpecification<T> : Specification<T>
    {
        #region fields
        private readonly ISpecification<T> one;
        #endregion

        #region ctor
        public NotSpecification(ISpecification<T> one)
        {
            this.one = one;
        }
        #endregion

        #region methods
        public override bool IsSatisfiedBy(T entity)
        {
            return !one.IsSatisfiedBy(entity);
        }
        #endregion
    }
    #endregion
}
