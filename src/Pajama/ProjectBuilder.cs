using System;
using System.Collections.Generic;

namespace Pajama
{
	internal abstract class ProjectBuilder
	{
		public void BuildProject(string sourceFolder, string fullyQualifiedOutputDirectory, bool copyAllRootFiles, string[] supportFileFolders, string[] supportFiles)
		{
			string targetDir = fullyQualifiedOutputDirectory;
			this.EnsureFolderExists(targetDir);

			List<string> code = new List<string>();
			List<string> images = new List<string>();
			List<string> sounds = new List<string>();
			List<string> text = new List<string>();

			this.CopySupportOver(sourceFolder, targetDir, supportFileFolders, supportFiles, copyAllRootFiles, code, images, sounds, text);

			List<string> imagesForLoading = new List<string>();
			foreach (string image in images)
			{
				imagesForLoading.Add(image.Substring(targetDir.Length + 1));
			}

			Node.Class[] classes = this.Parse(sourceFolder, code);
			SerializerBase codeSerializer = this.CreateSerializer(classes, imagesForLoading);

			string codeContents = codeSerializer.Serialize();

			this.CreateCodeFiles(targetDir, codeContents);
		}

		protected abstract SerializerBase CreateSerializer(Node.Class[] classes, List<string> images);
		protected abstract void CreateCodeFiles(string targetDir, string codeContents);

		protected Pajama.Node.Class[] Parse(string sourceFolder, List<string> codeFiles)
		{
			Dictionary<string, string> files = new Dictionary<string, string>()
			{
				{ "$Core.pj", Util.ReadEmbeddedFile("PJ.pj") }
			};

			foreach (string codeFile in codeFiles)
			{
				string relativePath = codeFile.Substring(sourceFolder.Length + 1);
				files[relativePath] = System.IO.File.ReadAllText(codeFile);
			}

			Token[] tokens = new Tokenizer(files).Tokenize();

			Parser parser = new Parser(new Tokens(tokens));
			parser.Parse();

			Node.Class[] classes = parser.Classes;

			int programStartCount = 0;
			foreach (Node.Class cls in classes)
			{
				if (cls.SimpleName == "Program")
				{
					Node.ClassMember start;
					if (cls.Members.TryGetValue("init", out start))
					{
						Node.Method startMethod = start as Node.Method;
						if (startMethod != null)
						{
							if (startMethod.Type == Node.ZType.VOID && startMethod.Args.Length == 0)
							{
								programStartCount++;
							}
						}
					}
				}
			}

			if (programStartCount != 1)
			{
				throw new Exception("There must be exactly 1 class named Program that has a method called start that contains no args and has a void return type.");
			}

			TypeResolver typeResolver = new TypeResolver(classes);

			typeResolver.DoYourThing();

			return classes;
		}

		protected void EnsureFolderExists(string folder)
		{
			if (!System.IO.Directory.Exists(folder))
			{
				System.IO.Directory.CreateDirectory(folder);
			}
		}

		private string[] GetDirectories(string path)
		{
			List<string> output = new List<string>();
			foreach (string file in System.IO.Directory.GetDirectories(path))
			{
				output.Add(file.Substring(path.Length + 1));
			}
			return output.ToArray();
		}

		private string[] GetFiles(string path)
		{
			List<string> output = new List<string>();
			foreach (string file in System.IO.Directory.GetFiles(path))
			{
				output.Add(file.Substring(path.Length + 1));
			}
			return output.ToArray();
		}

		protected void CopySupportOver(string sourceFolder, string targetDir, string[] supportFileFolders, string[] supportFiles, bool copyOthersInRootWithoutManifest, List<string> code, List<string> images, List<string> sounds, List<string> text)
		{
			HashSet<string> supportFolders = new HashSet<string>();
			foreach (string supportFolder in supportFileFolders)
			{
				supportFolders.Add(supportFolder);
				string sourcePath = sourceFolder + '\\' + supportFolder;
				string targetPath = targetDir + '\\' + supportFolder;
				this.CopyThis(sourcePath, targetPath, false, code, images, sounds, text);
			}

			foreach (string nonSupportFolder in this.GetDirectories(sourceFolder))
			{	
				if (!supportFolders.Contains(nonSupportFolder))
				{
					this.CopyThis(sourceFolder + '\\' + nonSupportFolder, targetDir + '\\' + nonSupportFolder, true, code, images, sounds, text);
				}
			}

			HashSet<string> copied = new HashSet<string>();
			foreach (string supportFile in supportFiles)
			{
				copied.Add(supportFile);
				string sourceFile = sourceFolder + '\\' + supportFile;
				string targetFile = targetDir + '\\' + supportFile;
				if (!this.IsCode(targetFile))
				{
					System.IO.File.Copy(sourceFile, targetFile, true);
				}
				this.ApplyFileToManifest(this.IsCode(targetFile) ? sourceFile : targetFile, code, images, sounds, text);
			}

			foreach (string nonSupportFile in this.GetFiles(sourceFolder))
			{
				if (copyOthersInRootWithoutManifest || this.IsCode(nonSupportFile))
				{
					if (!copied.Contains(nonSupportFile))
					{
						string sourceFile = sourceFolder + '\\' + nonSupportFile;
						string targetFile = targetDir + '\\' + nonSupportFile;
						if (!this.IsCode(targetFile))
						{
							System.IO.File.Copy(sourceFile, targetFile, true);
						}
						else
						{
							this.ApplyFileToManifest(this.IsCode(targetFile) ? sourceFile : targetFile, code, images, sounds, text);
						}

					}
				}
			}
		}

		private bool IsCode(string file)
		{
			return file.ToUpperInvariant().EndsWith(".PJ");
		}

		private void ApplyFileToManifest(string file, List<string> code, List<string> images, List<string> sounds, List<string> text)
		{
			string[] parts = file.Split('.');
			string extension = "";
			if (parts.Length > 1)
			{
				extension = parts[parts.Length - 1].ToUpperInvariant();
			}

			switch (extension)
			{
				case "PJ":
					code.Add(file);
					break;

				case "PNG":
				case "JPG":
				case "JPEG":
					images.Add(file);
					break;

				case "MP3":
				case "OGG":
				case "WAV":
					sounds.Add(file);
					break;

				default:
					text.Add(file);
					break;
			}
		}

		protected void CopyThis(string sourcePath, string targetPath, bool codeOnly, List<string> code, List<string> images, List<string> sounds, List<string> text)
		{
			bool folderCreated = false;
			foreach (string file in this.GetFiles(sourcePath))
			{
				string sourceFile = sourcePath + '\\' + file;
				string targetFile = targetPath + '\\' + file;
				if (this.IsCode(targetFile))
				{
					this.ApplyFileToManifest(sourceFile, code, images, sounds, text);
				}
				else if (!codeOnly)
				{
					if (!folderCreated)
					{
						this.EnsureFolderExists(targetPath);
						folderCreated = true;
					}
					System.IO.File.Copy(sourceFile, targetFile, true);
					this.ApplyFileToManifest(targetFile, code, images, sounds, text);
				}
			}

			foreach (string folder in this.GetDirectories(sourcePath))
			{
				string sourceSubdir = sourcePath + '\\' + folder;
				string targetSubdir = targetPath + '\\' + folder;
				this.CopyThis(sourceSubdir, targetSubdir, codeOnly, code, images, sounds, text);
			}
		}

	}
}
