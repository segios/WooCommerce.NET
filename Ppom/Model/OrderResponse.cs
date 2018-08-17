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
    public class OrderResponse : ResponseBase
    {
        [DataMember(EmitDefaultValue = false, Name = "order_items_meta")]
        public List<OrderItemMeta> OrderItemsMeta { get; set; }
    }

    [DataContract]
    public class OrderItemMeta
    {
        [DataMember(EmitDefaultValue = false, Name = "product_id")]
        public int ProductId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "product_meta_data")]
        public List<ItemData> OrderItemsMeta { get; set; }
    }

    [DataContract]
    public class ItemData
    {
        [DataMember(EmitDefaultValue = false, Name = "id")]
        public int Id { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "key")]
        public string Key { get; set; }

        private string _value;
        [DataMember(EmitDefaultValue = false, Name = "value")]
        public string Value
        {
            get
            {
                if (Values == null)
                {
                    Values = new List<OptionValue>();
                    if (!string.IsNullOrEmpty(_value))
                    {
                        return _value;
                    }
                }

                return JsonHelper.SerializeJSon(Values);
            }
            set
            {
                _value = value;
                Values = new List<OptionValue>();
                try
                {
                    Values = JsonHelper.DeserializeJSon<List<OptionValue>>(value);
                }
                catch
                {
                }
            }
        }


        [IgnoreDataMember()]
        public List<OptionValue> Values { get; set; }

    }

    [DataContract]
    public class OptionValue
    {
        [DataMember(EmitDefaultValue = false, Name = "id")]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "option")]
        public string Option { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "price")]
        public decimal? Price { get; set; }

    }
}
