using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using WooCommerce.NET.Exceptions;
using WooCommerce.NET.Util;
using WooCommerceNET;
using WooCommerceNET.Base;

namespace WooCommerce.NET.Ppom
{
    public class PPOMRestAPI : RestAPI
    {
        const string urlModifier = "/wp-json/ppom/v1";
        public string PpomKey { get; }


        public PPOMRestAPI(string url, string key, string secret, bool authorizedHeader = true,
                            Func<string, string> jsonSerializeFilter = null,
                            Func<string, string> jsonDeserializeFilter = null,
                            Action<HttpWebRequest> requestFilter = null,
                            Action<HttpWebResponse> responseFilter = null)//, bool useProxy = false)
            : base(url.TrimEnd('/') + urlModifier, key, secret, authorizedHeader, jsonSerializeFilter, jsonDeserializeFilter, requestFilter, responseFilter)
        {
            PpomKey = key;
        }

        protected override void SetApiVersion(string urlLower)
        {
            Version = APIVersion.Version1;
        }

        protected string AttachParameters(string endpoint, Dictionary<string, string> parms = null)
        {
            if (parms == null)
                return endpoint;
            else
            {
                string requestParms = string.Empty;
                foreach (var parm in parms)
                    requestParms += parm.Key + "=" + parm.Value + "&";

                return endpoint + "?" + requestParms.TrimEnd('&');
            }
        }

        public override async Task<string> SendHttpClientRequest<T>(string endpoint, RequestMethod method, T requestBody, Dictionary<string, string> parms = null)
        {
            HttpWebRequest httpWebRequest = null;
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(WcUrl + AttachParameters(endpoint, parms));

                // start the stream immediately
                httpWebRequest.Method = method.ToString();
                httpWebRequest.AllowReadStreamBuffering = false;

                if (webRequestFilter != null)
                    webRequestFilter.Invoke(httpWebRequest);

                if (requestBody.GetType() != typeof(string))
                {
                    httpWebRequest.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(SerializeJSon(requestBody));
                    Stream dataStream = await httpWebRequest.GetRequestStreamAsync().ConfigureAwait(false);
                    dataStream.Write(buffer, 0, buffer.Length);
                }

                // asynchronously get a response
                WebResponse wr = await httpWebRequest.GetResponseAsync().ConfigureAwait(false);

                if (webResponseFilter != null)
                    webResponseFilter.Invoke((HttpWebResponse)wr);

                return await GetStreamContent(wr.GetResponseStream(), wr.ContentType.Contains("=") ? wr.ContentType.Split('=')[1] : "UTF-8").ConfigureAwait(false);
            }
            catch (WebException we)
            {
                if (httpWebRequest != null && httpWebRequest.HaveResponse)
                {
                    if (we.Response != null)
                        throw new WooCommerceException(await GetStreamContent(we.Response.GetResponseStream(), we.Response.ContentType.Contains("=") ? we.Response.ContentType.Split('=')[1] : "UTF-8").ConfigureAwait(false), we.InnerException);
                    else
                        throw;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                throw new WooCommerceException(e.Message, e.InnerException);
            }
        }


    }
}
