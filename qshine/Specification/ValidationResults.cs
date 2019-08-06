using System.Collections.ObjectModel;
using System.Linq;

namespace qshine.Specification
{
    /// <summary>
    /// Represents the results of validating an object or process.
    /// </summary>
    public class ValidationResults : Collection<ValidationResult>
    {
        #region properties

        /// <summary>
        /// Gets the indication of whether the validation successful or failure
        /// </summary>
        public bool IsValid
        {
            get {
                return Count==0 ||
                    this.Any(x => !x.Success);
            }
        }
        /// <summary>
        /// Gets/Sets the object to validate
        /// </summary>
        public object ObjectInstance {get;set;}

        #endregion

        /// <summary>
        /// Throw Validation exception
        /// </summary>
        public void ThrowExceptionOnError()
        {
            if (this.Any(x => !x.Success))
            {
                var exception = new BatchException();
                foreach (var error in this)
                {
                    if (!error.Success)
                    {
                        exception.InnerExceptions.Add(error.Error);
                    }
                }

                throw exception;
            }
        }


    }


}
