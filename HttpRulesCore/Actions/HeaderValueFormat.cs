using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpRulesCore.Actions
{
    public enum HeaderValueFormat
    {
        /// <summary>
        /// Header format not specified.
        /// </summary>
        None,

        /// <summary>
        /// The headers are delimited by a specific character or string.
        /// </summary>
        Delimited
    }
}
