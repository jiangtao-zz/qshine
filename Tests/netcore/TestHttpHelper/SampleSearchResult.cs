using System;
using System.Collections.Generic;
using System.Text;

namespace TestHttpHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class SampleSearchResult
    {
        /// <summary>
        /// 
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string next { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string previous { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<SampleUrl> results { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SampleUrl {
        /// <summary>
        ///
        /// </summary>
        public string url {get;set;}
    }
}
