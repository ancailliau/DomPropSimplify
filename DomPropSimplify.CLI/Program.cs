using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NDesk.Options;

namespace DomPropSimplify.CLI
{
	public class MainClass
	{
		public static HashSet<SmartFact> BuildSmartFacts (Tuple<List<Fact>, List<Formula>> tuple)
		{
			var facts = new HashSet<Fact> ();
			foreach (var fcl in tuple.Item1.ToArray ()) {
				facts.Add (fcl);
			}
			foreach (var fact in tuple.Item1.ToArray ()) {
				foreach (var consequentTerminal in fact.Consequent.Terminals) {
					facts.Add (new Fact (fact.Antecedant, new DNFAnd (consequentTerminal)));
				}
			}
			foreach (var fact in facts.ToArray ()) {
				if (fact.Antecedant.Terminals.Count () == 1 & fact.Consequent.Terminals.Count () == 1) {
					facts.Add (new Fact (new DNFAnd (fact.Consequent.Terminals.Single ().Negate ()), new DNFAnd (fact.Antecedant.Terminals.Single ().Negate ())));
				}
			}
			var smartFacts = new HashSet<SmartFact> ();
			foreach (var f in new HashSet<DNFAnd> (facts.Select (x => x.Antecedant))) {
				var s = new SmartFact (f, facts.Where (x => x.Antecedant.Equals (f)).Select (x => x.Consequent));
				smartFacts.Add (s);
			}
			return smartFacts;
		}

		public static void Main (string[] args)
		{
			bool help   = false;
			bool heuristically = false;
			bool report = false;
			int verbose = 0;
			int rounds = -1;
			var p = new OptionSet () {
				{ "v|verbose",  v => { ++verbose; } },
				{ "rounds=",  v => rounds = int.Parse (v) },
				{ "heuristic",  v => heuristically = true },
				{ "r|report",  v => report = true },
				{ "h|?|help",   v => help = v != null },
			};

			List<string> extra = p.Parse (args);

			string input = "";
			if (extra.Count == 0) {
				input = Console.In.ReadToEnd ();

			} else if (extra.Count == 1) {
				if (!File.Exists (extra[0])) {
					Console.WriteLine ("File {0} does not exists", extra[0]);
					return;
				}
				input = File.ReadAllText (extra[0]);

			} else {
				help = true;
			}

			if (help) {
				p.WriteOptionDescriptions (Console.Out);
				return;
			}

			var parser = new FormulaParser ();
			var tuple = (Tuple<List<Fact>, List<Formula>>) parser.Parse (input, null);

			var smartFacts = BuildSmartFacts (tuple);





			if (verbose > 0) {
				Console.WriteLine ("[ Facts ({0}) ] ---", smartFacts.Sum (x => x.Consequents.Count));
				Console.WriteLine (string.Join ("\n", smartFacts.Select (x => x.ToString ())));
				Console.WriteLine ();

				Console.WriteLine ("[ Formulas ] ---");
				Console.WriteLine (string.Join ("\n", tuple.Item2.Select (x => x.DNF.ToString ())));
				Console.WriteLine ();

			}

			foreach (var formula in tuple.Item2) {
				var dNF = formula.DNF;
				if (verbose > 0) {
					Console.WriteLine ("[ Simplifying {0} ] ---", dNF);
					Console.WriteLine ();
				}

				var reducer = new Reducer (dNF, smartFacts.ToList (), verbose, rounds, heuristically);
				if (verbose > 0) {
					Console.WriteLine ("Best solution found ({1}): {0}", reducer.BestSolution, reducer.BestSolution.Length);
				} else {
					if (report) {
						Console.WriteLine ("Not simplified ({0}): {1}", dNF.Length, dNF);
						Console.WriteLine ("Simplified     ({0}): {1}", reducer.BestSolution.Length, reducer.BestSolution);
						Console.WriteLine ();
					} else {
						Console.WriteLine (reducer.BestSolution);
					}
				}

				if (verbose > 0) {
					Console.WriteLine ();
				}
			}
		}
	}
}
