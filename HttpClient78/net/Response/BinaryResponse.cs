using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www778878net.Net.Response
{
	public sealed class BinaryResponse : BasicResponse
	{
		
		public IReadOnlyCollection<byte>? Content => Bytes;

		private readonly byte[]? Bytes;

		public BinaryResponse(BasicResponse basicResponse, byte[] bytes) : this(basicResponse) => Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

		public BinaryResponse(BasicResponse basicResponse) : base(basicResponse) => ArgumentNullException.ThrowIfNull(basicResponse);
	}

}
