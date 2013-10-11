using NUnit.Framework;
using System;

namespace DomPropSimplify.Tests
{
	[TestFixture()]
	public class NUnitTestClass
	{
		[Test()]
		public void Test1 ()
		{
			var formula = new DNFOr (new DNFAnd (new DNFVar ("a")), new DNFAnd(new DNFVar ("b"), new DNFVar ("c")));
			Assert.AreEqual (new DNFOr (new DNFAnd (new DNFVar ("a")), new DNFAnd(new DNFVar ("b"), new DNFVar ("c"))), 
			                 formula.Simplify ());
		}

		[Test()]
		public void Test2 ()
		{
			var formula = new DNFOr (new DNFAnd (new DNFVar ("b")), new DNFAnd(new DNFVar ("b"), new DNFVar ("c")));
			Assert.AreEqual (new DNFOr(new DNFAnd (new DNFVar ("b"))), formula.Simplify ());
		}

		[Test()]
		public void Test3 ()
		{
			var formula = new DNFOr (new DNFAnd(new DNFVar ("a"), new DNFVar ("a"), new DNFVar ("b"), new DNFVar ("c")));
			Assert.AreEqual (new DNFOr (new DNFAnd(new DNFVar ("a"), new DNFVar ("b"), new DNFVar ("c"))), 
			                 formula.Simplify ());
		}

		[Test()]
		public void Test4 ()
		{
			var f1 = new Fact (new DNFAnd(new DNFVar ("a")), new DNFAnd(new DNFVar ("b"), new DNFVar ("c")));
			var f2 = new Fact (new DNFAnd(new DNFVar ("a")), new DNFAnd(new DNFVar ("b"), new DNFVar ("c")));
			Assert.AreEqual (f1.GetHashCode (), f2.GetHashCode ());
			Assert.AreEqual (f1, f2);
		}
	}
}

