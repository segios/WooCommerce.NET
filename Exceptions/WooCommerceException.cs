using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WooCommerce.NET.Util;
using WooCommerce.NET.WooCommerce.v2;

namespace WooCommerce.NET.Exceptions
{
    public class WooCommerceException: Exception
    {
        public WooCommerceException() { }
        public WooCommerceException(string msg): base (msg) { }
        public WooCommerceException(string msg, Exception inner) : base(msg, inner) { }

        public WcError TryGetError() {
            try
            {
                WcError error = JsonConvert.DeserializeObject<WcError>(Message);
                return error;
            }
            catch { }

            return null;
        }
    }
}
