using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace www778878net.net
{
    /// <summary>
    /// httpclient
    /// </summary>
    public class HttpClient78
    {
		
		#region 静态成员
		/// <summary>
		/// 
		/// </summary>
		public static HttpClient78 Client78 { get; set; }
        /// <summary>
        /// 最大重试次数 (可用于 出错内部重试)
        /// </summary>	
        public const byte MaxTries = 3;
        /// <summary>
        /// 10秒超时
        /// </summary>
        public const byte TimeoutSec = 30;
        private static readonly ServiceCollection serviceCollection;
        private static readonly ServiceProvider serviceProvider;
        private static readonly IHttpClientFactory? HttpClientFactory;
		#endregion

		#region 成员
		/// <summary>
		/// cookie
		/// </summary>
		public string? Cookiestr { get; set; }
		public readonly HttpClient HttpClient;
		#endregion


		/// <summary>
		/// 单例 方便简单调用 默认重试3次
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		static HttpClient78()
        {
            string sname = "7788";
            serviceCollection = new();
       
			serviceCollection.AddHttpClient(sname, client =>
			{
#if !NETFRAMEWORK
				client.DefaultRequestVersion = HttpVersion.Version30;
#endif
				client.Timeout = TimeSpan.FromSeconds(TimeoutSec);
				//client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("7788soft", "5.2.7.2"));
				//client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(source; .NET 6.0.5; win10-x64; Microsoft Windows 10.0.19042; +https://github.com/JustArchiNET/ArchiSteamFarm)"));
			});
			//自定义重试了 不用这个
			//TimeSpan[] times = new TimeSpan[MaxTries];
			//for (int i = 0; i < MaxTries; i++)
			//{
			//	times[i] = TimeSpan.FromSeconds(2 + i * 3);
			//}
			//.AddTransientHttpErrorPolicy(
			//           builder => builder.WaitAndRetryAsync(times));
			serviceProvider = serviceCollection.BuildServiceProvider();
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            HttpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            if (HttpClientFactory is null)
            {
                throw new ArgumentNullException(nameof(HttpClientFactory));
            }
            Client78 = new();

        }

        public HttpClient78(string sname = "7788")
        {
            HttpClient = HttpClientFactory!.CreateClient(sname);
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            serviceProvider.Dispose(); 
        }

        
        public static HttpClient CreateHttpClient()
        {
            HttpClient result = HttpClientFactory!.CreateClient("7788");
            return result;
        } 
 
		private async Task<HttpResponseMessage?> InternalRequest<T>(Uri request
			, HttpMethod httpMethod
			, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null
			, T? data = null, Uri? referer = null
			, ERequestOptions requestOptions = ERequestOptions.None
			, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead
			, byte maxRedirections = MaxTries) where T : class
		{
			ArgumentNullException.ThrowIfNull(request);
			ArgumentNullException.ThrowIfNull(httpMethod);

			HttpResponseMessage response;

			while (true)
			{
				using (HttpRequestMessage requestMessage = new(httpMethod, request))
				{
#if !NETFRAMEWORK
					requestMessage.Version = HttpClient.DefaultRequestVersion;
#endif

					if (!string.IsNullOrEmpty(Cookiestr))
						requestMessage.Headers.Add("Cookie", Cookiestr);

					if (data != null)
					{
						switch (data)
						{
							case HttpContent content:
								requestMessage.Content = content;

								break;
							case IReadOnlyCollection<KeyValuePair<string, string>> nameValueCollection:
								try
								{
									requestMessage.Content = new FormUrlEncodedContent(nameValueCollection);
								}
								catch (UriFormatException)
								{
									requestMessage.Content = new StringContent(string.Join("&", nameValueCollection.Select(static kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}")), null, "application/x-www-form-urlencoded");
								}

								break;
							case string text:
								requestMessage.Content = new StringContent(text, Encoding.UTF8, "application/x-www-form-urlencoded");
								//requestMessage.Content = new StringContent(text); 
								break;
							default://obj
								requestMessage.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

								break;
						}
					}

					if (headers != null)
					{
						foreach ((string header, string value) in headers)
						{
							//if (header == "Content-type")
							//	requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
							//else
							requestMessage.Headers.Add(header, value);
						}
					}

					if (referer != null)
					{
						requestMessage.Headers.Referrer = referer;
					}


					await Log78.AddDebug($"{httpMethod} {request}");


					try
					{
						response = await HttpClient.SendAsync(requestMessage, httpCompletionOption).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						//await Log78.AddErr(e);

						return null;
					}
					finally
					{
						if (data is HttpContent)
						{
							// We reset the request content to null, as our http content will get disposed otherwise, and we still need it for subsequent calls, such as redirections or retries
							requestMessage.Content = null;
						}
					}
				}


				await Log78.AddDebug($"{response.StatusCode} <- {httpMethod} {request}");


				if (response.IsSuccessStatusCode)
				{
					return response;
				}

				// WARNING: We still have not disposed response by now, make sure to dispose it ASAP if we're not returning it!
				if (response.StatusCode.IsRedirectionCode() && (maxRedirections > 0))
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
					{
						// User wants to handle it manually, that's alright
						return response;
					}

					Uri? redirectUri = response.Headers.Location;

					if (redirectUri == null)
					{
						await Log78.AddErrStr("redirectUri is null");
						return null;
					}

					if (redirectUri.IsAbsoluteUri)
					{
						switch (redirectUri.Scheme)
						{
							case "http" or "https":
								break;
							case "steammobile":
								// Those redirections are invalid, but we're aware of that and we have extra logic for them
								return response;
							default:
								// We have no clue about those, but maybe HttpClient can handle them for us
								await Log78.AddErrStr(nameof(redirectUri.Scheme) + redirectUri.Scheme);

								break;
						}
					}
					else
					{
						redirectUri = new Uri(request, redirectUri);
					}

					switch (response.StatusCode)
					{
						case HttpStatusCode.MovedPermanently: // Per https://tools.ietf.org/html/rfc7231#section-6.4.2, a 301 redirect may be performed using a GET request
						case HttpStatusCode.Redirect: // Per https://tools.ietf.org/html/rfc7231#section-6.4.3, a 302 redirect may be performed using a GET request
						case HttpStatusCode.SeeOther: // Per https://tools.ietf.org/html/rfc7231#section-6.4.4, a 303 redirect should be performed using a GET request
							if (httpMethod != HttpMethod.Head)
							{
								httpMethod = HttpMethod.Get;
							}

							// Data doesn't make any sense for a fetch request, clear it in case it's being used
							data = null;

							break;
					}

					response.Dispose();

					// Per https://tools.ietf.org/html/rfc7231#section-7.1.2, a redirect location without a fragment should inherit the fragment from the original URI
					if (!string.IsNullOrEmpty(request.Fragment) && string.IsNullOrEmpty(redirectUri.Fragment))
					{
						redirectUri = new UriBuilder(redirectUri) { Fragment = request.Fragment }.Uri;
					}

					request = redirectUri;
					maxRedirections--;

					continue;
				}

				break;
			}


			await Log78.AddDebug($"{response.StatusCode} <- {httpMethod} {request}");


			if (response.StatusCode.IsClientErrorCode())
			{

				await Log78.AddDebug(await response.Content.ReadAsStringAsync().ConfigureAwait(false));


				// Do not retry on client errors
				return response;
			}

			if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors) && response.StatusCode.IsServerErrorCode())
			{

				await Log78.AddDebug(await response.Content.ReadAsStringAsync().ConfigureAwait(false));


				// Do not retry on server errors in this case
				return response;
			}

			using (response)
			{

				await Log78.AddDebug(await response.Content.ReadAsStringAsync().ConfigureAwait(false));


				return null;
			}
		}

	}
}
