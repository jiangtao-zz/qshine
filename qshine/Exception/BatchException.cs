using qshine.Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace qshine
{
    /// <summary>
    /// Defines application multiple validation exception and exception policy object.
    /// The exception policy is used to indicate how to handle application exception in batch process.
    /// It could raise or supress exception based on exception policy.
    /// Common Exception policy:
    /// 
    /// BatchException.SkipException - it returns BatchException object which can hold multiple errors without throw exception.
    /// BatchException.FirstException - it returns BatchException object which will throw in first application process exception.
    /// BatchException.LastException - it returns BatchException object which will throw in end of process if any error capture with in the process.
    /// </summary>
    public class BatchException : ValidationException
    {
        int _chainCounter = 0;

        #region Ctro.
        /// <summary>
        /// Ctro.
        /// </summary>
        public BatchException()
            :this("Batch Process Exception"._G())
        {}
        /// <summary>
        /// Ctro.
        /// </summary>
        /// <param name="message"></param>
        public BatchException(string message)
            : base(message)
        {
            InnerExceptions = new List<Exception>();
        }
        #endregion
        
        #region public properties

        /// <summary>
        /// Keep batch exceptions.
        /// </summary>
        public List<Exception> InnerExceptions
        {
            get; set;
        }

        #endregion

        #region public methods
        /// <summary>
        /// Chain batch exception for multi-level batch call
        /// </summary>
        public void ChainBatchException()
        {
            _chainCounter++;
        }

        /// <summary>
        /// Add exception into batch exception.
        /// </summary>
        /// <param name="ex"></param>
        public void AddException(Exception ex)
        {
            InnerExceptions.Add(ex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        public void TryThrow(Exception ex = null) { }

        /// <summary>
        /// 
        /// </summary>
        public List<Exception> Exceptions;


        #endregion
    }
}
