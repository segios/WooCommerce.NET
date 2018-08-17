using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce.NET.Ppom.Model;

namespace WooCommerce.NET.Ppom
{
    public class OrderApi
    {
        public PPOMRestAPI API { get; protected set; }
        public OrderApi(PPOMRestAPI ppomRestAPI)
        {
            API = ppomRestAPI;
        }

        public async Task<OrderResponse> GetOrderPPOM(OrderRequest request, Dictionary<string, string> parms = null)
        {
            var endPoint = OrderRequest.Endpoint;

            if (parms == null)
                parms = new Dictionary<string, string>();

            if (!parms.ContainsKey("order_id "))
                parms.Add("order_id", request.OrderId.ToString());
            if (!parms.ContainsKey("secret_key "))
                parms.Add("secret_key", API.PpomKey);

            return API.DeserializeJSon<OrderResponse>(await API.GetRestful(endPoint, parms).ConfigureAwait(false));
        }

    }
}
