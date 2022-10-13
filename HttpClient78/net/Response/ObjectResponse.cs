using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www778878net.Net.Response
{
	public sealed class ObjectResponse<T> : BasicResponse
	{
		 
		public T? Content { get; }

		public ObjectResponse(BasicResponse basicResponse, T content) : this(basicResponse) => Content = content ?? throw new ArgumentNullException(nameof(content));

		public ObjectResponse(BasicResponse basicResponse) : base(basicResponse) => ArgumentNullException.ThrowIfNull(basicResponse);
	}
}
