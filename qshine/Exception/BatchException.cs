using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Defines application multiple error exception and exception policy object.
    /// The exception policy is used to indicate how to handle application exception in batch process.
    /// It could raise or supress rasied exception based on exception policy.
    /// Common Exception policy:
    /// 
    /// BatchException.SkipException - it returns BatchException object which can hold multiple errors without throw exception.
    /// BatchException.FirstException - it returns BatchException object which will throw in first application process exception.
    /// BatchException.LastException - it returns BatchException object which will throw in end of process if any error capture with in the process.
    /// </summary>
    public class BatchException : Exception
    {
        int _chainCounter = 0;
        #region Ctors
        public BatchException()
            :this("Batch Process Exception")
        {}

        public BatchException(string message)
            : base(message)
        {
            Exceptions = new List<Exception>();
        }
        #endregion
        
        #region public static shotcut
        /// <summary>
        /// it returns BatchException object which can hold multiple errors without throw exception.
        /// </summary>
        static public BatchException SkipException
        {
            get
            {
                return new BatchException
                {
                    NeedSkipException = true
                };
            }
        }

        /// <summary>
        /// it returns BatchException object which will throw in first application process exception.
        /// </summary>
        static public BatchException FirstException
        {
            get
            {
                return new BatchException
                {
                    NeedSkipException = false,
                    ExceptionLimit = 1
                };
            }
        }

        /// <summary>
        /// it returns BatchException object which will throw in end of process if any error capture with in the process.
        /// </summary>
        static public BatchException LastException
        {
            get
            {
                return new BatchException
                {
                    NeedSkipException = false,
                    ExceptionLimit = 0
                };
            }
        }
        #endregion

        #region public properties

        /// <summary>
        /// Indicates whether the exception should be skipped.
        /// </summary>
        public bool NeedSkipException { get; set; }
        /// <summary>
        /// Limitaion of the exceptions.
        ///     -1:     Skip all exceptions. Do not keep any exception in BatchException instance.
        /// When SkipException == true:
        ///     0:      Skip all exceptions, but keep all exception data in BatchException instance.
        ///     1:      Terminate the batch process in first exception.
        ///     [N]:    Terminate the batch process after reach to N times of exceptions.
        /// When SkipException == false:
        ///     0:      Throw BatchException in end of the process. 
        ///     1:      Throw BatchException in first exception.
        ///     [N]:    Throw BatchException after reach to N times of exceptions.
        /// </summary>
        public int ExceptionLimit { get; set; }

        /// <summary>
        /// Total number of exceptions
        /// </summary>
        public int TotalException { get; set; }
        /// <summary>
        /// Keep batch exceptions.
        /// </summary>
        public List<Exception> Exceptions { get; private set; }

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
        /// Try throw exception if BatchException SkipException is false.
        /// </summary>
        /// <param name="ex">Exception object. It could be null to indicate a flush required.</param>
        public void TryThrow(Exception ex = null)
        {
            if (ex != null)
            {
                TotalException++;
                if (ExceptionLimit >= 0)
                {
                    if (ExceptionLimit == 0 ||
                        ExceptionLimit >= TotalException)
                    {
                        Exceptions.Add(ex);

                        if (!NeedSkipException && ExceptionLimit == TotalException
                            )
                        {
                            throw this;
                        }
                    }
                }
            }
            else
            {
                _chainCounter--;//back to up level chain
                if (!NeedSkipException && TotalException > 0)
                {
                    //throw exception in final stage if any error
                    if (_chainCounter <= 0)
                    {
                        throw this;
                    }
                }
            }
        }
        #endregion
    }
}
