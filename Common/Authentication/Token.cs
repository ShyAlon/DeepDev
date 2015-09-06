using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIBuildIt.Common.Authentication
{
    public class Token
    {
        /// <summary>
        /// The Id of the token
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The token's text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The tokens creation time
        /// </summary>
        public DateTime Creation { get; set; }

        /// <summary>
        /// The tokens expiration time
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// The Id of the user the token belongs to
        /// </summary>
        public int UserId { get; set; }
    }
}
