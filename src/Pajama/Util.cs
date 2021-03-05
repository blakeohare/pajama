using System.Text;

namespace Pajama
{
	internal static class Util
	{
		internal static string ReadEmbeddedFile(string path)
		{
			System.Reflection.Assembly assembly = typeof(Util).Assembly;
			string[] foo = assembly.GetManifestResourceNames();
			string assemblyName = assembly.FullName.Split(',')[0].Trim();
			string embeddedPath = assemblyName + "." + path.Replace('/', '.').Replace('\\', '.');
			System.IO.Stream stream = assembly.GetManifestResourceStream(embeddedPath);

			System.Text.StringBuilder output = new StringBuilder();
			int valueRead;
			do
			{
				valueRead = stream.ReadByte();
				if (valueRead != -1)
				{
					output.Append((char)(byte)valueRead);
				}
			} while (valueRead != -1);

			string value = output.ToString();
			if (value.Length >= 3 &&
				value[0] == 239 &&
				value[1] == 187 &&
				value[2] == 191)
			{
				value = value.Substring(3);
			}
			return value;
		}
	}
}
