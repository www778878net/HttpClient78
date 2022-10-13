using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www778878net.Net.Response
{
    public sealed class StreamResponse : BasicResponse, IAsyncDisposable
    {
   
        public Stream? Content { get; }

        
        public long Length { get; }

        private readonly HttpResponseMessage ResponseMessage;

        internal StreamResponse(HttpResponseMessage httpResponseMessage, Stream content) : this(httpResponseMessage) => Content = content ?? throw new ArgumentNullException(nameof(content));

        internal StreamResponse(HttpResponseMessage httpResponseMessage) : base(httpResponseMessage)
        {
            ResponseMessage = httpResponseMessage ?? throw new ArgumentNullException(nameof(httpResponseMessage));
            Length = httpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault();
        }

        public async ValueTask DisposeAsync()
        {
            if (Content != null)
            {
                await Content.DisposeAsync().ConfigureAwait(false);
            }

            ResponseMessage.Dispose();
        }
    }

}
