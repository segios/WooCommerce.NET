using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.NET.Ppom.Model
{
    [DataContract]
    public class ResponseBase
    {

        [DataMember(EmitDefaultValue = false, Name = "status")]
        public string Status { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "message")]
        public string Message { get; set; }

        
    }


}
