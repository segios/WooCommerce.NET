using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.NET.Ppom.Model
{
    [DataContract]
    public class ProductRequest
    {
        public static string Endpoint { get { return "get/product"; } }

      
        [DataMember(EmitDefaultValue = false, Name = "product_id")]
        public int ProductId { get; set; }


        [DataMember(EmitDefaultValue = false, Name = "secret_key")]
        public string SecretKey { get; set; }
        
    }

}
