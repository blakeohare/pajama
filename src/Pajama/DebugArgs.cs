using System.Collections.Generic;

namespace Pajama
{
	// Actually easier than digging through the project compile properties menu...
	internal static class DebugArgs
	{
		private static readonly bool USE_DEBUG_ARGS = false;

		private static readonly string OUTPUT_FORMAT = "js";

		private static readonly string INPUT_FOLDER = "C:\\Things\\Pajama\\Demos\\Minesweeper";

		private static readonly string OUTPUT_FOLDER = "C:\\Users\\Blake\\Desktop\\" + OUTPUT_FORMAT + "_out";

		private static readonly string[] ASSET_DIRECTORIES = new string[] { };

		private static readonly string[] ASSET_FILES = new string[] { "image_sheet.png" };

		public static string[] MaybeReplaceWithDebugArgs(string[] actualArgs)
		{

			if (USE_DEBUG_ARGS)
			{
				List<string> args = new List<string>() {
					"-source", INPUT_FOLDER,
					"-target", OUTPUT_FOLDER };

				if (ASSET_DIRECTORIES.Length > 0)
				{
					args.Add("-directories");
					args.Add(string.Join(",", ASSET_DIRECTORIES));
				}

				if (ASSET_FILES.Length > 0)
				{
					args.Add("-files");
					args.Add(string.Join(",", ASSET_FILES));
				}

				args.Add("-platform");
				args.Add(OUTPUT_FORMAT);

				return args.ToArray();
			}

			return actualArgs;
		}
	}
}
