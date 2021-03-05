namespace Pajama
{
	class Program
	{
		static void Main(string[] args)
		{
			args = DebugArgs.MaybeReplaceWithDebugArgs(args);

			if (args.Length == 0)
			{
				string[] message = new string[] {
					"Usage:",
					"  pj -source {source folder} -target {target folder} -platform {js|py}",
					"",
					"-- List of arguments --",
					"",
					" -source, -s:",
					"    Source folder. Can be relative or absolute. All .pj files will be compiled",
					"    in this folder (recursive).",
					"",
					" -target, -t:",
					"    Target folder. Output files will be saved here. Previous files will be",
					"    overwritten, but extraneous files will not be deleted.",
					"    If the folder does not exist, it will be created.",
					"",
					" -platform, -p:",
					"    Target platform. Valid values and aliases are py, python, js, and",
					"    javascript.",
					"",
					" -directories, -d:",
					"    [Optional]",
					"    Support file directories. (comma-delimited) All non-code files in these",
					"    folders will be copied to the output directory. In JavaScript games, all",
					"    of the images will automatically get preloaded and all text will be",
					"    included in the JS file itself as strings.",
					"",
					" -files, -f:",
					"    [Optional]",
					"    Support files located in the root source directory to be included in",
					"    output. (comma-delimited)",
					"",
					" -copyroot, -c:",
					"    [Optional]",
					"    All files in the root source directory will be copied, but will not be",
					"    preloaded in JavaScript if it is not specifically included with -files.",
					"    Good for readmes, license documents, etc."
				};
				foreach (string line in message)
				{
					System.Console.WriteLine(line);
				}
				return;
			}

			bool copyAllRoot = false;
			string sourceFolder = null;
			string targetFolder = null;
			string rawSupportFolders = "";
			string rawRootSupportFiles = "";
			string outputFormat = null;

			for (int i = 0; i < args.Length; ++i)
			{
				switch (args[i].ToLowerInvariant())
				{
					case "-copyroot":
					case "-c":
						copyAllRoot = true;
						break;

					case "-source":
					case "-s":
						sourceFolder = args[++i];
						break;

					case "-target":
					case "-t":
						targetFolder = args[++i];
						break;

					case "-directories":
					case "-d":
						rawSupportFolders = args[++i];
						break;

					case "-files":
					case "-f":
						rawRootSupportFiles = args[++i];
						break;

					case "-platform":
					case "-p":
						outputFormat = args[++i];
						break;

					default:
						System.Console.WriteLine("Unrecognized flag: " + args[i]);
						return;
				}
			}

			if (sourceFolder == null)
			{
				System.Console.WriteLine("Source folder argument is missing.");
				return;
			}

			if (targetFolder == null)
			{
				System.Console.WriteLine("Target folder argument is missing.");
				return;
			}

			if (outputFormat == null)
			{
				System.Console.WriteLine("Platform argument is missing.");
				return;
			}

			string[] supportFolders = rawSupportFolders.Split(',');
			supportFolders = supportFolders.Length == 1 && supportFolders[0].Length == 0 ? new string[0] : supportFolders;

			string[] rootSupportFiles = rawRootSupportFiles.Split(',');
			rootSupportFiles = rootSupportFiles.Length == 1 && rootSupportFiles[0].Length == 0 ? new string[0] : rootSupportFiles;

			if (!System.IO.Directory.Exists(sourceFolder))
			{
				System.Console.WriteLine("Input folder doesn't exist.");
				return;
			}

			ProjectBuilder projectBuilder;
			switch (outputFormat.Trim().ToUpperInvariant())
			{
				case "JS":
				case "JAVASCRIPT":
					projectBuilder = new JavaScript.JavaScriptProjectBuilder();
					break;
				case "PY":
				case "PYTHON":
					projectBuilder = new Python.PythonProjectBuilder();
					break;
				default:
					System.Console.WriteLine("Unrecognized output format. Choose one of: js, py");
					return;
			}

#if DEBUG
			projectBuilder.BuildProject(sourceFolder, targetFolder, copyAllRoot, supportFolders, rootSupportFiles);
#else
			try
			{
				projectBuilder.BuildProject(sourceFolder, targetFolder, copyAllRoot, supportFolders, rootSupportFiles);
			}
			catch (System.Exception e)
			{
				System.Console.WriteLine(e.Message);
			}
#endif

		}
	}
}
