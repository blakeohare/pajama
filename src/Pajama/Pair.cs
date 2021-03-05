namespace Pajama
{
	internal class Pair<A, B>
	{
		public A First { get; private set; }
		public B Second { get; private set; }

		public Pair(A a, B b)
		{
			this.First = a;
			this.Second = b;
		}
	}
}
