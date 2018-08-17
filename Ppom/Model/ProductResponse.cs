using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.NET.Ppom.Model
{
    [DataContract]
    public class ProductResponse: ResponseBase
    {


        [DataMember(EmitDefaultValue = false, Name = "meta_id")]
        public int? MetaId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "product_id")]
        public int? ProductId { get; set; }


        [DataMember(EmitDefaultValue = false, Name = "ppom_fields")]
        public List<PPOMField> PPOMFields { get; set; }
        
    }


}
