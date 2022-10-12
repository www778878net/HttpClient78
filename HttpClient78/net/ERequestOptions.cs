using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www778878net.net
{
	public enum ERequestOptions : byte
	{
		None = 0,
		ReturnClientErrors = 1,
		ReturnServerErrors = 2,
		ReturnRedirections = 4,
		AllowInvalidBodyOnSuccess = 8,
		AllowInvalidBodyOnErrors = 16
	}
}
