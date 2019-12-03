using qshine.Utility.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qshine.oauth2
{
    /// <summary>
    /// User information from OAuth2 resource server.
    /// </summary>
    public class UserInfo: WebApiResult
    {
        /// <summary>
        /// User Id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// User login name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User given name
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// User family name
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// User gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// User prefer locale
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// user profile picture url
        /// </summary>
        public string ProfilePictureUrl { get; set; }

        /// <summary>
        /// Timezone, number in GMT
        /// </summary>
        public int TimeZone { get; set; }
    }
}
