using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DomPropSimplify.Tests
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void Test1 ()
		{
			var formula = new DNFOr (new DNFAnd (new DNFVar ("a"), new DNFVar ("b"), new DNFVar ("c")));

			var facts = new List<Fact> ();
			facts.Add (new Fact (
				new DNFAnd (new DNFVar ("a")),
				new DNFAnd (new DNFVar ("b"))
				));
			facts.Add (new Fact (
				new DNFAnd (new DNFVar ("a")),
				new DNFAnd (new DNFVar ("c"))
				));

			var reducer = new Reducer (formula, facts);

			Console.WriteLine (reducer.BestSolution);
			Assert.AreEqual (new DNFOr(new DNFAnd (new DNFVar ("a"))), reducer.BestSolution);
		}
		
		[Test()]
		public void Test2 ()
		{
			var formula = new DNFOr (new DNFAnd (new DNFVar ("a"), new DNFVar ("b"), new DNFVar ("c")));

			var facts = new List<Fact> ();
			facts.Add (new Fact (
				new DNFAnd (new DNFVar ("a"), new DNFVar ("b")),
				new DNFAnd (new DNFVar ("c"))
				));

			var reducer = new Reducer (formula, facts);

			Console.WriteLine (reducer.BestSolution);
			Assert.AreEqual (new DNFOr(new DNFAnd (new DNFVar ("a"), new DNFVar ("b"))), reducer.BestSolution);
		}

		[Test()]
		public void Test3 ()
		{
			var formula = new DNFOr (new DNFAnd (new DNFVar ("a"), new DNFVar ("b"), new DNFVar ("c")));

			var facts = new List<Fact> ();
			facts.Add (new Fact (
				new DNFAnd (new DNFVar ("a")),
				new DNFAnd (new DNFVar ("b"))
				));

			var reducer = new Reducer (formula, facts);

			Console.WriteLine (reducer.BestSolution);
			Assert.AreEqual (new DNFOr(new DNFAnd (new DNFVar ("a"), new DNFVar ("c"))), reducer.BestSolution);
		}

		[Test()]
		public void Test4 ()
		{
			var formula = new DNFOr (new DNFAnd (new DNFVar ("a")), new DNFAnd(new DNFVar ("b"), new DNFVar ("c")));

			var facts = new List<Fact> ();
			facts.Add (new Fact (
				new DNFAnd ( new DNFVar ("b")),
				new DNFAnd (new DNFVar ("c"))
				));

			var reducer = new Reducer (formula, facts);

			Console.WriteLine (reducer.BestSolution);
			Assert.AreEqual (new DNFOr(new DNFAnd (new DNFVar ("a")), new DNFAnd (new DNFVar ("b"))), reducer.BestSolution);
		}
	}
}

