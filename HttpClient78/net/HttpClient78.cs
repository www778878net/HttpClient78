using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using www778878net.Log;
using www778878net.Net.Response;

namespace www778878net.net
{
    /// <summary>
    /// httpclient
    /// </summary>
    public class HttpClient78
    {
		
		#region 静态成员
		/// <summary>
		/// 单例连接
		/// </summary>
		public static HttpClient78 Client78 { get; set; }
        /// <summary>
        /// 最大重试次数 (可用于 出错内部重试)
        /// </summary>	
        public const  byte MaxTries   = 3;
        /// <summary>
        /// 30秒超时
        /// </summary>
        public const  byte TimeoutSec  = 20;
        private static readonly ServiceCollection serviceCollection;
        private static readonly ServiceProvider serviceProvider;
        private static readonly IHttpClientFactory? HttpClientFactory;
		#endregion

		#region 成员
		/// <summary>
		/// cookie 字符串
		/// </summary>
		public string? Cookiestr { get; set; }
		/// <summary>
		/// HttpClient连接
		/// </summary>
		public HttpClient HttpClient { get; set; }
		/// <summary>
		/// 是否打印详细调试信息
		/// </summary>
		public bool IsDebug { get; set; }
		/// <summary>
		/// log78 日志对象
		/// </summary>
		public   Log78 Logger { get; set; }
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
			IsDebug = false;
            Logger = new()
            {
                Logger = LogManager.GetCurrentClassLogger()
            };
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            serviceProvider.Dispose(); 
        }

        /// <summary>
		/// 返回一个连接
		/// </summary>
		/// <returns></returns>
        public static HttpClient CreateHttpClient()
        {
            HttpClient result = HttpClientFactory!.CreateClient("7788");
            return result;
        }

		/// <summary>
		/// 通过网址下载文件
		/// </summary>
		/// <param name="Urlpath"></param>
		/// <param name="Filepath"></param>
		/// <returns></returns>
		public async Task<int> DownFile(string Urlpath, string Filepath)
		{
			StreamResponse? tmpstm = await GetToStream(new Uri(Urlpath));
			if (tmpstm == null || tmpstm.Content == null)
				return -1;
			using (Stream stream = tmpstm.Content)
			{
				using var fileStream = File.Open(Filepath, FileMode.Create);
				stream.CopyTo(fileStream);
			}
			return 0;
		}

		public async Task<ObjectResponse<String>?> PostStringToString(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, string? data = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 0)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				StreamResponse? response = await PostToStream(request, headers, data, referer, requestOptions | ERequestOptions.ReturnClientErrors, 1, rateLimitingDelay).ConfigureAwait(false);

				if (response?.Content == null)
				{
					// Request timed out, try again
					continue;
				}

				await using (response.ConfigureAwait(false))
				{
					if (response.StatusCode.IsRedirectionCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
						{
							return new ObjectResponse<String>(response);
						}

						break;
					}

					if (response.StatusCode.IsClientErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
						{
							return new ObjectResponse<String>(response);
						}

						break;
					}

					if (response.StatusCode.IsServerErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
						{
							return new ObjectResponse<String>(response);
						}

						continue;
					}

					String? obj;

					try
					{
						using StreamReader streamReader = new(response.Content);
						obj = streamReader.ReadToEnd();
						//using JsonTextReader jsonReader = new(streamReader);

						//JsonSerializer serializer = new();
						//obj = serializer.Deserialize<T>(jsonReader);

					}
					catch (Exception e)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new ObjectResponse<string>(response);
						}

						Logger.Error(e);
						continue;
					}

					if (obj is null)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new ObjectResponse<string>(response);
						}
						Logger.Error ("obj is null");

						continue;
					}

					return new ObjectResponse<string>(response, obj);
				}
			}

			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries + request.AbsoluteUri + "失败的请求" + request);

			return null;
		}

		public async Task<StreamResponse?> PostToStream<T>(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, T? data = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 0) where T : class
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				HttpResponseMessage? response = await PrivatePost(request, headers, data, referer, requestOptions, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

				if (response == null)
				{
					// Request timed out, try again
					continue;
				}

				if (response.StatusCode.IsRedirectionCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
					{
						return new StreamResponse(response);
					}

					break;
				}

				if (response.StatusCode.IsClientErrorCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
					{
						return new StreamResponse(response);
					}

					break;
				}

				if (response.StatusCode.IsServerErrorCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
					{
						return new StreamResponse(response);
					}

					continue;
				}

				return new StreamResponse(response, await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
			}

			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries + request.AbsoluteUri + "失败的请求" + request);

			return null;
		}

		public async Task<ObjectResponse<String>?> PostToString<T>(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, JObject? data = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 0) where T : class
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				StreamResponse? response = await PostToStream(request, headers, data, referer, requestOptions | ERequestOptions.ReturnClientErrors, 1, rateLimitingDelay).ConfigureAwait(false);

				if (response?.Content == null)
				{
					// Request timed out, try again
					continue;
				}

				await using (response.ConfigureAwait(false))
				{
					if (response.StatusCode.IsRedirectionCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
						{
							return new ObjectResponse<String>(response);
						}

						break;
					}

					if (response.StatusCode.IsClientErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
						{
							return new ObjectResponse<String>(response);
						}

						break;
					}

					if (response.StatusCode.IsServerErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
						{
							return new ObjectResponse<String>(response);
						}

						continue;
					}

					String? obj;

					try
					{
						using StreamReader streamReader = new(response.Content);
						obj = streamReader.ReadToEnd();
						//using JsonTextReader jsonReader = new(streamReader);

						//JsonSerializer serializer = new();
						//obj = serializer.Deserialize<T>(jsonReader);

					}
					catch (Exception e)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new ObjectResponse<string>(response);
						}

						Logger.Error(e);
						continue;
					}

					if (obj is null)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new ObjectResponse<string>(response);
						}
						Logger.Error("obj is null");

						continue;
					}

					return new ObjectResponse<string>(response, obj);
				}
			}

			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries + request.AbsoluteUri + "失败的请求" + request);

			return null;
		}

		public async Task<BasicResponse?> Post<T>(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, T? data = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 0) where T : class
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				using HttpResponseMessage? response = await PrivatePost(request, headers, data, referer, requestOptions).ConfigureAwait(false);

				if (response == null)
				{
					continue;
				}

				if (response.StatusCode.IsRedirectionCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
					{
						return new BasicResponse(response);
					}

					break;
				}

				if (response.StatusCode.IsClientErrorCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
					{
						return new BasicResponse(response);
					}

					break;
				}

				if (response.StatusCode.IsServerErrorCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
					{
						return new BasicResponse(response);
					}

					continue;
				}

				return new BasicResponse(response);
			}

			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries + request.AbsoluteUri + "失败的请求" + request);

			return null;
		}

		public async Task<BasicResponse?> Head(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 0)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				using HttpResponseMessage? response = await PrivateHead(request, headers, referer, requestOptions).ConfigureAwait(false);

				if (response == null)
				{
					continue;
				}

				if (response.StatusCode.IsRedirectionCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
					{
						return new BasicResponse(response);
					}

					break;
				}

				if (response.StatusCode.IsClientErrorCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
					{
						return new BasicResponse(response);
					}

					break;
				}

				if (response.StatusCode.IsServerErrorCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
					{
						return new BasicResponse(response);
					}

					continue;
				}

				return new BasicResponse(response);
			}

			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries + request.AbsoluteUri + "失败的请求" + request);

			return null;
		}
		/// <summary>
		/// 获取STRING
		/// </summary>
		/// <param name="request">请求URL</param>
		/// <param name="headers">请求头</param>
		/// <param name="referer"></param>
		/// <param name="requestOptions"></param>
		/// <param name="maxTries">最多重试次数</param>
		/// <param name="rateLimitingDelay">失败后等待多久重试</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>	
		public async Task<ObjectResponse<String>?> GetToString(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 0)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				StreamResponse? response = await GetToStream(request, headers, referer, requestOptions | ERequestOptions.ReturnClientErrors, 1, rateLimitingDelay).ConfigureAwait(false);

				if (response?.Content == null)
				{
					// Request timed out, try again
					continue;
				}

				await using (response.ConfigureAwait(false))
				{
					if (response.StatusCode.IsRedirectionCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
						{
							return new ObjectResponse<String>(response);
						}

						break;
					}

					if (response.StatusCode.IsClientErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
						{
							return new ObjectResponse<String>(response);
						}

						break;
					}

					if (response.StatusCode.IsServerErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
						{
							return new ObjectResponse<String>(response);
						}

						continue;
					}

					String? obj;

					try
					{
						using StreamReader streamReader = new(response.Content);
						obj = streamReader.ReadToEnd();
						//using JsonTextReader jsonReader = new(streamReader);

						//JsonSerializer serializer = new();
						//obj = serializer.Deserialize<T>(jsonReader);

					}
					catch (Exception e)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new ObjectResponse<string>(response);
						}

						Logger.Error(e);
					 
						continue;
					}

					if (obj is null)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new ObjectResponse<string>(response);
						}
						Logger.Warn("obj is null");

						continue;
					}

					return new ObjectResponse<string>(response, obj);
				}
			}

			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries + request.AbsoluteUri + "失败的请求" + request);

			return null;
		}

		public async Task<ObjectResponse<T>?> GetToJsonObject<T>(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 0)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				StreamResponse? response = await GetToStream(request, headers, referer, requestOptions | ERequestOptions.ReturnClientErrors, 1, rateLimitingDelay).ConfigureAwait(false);

				if (response?.Content == null)
				{
					// Request timed out, try again
					continue;
				}

				await using (response.ConfigureAwait(false))
				{
					if (response.StatusCode.IsRedirectionCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
						{
							return new ObjectResponse<T>(response);
						}

						break;
					}

					if (response.StatusCode.IsClientErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
						{
							return new ObjectResponse<T>(response);
						}

						break;
					}

					if (response.StatusCode.IsServerErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
						{
							return new ObjectResponse<T>(response);
						}

						continue;
					}

					T? obj;

					try
					{
						using StreamReader streamReader = new(response.Content);


						using JsonTextReader jsonReader = new(streamReader);

						JsonSerializer serializer = new();
						obj = serializer.Deserialize<T>(jsonReader);
						//JArray jarr = JArray.Parse(obj.ToString());
					}
					catch (Exception e)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new ObjectResponse<T>(response);
						}

						Logger.Error(e);
						continue;
					}

					if (obj is null)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new ObjectResponse<T>(response);
						}
						Logger.Warn("obj is null");

						continue;
					}

					return new ObjectResponse<T>(response, obj);
				}
			}

			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries + request.AbsoluteUri + "失败的请求" + request);

			return null;
		}

		public async Task<StreamResponse?> GetToStream(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 0)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				HttpResponseMessage? response = await PrivateGet(request, headers, referer, requestOptions, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

				if (response == null)
				{
					// Request timed out, try again
					continue;
				}

				if (response.StatusCode.IsRedirectionCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
					{
						return new  StreamResponse(response);
					}

					break;
				}

				if (response.StatusCode.IsClientErrorCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
					{
						return new StreamResponse(response);
					}

					break;
				}

				if (response.StatusCode.IsServerErrorCode())
				{
					if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
					{
						return new StreamResponse(response);
					}

					continue;
				}

				return new StreamResponse(response, await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
			}
			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries + request.AbsoluteUri+ "失败的请求" + request);
	 

			return null;
		}

		public async Task<HtmlDocumentResponse?> GetToHtmlDocument(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, byte maxTries = MaxTries, int rateLimitingDelay = 500)
		{
			ArgumentNullException.ThrowIfNull(request);

			if (maxTries == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxTries));
			}

			if (rateLimitingDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rateLimitingDelay));
			}

			for (byte i = 0; i < maxTries; i++)
			{
				if ((i > 0) && (rateLimitingDelay > 0))
				{
					await Task.Delay(rateLimitingDelay).ConfigureAwait(false);
				}

				StreamResponse? response = await GetToStream(request, headers, referer, requestOptions | ERequestOptions.ReturnClientErrors, 1, rateLimitingDelay).ConfigureAwait(false);

				if (response?.Content == null)
				{
					// Request timed out, try again
					continue;
				}

				await using (response.ConfigureAwait(false))
				{
					if (response.StatusCode.IsRedirectionCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnRedirections))
						{
							return new HtmlDocumentResponse(response);
						}

						break;
					}

					if (response.StatusCode.IsClientErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnClientErrors))
						{
							return new HtmlDocumentResponse(response);
						}

						break;
					}

					if (response.StatusCode.IsServerErrorCode())
					{
						if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors))
						{
							return new HtmlDocumentResponse(response);
						}

						continue;
					}

					try
					{
						return await HtmlDocumentResponse.Create(response).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						if ((requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnSuccess) && response.StatusCode.IsSuccessCode()) || (requestOptions.HasFlag(ERequestOptions.AllowInvalidBodyOnErrors) && !response.StatusCode.IsSuccessCode()))
						{
							return new HtmlDocumentResponse(response);
						}
						Logger.Error(e); 

					}
				}
			}
			if (IsDebug)
				Logger.Debug("请求超过最大次数限制" + maxTries+"失败的请求" + request);
			return null;
		}


		private async Task<HttpResponseMessage?> PrivatePost<T>(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, T? data = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead) where T : class
		{
			ArgumentNullException.ThrowIfNull(request);

			return await PrivateRequest(request, HttpMethod.Post, headers, data, referer, requestOptions, httpCompletionOption).ConfigureAwait(false);
		}
		private async Task<HttpResponseMessage?> PrivateGet(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
		{
			ArgumentNullException.ThrowIfNull(request);

			return await PrivateRequest<object>(request, HttpMethod.Get, headers, null, referer, requestOptions, httpCompletionOption).ConfigureAwait(false);
		}

		private async Task<HttpResponseMessage?> PrivateHead(Uri request, IReadOnlyCollection<KeyValuePair<string, string>>? headers = null, Uri? referer = null, ERequestOptions requestOptions = ERequestOptions.None, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
		{
			ArgumentNullException.ThrowIfNull(request);

			return await PrivateRequest<object>(request, HttpMethod.Head, headers, null, referer, requestOptions, httpCompletionOption).ConfigureAwait(false);
		}

		private async Task<HttpResponseMessage?> PrivateRequest<T>(Uri request
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

					if(IsDebug)
						Logger.Debug($"{httpMethod} {request}");


					try
					{
						response = await HttpClient.SendAsync(requestMessage, httpCompletionOption).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						Logger.Error(e);

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


				if (IsDebug)
					Logger.Debug($"{response.StatusCode} <- {httpMethod} {request}");


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
						Logger.Error("redirectUri is null");
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
								 
								Logger.Error(nameof(redirectUri.Scheme) + redirectUri.Scheme);

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


			if (IsDebug)
				Logger.Debug($"{response.StatusCode} <- {httpMethod} {request}");


			if (response.StatusCode.IsClientErrorCode())
			{

				//if (isDebug)
				Logger.Error(await response.Content.ReadAsStringAsync().ConfigureAwait(false));


				// Do not retry on client errors
				return response;
			}

			if (requestOptions.HasFlag(ERequestOptions.ReturnServerErrors) && response.StatusCode.IsServerErrorCode())
			{

				Logger.Error(await response.Content.ReadAsStringAsync().ConfigureAwait(false));


				// Do not retry on server errors in this case
				return response;
			}

			using (response)
			{

				if (IsDebug)
					Logger.Debug(await response.Content.ReadAsStringAsync().ConfigureAwait(false));


				return null;
			}
		}

	}
}
