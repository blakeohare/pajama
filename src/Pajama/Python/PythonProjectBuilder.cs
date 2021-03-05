using System.Collections.Generic;

namespace Pajama.Python
{
	internal class PythonProjectBuilder : ProjectBuilder
	{
		public PythonProjectBuilder()
			: base()
		{ }

		protected override SerializerBase CreateSerializer(Node.Class[] classes, List<string> images)
		{
			return new PythonSerializer(classes);
		}

		protected override void CreateCodeFiles(string targetDir, string codeContents)
		{
			System.IO.File.WriteAllText(targetDir + "\\game.py", codeContents);
		}
	}
}
