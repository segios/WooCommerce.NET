using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce.NET.Ppom.Model;

namespace WooCommerce.NET.Ppom
{
    public class ProductApi
    {
        public PPOMRestAPI API { get; protected set; }
        public ProductApi(PPOMRestAPI ppomRestAPI)
        {
            API = ppomRestAPI;
        }

        public async Task<ProductResponse> GetProductsPPOM(ProductRequest request, Dictionary<string, string> parms = null)
        {
            var endPoint = ProductRequest.Endpoint;

            if (parms == null)
                parms = new Dictionary<string, string>();

            if (!parms.ContainsKey("product_id "))
                parms.Add("product_id", request.ProductId.ToString());
            if (!parms.ContainsKey("secret_key "))
                parms.Add("secret_key", API.PpomKey);

            return API.DeserializeJSon<ProductResponse>(await API.GetRestful(endPoint, parms).ConfigureAwait(false));
        }

        public async Task<ProductResponse> SetProductsPPOM(ProductUpdate request, Dictionary<string, string> parms = null)
        {
            var endPoint = ProductUpdate.Endpoint;

            request.SecretKey = API.PpomKey;

            return API.DeserializeJSon<ProductResponse>(await API.PostRestful(endPoint, request, parms).ConfigureAwait(false));
        }

        public async Task<ProductResponse> DeleteProductsPPOM(ProductDelete request, Dictionary<string, string> parms = null)
        {
            var endPoint = ProductDelete.Endpoint;

            request.SecretKey = API.PpomKey;

            return API.DeserializeJSon<ProductResponse>(await API.PostRestful(endPoint, request, parms).ConfigureAwait(false));
        }
    }
}
