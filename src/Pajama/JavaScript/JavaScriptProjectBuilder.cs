using System.Collections.Generic;
using System.Text;

namespace Pajama.JavaScript
{
	internal class JavaScriptProjectBuilder : ProjectBuilder
	{
		public JavaScriptProjectBuilder()
		{
		}

		protected override SerializerBase CreateSerializer(Node.Class[] classes, List<string> images)
		{
			string gameJs = Util.ReadEmbeddedFile("JavaScript/game.js");
			return new JavaScriptSerializer(classes, gameJs, images);
		}

		protected override void CreateCodeFiles(string targetDir, string codeContents)
		{
			System.IO.File.WriteAllText(targetDir + "\\code.js", codeContents);
			System.IO.File.WriteAllText(targetDir + "\\index.html", this.GetIndexHtml(), UnicodeEncoding.Default);
		}

		private string GetIndexHtml()
		{
			return string.Join("\n", new string[] {
				"<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
				"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"h" + "ttp://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">",
				"",
				"<html xmlns=\"h"+"ttp://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">",
				"	<head>",
				"		<meta content=\"text/html;charset=utf-8\" http-equiv=\"Content-Type\">",
				"		<meta content=\"utf-8\" http-equiv=\"encoding\">",
				"		<title>Game</title>",
				"		<script type=\"text/javascript\" src=\"code.js\"></script>",
				"	</head>",
				"	<body onload=\"setup()\">",
				"		<div id=\"pj_host\" oncontextmenu=\"return false;\"></div>",
				"	</body>",
				"</html>",
				""
			});
		}
	}
}
