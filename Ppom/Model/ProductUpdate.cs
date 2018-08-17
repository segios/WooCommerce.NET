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
    public class ProductUpdate: ProductRequest
    {
        public new static string Endpoint { get { return "set/product"; } }

        [DataMember(EmitDefaultValue = false, Name = "fields")]
        public string Fields
        {
            get
            {
                if (PPOMFields == null) {
                    PPOMFields = new List<PPOMField>();
                }
                return JsonHelper.SerializeJSon(PPOMFields);
            }
            set
            {
                if (value == null)
                {
                    PPOMFields = new List<PPOMField>();
                }
                else
                {
                    PPOMFields = JsonHelper.DeserializeJSon<List<PPOMField>>(value);
                }
            }
        }


        [IgnoreDataMember()]
        public List<PPOMField> PPOMFields { get; set; } = new List<PPOMField>();
        
    }



}
