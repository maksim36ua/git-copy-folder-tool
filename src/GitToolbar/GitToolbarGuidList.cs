using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitToolbar
{
	static class GuidList
	{
		public const string guidComboBoxPkgString = "ed30f6ee-6706-4241-8f2b-6a6f27fd83cd";
		public const string guidComboBoxCmdSetString = "ecd0587b-f741-47ae-ae52-14ee47340eb1";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Guid guidComboBoxPkg = new Guid(guidComboBoxPkgString);
		public static readonly Guid guidComboBoxCmdSet = new Guid(guidComboBoxCmdSetString);
	};
}
