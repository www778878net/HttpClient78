using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace www778878net.net
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class Utilities78
    { 
        /// <summary>
        /// 客户端出错
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static bool IsClientErrorCode(this HttpStatusCode statusCode) => statusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError;

 
        public static bool IsRedirectionCode(this HttpStatusCode statusCode) => statusCode is >= HttpStatusCode.Ambiguous and < HttpStatusCode.BadRequest;

 
        public static bool IsServerErrorCode(this HttpStatusCode statusCode) => statusCode is >= HttpStatusCode.InternalServerError and < (HttpStatusCode)600;

 
        public static bool IsSuccessCode(this HttpStatusCode statusCode) => statusCode is >= HttpStatusCode.OK and < HttpStatusCode.Ambiguous;

    }
}
