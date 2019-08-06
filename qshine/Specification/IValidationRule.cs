namespace qshine.Specification
{
    /// <summary>
    /// RealSuite validation rule interface
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Test whether a given value satisfis the rule.
        /// </summary>
        /// <param name="value">A value to be evaludated</param>
        /// <returns>It returns true if the value is satisfied by the rule.
        ///</returns>
       bool IsSatisfiedBy(object value);
        /// <summary>
        /// Get the target object property name the rule refer to.
        /// </summary>
       string TargetPropertyName { get;}

        /// <summary>
        /// the lower the rank, the earlier to validate
        /// </summary>
        int Rank { get; set; }

    }
}
