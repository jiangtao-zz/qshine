namespace qshine.Specification
{
    /// <summary>
    /// Create a domain validation interface
    /// </summary>
    public interface IDomainValidator
    {
        /// <summary>
        /// Validate domain entity
        /// </summary>
        /// <returns>Validation result.</returns>
        ValidationResults Validate();
    }
}
