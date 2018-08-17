using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.NET.Ppom.Model
{
    [DataContract]
    public class OrderRequest
    {
        public static string Endpoint { get { return "get/order"; } }

      
        [DataMember(EmitDefaultValue = false, Name = "order_id")]
        public int OrderId { get; set; }


        [DataMember(EmitDefaultValue = false, Name = "secret_key")]
        public string SecretKey { get; set; }
        
    }

}
