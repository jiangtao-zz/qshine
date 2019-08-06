using qshine.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Represents a predefined system error which contains an error code and message.
    /// <![CDATA[
    ///     var code = new MessageCode("TestClass", "E");
    ///     
    ///     code.ToString(100)
    ///     code.ToString(100, "This is a sample message");
    ///     //The result will like 
    ///     //    TestClass-E-100
    ///     //    TestClass-E-100::This is a sample message
    /// 
    ///     //optional
    ///     code.MessageFormat = (prefix, code, messageType) => { return string.Format("{0}-{1}-{2}",prefix, messageType, code);
    ///     };
    ///     
    /// ]]>
    /// 
    /// 
    /// </summary>
    public class MessageCode
    {
        #region Fields
        private string _prefix;
        private string _messageType;
        #endregion

        #region Static

        /// <summary>
        /// Get error code
        /// </summary>
        /// <param name="prefix">ErrorCode prefix</param>
        /// <returns></returns>
        public static MessageCode ErrorCode(string prefix)
        {
            return new MessageCode(prefix, "E");
        }

        /// <summary>
        /// Get error code
        /// </summary>
        /// <param name="prefix">code prefix</param>
        /// <returns></returns>
        public static MessageCode WarningCode(string prefix)
        {
            return new MessageCode(prefix, "W");
        }

        /// <summary>
        /// Global message format
        /// </summary>
        public static Func<string, string, string, string, string> GlobalMessageFormat = (prefix, code, messageType, message) =>
         {
             string codePart;
             if (string.IsNullOrEmpty(messageType))
             {
                 codePart = string.Format("{0}-{1}", prefix, code);
             }
             codePart = string.Format("{0}-{1}-{2}", prefix, messageType, code);

             if (!string.IsNullOrEmpty(message))
             {
                 return string.Format("{0}::{1}", codePart, message);
             }
             return codePart;
         };


        #endregion

        #region Ctro.

        /// <summary>
        /// Defines a message code
        /// </summary>
        /// <param name="prefix">code prefix.</param>
        /// <param name="codeType">The message code type. 
        /// </param>
        public MessageCode(string prefix, string codeType)
        {
            _prefix = prefix;
            _messageType = codeType;
        }

        /// <summary>
        /// Defines a message code
        /// </summary>
        /// <param name="prefix">code prefix.</param>
        public MessageCode(string prefix)
            : this(prefix, "")
        {
        }

        #endregion

        #region public Properties and Methods

        /// <summary>
        /// Output a formatted code
        /// </summary>
        /// <param name="code">message code</param>
        /// <returns></returns>
        public string ToString(string code)
        {
            if (MessageFormat != null) return MessageFormat(_prefix, code, _messageType,"");

            if(GlobalMessageFormat!=null) return GlobalMessageFormat(_prefix, code, _messageType, "");

            return code.ToString();
        }

        /// <summary>
        /// Output cided message
        /// </summary>
        /// <param name="code">message code</param>
        /// <param name="message">message</param>
        /// <returns></returns>
        public string ToString(string code, string message)
        {
            if (MessageFormat != null) return MessageFormat(_prefix, code, _messageType, message);

            if (GlobalMessageFormat != null) return GlobalMessageFormat(_prefix, code, _messageType, message);

            return code.ToString();
        }


        /// <summary>
        /// Set message format
        /// </summary>
        public Func<string, string, string, string, string> MessageFormat;
        
        #endregion

    }
}
