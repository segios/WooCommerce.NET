using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerceNET
{
    public static class DeafultFilters
    {
        public static string WooCommerceDeserializeFilter(string data)
        {
            const string html = "</html>";
            const string body = "</body>";

            int indexOfEnd = data.IndexOf(html);
            if (indexOfEnd > 0)
            {
                var res = data.Substring(indexOfEnd + html.Length);
                return res;
            }

            indexOfEnd = data.IndexOf(body);
            if (indexOfEnd > 0)
            {
                var res = data.Substring(indexOfEnd + body.Length);
                return res;
            }

            return data;
        }
    }
}
