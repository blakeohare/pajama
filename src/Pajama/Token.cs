namespace Pajama
{
	internal class Token
	{
		public string Value { get; private set; }
		public string LineValue { get; private set; }
		public string File { get; private set; }
		public int Line { get; private set; }
		public int Col { get; private set; }

		public Token(string value, string file, int line, int col, string[] lineValues)
		{
			this.Value = value;
			this.File = file;
			this.Line = line;
			this.Col = col;
			if (lineValues.Length >= line && line >= 0)
			{
				this.LineValue = lineValues[line - 1];
			}
			else
			{
#if DEBUG
				throw new System.Exception("No original file info for this token. Why?");
#endif
				this.LineValue = null;
			}
		}

		private string errorMessageCodeLine = null;
		private string errorMessagePointerLine = null;
		private void GenerateErrorMessageCodeLine()
		{
			if (errorMessageCodeLine != null && errorMessagePointerLine != null) return;

			string codeLine = "";
			string pointerLine = "";
			int col = this.Col;

			for (int i = 0; i < this.LineValue.Length; ++i)
			{
				string c = this.LineValue[i] + "";
				if (c == "\t")
				{
					c = "    ";
				}

				codeLine += c;

				if (i < this.Col)
				{
					for (int j = c.Length - 1; j >= 0; --j)
					{
						pointerLine += ' ';
					}
				}
				else if (i == this.Col)
				{
					pointerLine += '^';
				}
			}

			if (pointerLine.Length == 0 || pointerLine[pointerLine.Length - 1] != '^')
			{
				pointerLine += '^';
			}
			this.errorMessageCodeLine = codeLine;
			this.errorMessagePointerLine = pointerLine;
		}

		public string ErrorMessageCodeLine
		{
			get
			{
				this.GenerateErrorMessageCodeLine();
				return this.errorMessageCodeLine;
			}
		}

		public string ErrorMessagePointerLine
		{
			get
			{
				this.GenerateErrorMessageCodeLine();
				return this.errorMessagePointerLine;
			}
		}
	}
}
