using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.NET.Ppom.Model
{
   


    [DataContract]
    public class PPOMField
    {
     
        [DataMember(EmitDefaultValue = false, Name = "title")]
        public string Title { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "type")]
        public string Type { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "data_name")]
        public string DataName { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "description")]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "required")]
        public string Required { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "options")]
        public List<PPOMOption> Options { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "selected")]
        public string Selected { get; set; }
        
    }

 

}
