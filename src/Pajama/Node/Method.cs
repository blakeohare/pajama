namespace Pajama.Node
{
	internal class Method : ClassMember
	{
		public Pair<ZType, string>[] Args { get; private set; }
		public Executable[] Code { get; private set; }

		public Method(string name, ZType returnType, Pair<ZType, string>[] args, Executable[] code, Class parent, bool isStatic)
			: base(name, returnType, parent, isStatic)
		{
			this.Args = args;
			this.Code = code;
		}
	}
}
