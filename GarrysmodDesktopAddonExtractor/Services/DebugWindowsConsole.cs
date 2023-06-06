using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GarrysmodDesktopAddonExtractor.Services
{
	public class DebugWindowsConsole
	{
#if DEBUG
		[DllImport("Kernel32")]
		public static extern void AllocConsole();

		[DllImport("Kernel32")]
		public static extern void FreeConsole();
#else
		public static void AllocConsole() { }

		public static void FreeConsole() { }
#endif
	}
}
