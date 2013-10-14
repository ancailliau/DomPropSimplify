using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DomPropSimplify.Tests
{
	[TestFixture()]
	public class TestNew
	{
		[TestCase(@"a => b & c --- a")]
		public void TestCase (string input)
		{
			var parser = new FormulaParser ();
			var tuple = (Tuple<List<Fact>, List<Formula>>) parser.Parse (input, null);

			var sfs = DomPropSimplify.CLI.MainClass.BuildSmartFacts (tuple);
			foreach (var sf in sfs) {
				Console.WriteLine ("Adding csq of smartFact : \n" + sf);
				Console.WriteLine ("---");
				var a = Reducer.AddConsequent (tuple.Item2.Single().DNF, sf);
				foreach (var a2 in a) {
					Console.WriteLine (a2);
				}
			}
		}
		
		[TestCase(@"a => b & c --- a & b | a & b & c")]
		public void TestCase2 (string input)
		{
			var parser = new FormulaParser ();
			var tuple = (Tuple<List<Fact>, List<Formula>>) parser.Parse (input, null);

			var sfs = DomPropSimplify.CLI.MainClass.BuildSmartFacts (tuple);
			foreach (var sf in sfs) {
				Console.WriteLine ("Removing csq of smartFact : \n" + sf);
				Console.WriteLine ("---");
				var a = Reducer.RemoveConsequent (tuple.Item2.Single().DNF, sf);
				foreach (var a2 in a) {
					Console.WriteLine (a2);
				}
			}
		}
	}
}

