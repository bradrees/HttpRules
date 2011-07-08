using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpRulesCore.Tests
{
    public static class Extensions
    {
        public static RequestProperties CreateSimpleRequest(this Uri uri)
        {
            return new RequestProperties(uri, uri, "*/*");
        }
    }
}
