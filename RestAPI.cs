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
using WooCommerceNET.Base;

namespace WooCommerceNET
{
    public class RestAPI
    {
        private string wc_url = string.Empty;
        private string wc_key = "";
        private string wc_secret = "";
        //private bool wc_Proxy = false;

        protected bool AuthorizedHeader { get; set; }

        protected Func<string, string> jsonSeFilter;
        protected Func<string, string> jsonDeseFilter;
        protected Action<HttpWebRequest> webRequestFilter;
        protected Action<HttpWebResponse> webResponseFilter;

        protected Dictionary<Type, object> ApiRegister = new Dictionary<Type, object>();

        public void RegisterApi(object api)
        {
            ApiRegister[api.GetType()] = api;
        }

        public T GetApi<T>()
        {
            var t = typeof(T);
            if (ApiRegister.ContainsKey(t)) {
                return (T)ApiRegister[t];
            }
            return default(T);
        }

        /// <summary>
        /// Initialize the RestAPI object
        /// </summary>
        /// <param name="url">WooCommerce REST API URL, e.g.: http://yourstore/wp-json/wc/v1/ </param>
        /// <param name="key">WooCommerce REST API Key</param>
        /// <param name="secret">WooCommerce REST API Secret</param>
        /// <param name="authorizedHeader">WHEN using HTTPS, do you prefer to send the Credentials in HTTP HEADER?</param>
        /// <param name="jsonSerializeFilter">Provide a function to modify the json string after serilizing.</param>
        /// <param name="jsonDeserializeFilter">Provide a function to modify the json string before deserilizing.</param>
        /// <param name="requestFilter">Provide a function to modify the HttpWebRequest object.</param>
        /// <param name="responseFilter">Provide a function to grab information from the HttpWebResponse object.</param>
        public RestAPI(string url, string key, string secret, bool authorizedHeader = true, 
                            Func<string, string> jsonSerializeFilter = null, 
                            Func<string, string> jsonDeserializeFilter = null, 
                            Action<HttpWebRequest> requestFilter = null,
                            Action<HttpWebResponse> responseFilter = null)//, bool useProxy = false)
        {
            if (string.IsNullOrEmpty(url))
                throw new Exception("Please use a valid WooCommerce Restful API url.");

            string urlLower = url.Trim().ToLower().TrimEnd('/');
            SetApiVersion(urlLower);
            
            wc_url = url + (url.EndsWith("/") ? "" : "/");
            wc_key = key;
            AuthorizedHeader = authorizedHeader;

            //Why extra '&'? look here: https://wordpress.org/support/topic/woocommerce-rest-api-v3-problem-woocommerce_api_authentication_error/
            if ((url.ToLower().Contains("wc-api/v3") || !IsLegacy) && !wc_url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                wc_secret = secret + "&";
            else
                wc_secret = secret;

            jsonSeFilter = jsonSerializeFilter;
            jsonDeseFilter = jsonDeserializeFilter;
            webRequestFilter = requestFilter;
            webResponseFilter = responseFilter;

            //wc_Proxy = useProxy;
        }

        protected virtual void SetApiVersion(string urlLower)
        {
            if (urlLower.EndsWith("wc-api/v1") || urlLower.EndsWith("wc-api/v2") || urlLower.EndsWith("wc-api/v3"))
                Version = APIVersion.Legacy;
            else if (urlLower.EndsWith("wp-json/wc/v1"))
                Version = APIVersion.Version1;
            else if (urlLower.EndsWith("wp-json/wc/v2"))
                Version = APIVersion.Version2;
            else if (urlLower.Contains("wp-json/wc-"))
                Version = APIVersion.ThirdPartyPlugins;
            else
            {
                Version = APIVersion.Unknown;
                throw new Exception("Unknow WooCommerce Restful API version.");
            }
        }

        public bool IsLegacy
        {
            get
            {
                return Version == APIVersion.Legacy;
            }
        }

        public APIVersion Version { get; protected set; }

        public string Url { get { return wc_url; } }

        /// <summary>
        /// Make Restful calls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="method">HEAD, GET, POST, PUT, PATCH, DELETE</param>
        /// <param name="requestBody">If your call doesn't have a body, please pass string.Empty, not null.</param>
        /// <param name="parms"></param>
        /// <returns>json string</returns>
        public virtual async Task<string> SendHttpClientRequest<T>(string endpoint, RequestMethod method, T requestBody, Dictionary<string, string> parms = null)
        {
            HttpWebRequest httpWebRequest = null;
            try
            {
                if (wc_url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    if (AuthorizedHeader == true)
                    {
                        httpWebRequest = (HttpWebRequest)WebRequest.Create(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                        httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(wc_key + ":" + wc_secret));
                    }
                    else
                    {
                        if (parms == null)
                            parms = new Dictionary<string, string>();

                        if (!parms.ContainsKey("consumer_key"))
                            parms.Add("consumer_key", wc_key);
                        if (!parms.ContainsKey("consumer_secret"))
                            parms.Add("consumer_secret", wc_secret);

                        httpWebRequest = (HttpWebRequest)WebRequest.Create(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                    }
                }
                else
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                }

                // start the stream immediately
                httpWebRequest.Method = method.ToString();
                httpWebRequest.AllowReadStreamBuffering = false;

                if (webRequestFilter != null)
                    webRequestFilter.Invoke(httpWebRequest);

                //if (wc_Proxy)
                //    httpWebRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                //else
                //    httpWebRequest.Proxy = null;

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
                return e.Message;
            }
        }

        public async Task<string> SendFile(string endpoint, RequestMethod method, byte[] fileData, string fileName, Dictionary<string, string> parms = null)
        {
            HttpWebRequest httpWebRequest = null;
            try
            {
                if (wc_url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    if (AuthorizedHeader == true)
                    {
                        httpWebRequest = (HttpWebRequest)WebRequest.Create(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                        httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(wc_key + ":" + wc_secret));
                    }
                    else
                    {
                        if (parms == null)
                            parms = new Dictionary<string, string>();

                        if (!parms.ContainsKey("consumer_key"))
                            parms.Add("consumer_key", wc_key);
                        if (!parms.ContainsKey("consumer_secret"))
                            parms.Add("consumer_secret", wc_secret);

                        httpWebRequest = (HttpWebRequest)WebRequest.Create(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                    }
                }
                else
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                }

                // start the stream immediately
                httpWebRequest.Method = method.ToString();
                httpWebRequest.AllowReadStreamBuffering = false;

                if (webRequestFilter != null)
                    webRequestFilter.Invoke(httpWebRequest);

                //if (wc_Proxy)
                //    httpWebRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                //else
                //    httpWebRequest.Proxy = null;
                string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

                string extension = fileName.Split('.').Last();
                var contentType = MimeTypeHelper.GetMIMETypeFromExtension(extension);

                httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                // Build up the post message header
                StringBuilder sb = new StringBuilder();
                sb.Append("--");
                sb.Append(boundary);
                sb.Append("\r\n");
                sb.Append("Content-Disposition: form-data; name=\"");
                sb.Append(fileName);
                sb.Append("\"; filename=\"");
                sb.Append(fileName);
                sb.Append("\"");
                sb.Append("\r\n");
                sb.Append("Content-Type: ");
                sb.Append(contentType);
                sb.Append("\r\n");
                sb.Append("\r\n");
                string postHeader = sb.ToString();
                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
                byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                long length = postHeaderBytes.Length + fileData.Length + boundaryBytes.Length + 2;
                httpWebRequest.ContentLength = length;
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                requestStream.Write(fileData, 0, fileData.Length);
                boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);


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
                return e.Message;
            }
        }

        public async Task<string> GetRestful(string endpoint, Dictionary<string, string> parms = null)
        {
            return await SendHttpClientRequest(endpoint, RequestMethod.GET, string.Empty, parms).ConfigureAwait(false);
        }

        public async Task<string> PostRestful(string endpoint, object jsonObject, Dictionary<string, string> parms = null)
        {
            return await SendHttpClientRequest(endpoint, RequestMethod.POST, jsonObject, parms).ConfigureAwait(false);
        }

        public async Task<string> SendFile(string endpoint, byte[] data, string file, Dictionary<string, string> parms = null)
        {
            return await SendFile(endpoint, RequestMethod.POST, data, file, parms).ConfigureAwait(false);
        }

        public async Task<string> PutRestful(string endpoint, object jsonObject, Dictionary<string, string> parms = null)
        {
            return await SendHttpClientRequest(endpoint, RequestMethod.PUT, jsonObject, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteRestful(string endpoint, Dictionary<string, string> parms = null)
        {
            return await SendHttpClientRequest(endpoint, RequestMethod.DELETE, string.Empty, parms).ConfigureAwait(false);
        }

        protected string GetOAuthEndPoint(string method, string endpoint, Dictionary<string, string> parms = null)
        {
            if (wc_url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
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

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("oauth_consumer_key", wc_key);
            dic.Add("oauth_nonce", Guid.NewGuid().ToString("N"));
            dic.Add("oauth_signature_method", "HMAC-SHA256");
            dic.Add("oauth_timestamp", Common.GetUnixTime(false));

            if (parms != null)
                foreach (var p in parms)
                    dic.Add(p.Key, p.Value);

            string base_request_uri = method.ToUpper() + "&" + Uri.EscapeDataString(wc_url + endpoint) + "&";
            string stringToSign = string.Empty;

            foreach (var parm in dic.OrderBy(x => x.Key))
                stringToSign += Uri.EscapeDataString(parm.Key) + "=" + Uri.EscapeDataString(parm.Value) + "&";

            base_request_uri = base_request_uri + Uri.EscapeDataString(stringToSign.TrimEnd('&'));

            dic.Add("oauth_signature", Common.GetSHA256(wc_secret, base_request_uri));

            string parmstr = string.Empty;
            foreach (var parm in dic)
                parmstr += parm.Key + "=" + Uri.EscapeDataString(parm.Value) + "&";

            return endpoint + "?" + parmstr.TrimEnd('&');
        }
        
        protected async Task<string> GetStreamContent(Stream s, string charset)
        {
            StringBuilder sb = new StringBuilder();
            byte[] Buffer = new byte[512];
            int count = 0;
            count = await s.ReadAsync(Buffer, 0, Buffer.Length).ConfigureAwait(false);
            while (count > 0)
            {
                sb.Append(Encoding.GetEncoding(charset).GetString(Buffer, 0, count));
                count = await s.ReadAsync(Buffer, 0, Buffer.Length).ConfigureAwait(false);
            }

            return sb.ToString();
        }

        public virtual string SerializeJSon<T>(T t)
        {
            string jsonString = JsonHelper.SerializeJSon<T>(t, DateTimeFormat);

            if (IsLegacy)
            {
                if (typeof(T).IsArray)
                    jsonString = "{\"" + typeof(T).Name.ToLower().Replace("[]", "s") + "\":" + jsonString + "}";
                else
                    jsonString = "{\"" + typeof(T).Name.ToLower() + "\":" + jsonString + "}";
            }

            if (jsonSeFilter != null)
                jsonString = jsonSeFilter.Invoke(jsonString);

            return jsonString;
        }

        public virtual T DeserializeJSon<T>(string jsonString)
        {
            if (jsonDeseFilter != null)
                jsonString = jsonDeseFilter.Invoke(jsonString);


            T obj = JsonHelper.DeserializeJSon<T>(jsonString, DateTimeFormat);

            return obj;
        }

        public string DateTimeFormat
        {
            get
            {
                return IsLegacy ? "yyyy-MM-ddTHH:mm:ssZ" : "yyyy-MM-ddTHH:mm:ss";
            }
        }

        public string WcUrl { get { return wc_url; } set { wc_url = value; } }
    }

    public enum RequestMethod
    {
        HEAD = 1,
        GET = 2,
        POST = 3,
        PUT = 4,
        PATCH = 5,
        DELETE = 6
    }

    public enum APIVersion
    {
        Unknown = 0,
        Legacy = 1,
        Version1 = 2,
        Version2 = 3,
        ThirdPartyPlugins = 99
    }
}
