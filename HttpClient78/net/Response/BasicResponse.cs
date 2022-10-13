using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace www778878net.Net.Response
{
    public class BasicResponse
    {
        
        public Uri FinalUri { get; }

        
        public HttpStatusCode StatusCode { get; }

        public BasicResponse(HttpResponseMessage httpResponseMessage)
        {
            ArgumentNullException.ThrowIfNull(httpResponseMessage);

            FinalUri = httpResponseMessage.Headers.Location ?? httpResponseMessage.RequestMessage?.RequestUri ?? throw new InvalidOperationException();
            StatusCode = httpResponseMessage.StatusCode;
        }

        public BasicResponse(BasicResponse basicResponse)
        {
            ArgumentNullException.ThrowIfNull(basicResponse);

            FinalUri = basicResponse.FinalUri;
            StatusCode = basicResponse.StatusCode;
        }
    }

}
