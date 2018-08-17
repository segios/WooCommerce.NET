using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.NET.Ppom.Model
{
   


    [DataContract]
    public class PPOMOption
    {
        [DataMember(EmitDefaultValue = false, Name = "option")]
        public string Option { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "price")]
        public string Price { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "id")]
        public Guid Id { get; set; }
    }

}
