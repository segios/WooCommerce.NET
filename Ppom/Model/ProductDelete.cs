using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WooCommerce.NET.Util;

namespace WooCommerce.NET.Ppom.Model
{
    [DataContract]
    public class ProductDelete : ProductRequest
    {
        public new static string Endpoint { get { return "delete/product"; } }

        [DataMember(EmitDefaultValue = false, Name = "fields")]
        public string Fields
        {
            get
            {
                if (PPOMFields == null)
                {
                    PPOMFields = new List<string>();
                }
                return JsonHelper.SerializeJSon(PPOMFields);
            }
            set
            {
                if (value == null)
                {
                    PPOMFields = new List<string>();
                }
                else
                {
                    PPOMFields = JsonHelper.DeserializeJSon<List<string>>(value);
                }
            }
        }


        [IgnoreDataMember()]
        public List<string> PPOMFields { get; set; } = new List<string>();
    }


}
