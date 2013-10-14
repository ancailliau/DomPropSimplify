using System;
using System.Collections.Generic;
using System.Linq;

namespace DomPropSimplify
{
	public class Reducer {

		public DNFOr Formula {
			get;
			set;
		}

		public List<SmartFact> Facts {
			get;
			set;
		}

		public HashSet<DNFOr> Solutions {
			get;
			set;
		}

		public DNFOr BestSolution {
			get;
			set;
		}

		public int Verbose {
			get; set;
		}

		public Reducer (DNFOr formula, List<SmartFact> facts, int verbose = 0, int rounds = -1, bool heuristically = false)
		{
			Verbose = verbose;

			Formula = formula;
			Facts = facts;

			Solutions = new HashSet<DNFOr> ();

			ReduceExplicit (rounds, heuristically);
		}

		public static List<DNFOr> RemoveConsequent (DNFOr ff, SmartFact fact)
		{
			var solutions = new List<DNFOr> ();
			
			if (ff.Clauses.All (x => !fact.Antecedant.Terminals.IsSubsetOf (x.Terminals))) {
				return solutions;
			}

			foreach (var clause in ff.Clauses) {
				foreach (var newClause in RemoveConsequent (clause, fact)) {
					solutions.Add (ff.Replace (clause, newClause).Simplify ());
				}
			}

			return solutions;
		}

		public static List<DNFAnd> RemoveConsequent (DNFAnd ff, SmartFact fact) 
		{
			var solutions = new List<DNFAnd> ();

			foreach (var consequent in fact.Consequents) {
				// all terminals of the antecedant and consequent are contained in the clause
				if (!consequent.Terminals.IsSubsetOf (ff.Terminals)
				    | !fact.Antecedant.Terminals.IsSubsetOf (ff.Terminals))
					continue;

				var solution = ff.Copy ();

				// remove them
				foreach (var terminal in consequent.Terminals) {
					solution.Terminals.Remove (terminal);
				}

				solutions.Add (solution);
			}

			return solutions;
		}
		
		public static List<DNFOr> AddConsequent (DNFOr ff, SmartFact fact)
		{
			var solutions = new List<DNFOr> ();

			if (ff.Clauses.All (x => !fact.Antecedant.Terminals.IsSubsetOf (x.Terminals))) {
				return solutions;
			}

			foreach (var clause in ff.Clauses) {
				foreach (var newClause in AddConsequent (clause, fact)) {
					solutions.Add (ff.Replace (clause, newClause).Simplify ());
				}
			}

			return solutions;
		}

		public static List<DNFAnd> AddConsequent (DNFAnd ff, SmartFact fact)
		{
			var solutions = new List<DNFAnd> ();
			if (!fact.Antecedant.Terminals.IsSubsetOf (ff.Terminals))
				return solutions;

			foreach (var consequent in fact.Consequents) {
				// Formula already contains consequent terminals
				if (consequent.Terminals.IsSubsetOf (ff.Terminals))
					continue;

				var solution = ff.Copy ();

				// add them
				foreach (var terminal in consequent.Terminals) {
					solution.Terminals.Add (terminal);
				}

				solutions.Add (solution);
			}

			return solutions;
		}

		void ReduceExplicit (int maxRounds = -1, bool heuristically = false)
		{
			bool done;
			HashSet<DNFOr> roundSolutions, previousRound;
			DateTime start = DateTime.Now, end = DateTime.Now;
			int roundCount, roundNb = 0;

			Solutions.Add (Formula);
			roundSolutions = new HashSet<DNFOr> (Solutions);

			done = false;
			while (!done && (maxRounds <= 0 | roundNb < maxRounds)) {
				var bestCandidate = Solutions.OrderBy (x => x.Length).First ();
				if (bestCandidate.Length < 2) 
					break;

				done = true;
				previousRound = roundSolutions;
				roundSolutions = new HashSet<DNFOr> ();
				roundCount = previousRound.Count ();

				if (Verbose > 0) {
					Console.WriteLine ("Round " + (roundNb) + " : " + roundCount);
					Console.WriteLine ("Best candidate (" + bestCandidate.Length +") : " + bestCandidate );
				}
				if (Verbose > 1) {
					start = DateTime.Now;
				}

				roundNb++;

				var formulas = previousRound.ToList ();
				for (int i = 0; i < formulas.Count; i++) {

					var formula = formulas [i];

					for (int j = 0; j < Facts.Count; j++) {
						var fact = Facts [j];
						var list = AddConsequent (formula, fact);
						for (int k = 0; k < list.Count; k++) {
							var solution = list [k];
							if (!Solutions.Contains (solution) & !roundSolutions.Contains (solution)) {

								if (Verbose > 2) {									
									Console.WriteLine ("");
									Console.WriteLine ("Adding terminals");
									Console.WriteLine (fact);
									Console.WriteLine ();
									Console.WriteLine ("   " + formula);
									Console.WriteLine ("=> " + solution);
								}

								roundSolutions.Add (solution);
								done = false;
							}
						}

						list = RemoveConsequent (formula, fact);
						for (int k = 0; k < list.Count; k++) {
							var solution = list [k];
							if (!Solutions.Contains (solution) & !roundSolutions.Contains (solution)) {

								if (Verbose > 2) {
									Console.WriteLine ("");
									Console.WriteLine ("Removing terminals");
									Console.WriteLine (fact);
									Console.WriteLine ();
									Console.WriteLine ("   " + formula);
									Console.WriteLine ("=> " + solution);
								}

								roundSolutions.Add (solution);
								done = false;
							}
						}
					}
				}

				if (Verbose > 1) {
					end = DateTime.Now;
					Console.WriteLine (string.Format ("Time for formula : {0}", (end - start)));
				}

				foreach (var solution in roundSolutions) {
					Solutions.Add (solution);
				}

				BestSolution = Solutions.OrderBy (x => x.Length).First ();

				if (heuristically & roundNb > 1 & roundSolutions.Count > 0 & previousRound.Count > 0) {
					var roundBest = roundSolutions.OrderBy (x => x.Length).First ();
					var previousRoundBest = previousRound.OrderBy (x => x.Length).First ();

					if (roundBest.Length >= previousRoundBest.Length) {
						if (Verbose > 1) {
							Console.WriteLine ("Solution did not improve (stopping search)");
						}
						return;
					} else {
						if (Verbose > 1) {
							Console.WriteLine ("Solution improved (continuing search): " + roundBest.Length + " vs " + previousRoundBest.Length);
						}
					}
				}

			}

			BestSolution = Solutions.OrderBy (x => x.Length).First ();

		}

		private class Candidate
		{
			public Fact Fact {
				get;
				set;
			}
			// Just to avoid looking for it a second time
			public DNFAnd FactClause {
				get;
				set;
			}

			public Candidate (Fact fact, DNFAnd factClause)
			{
				this.Fact = fact;
				this.FactClause = factClause;
			}
		}
	}

	public class Fact
	{
		public DNFAnd Antecedant {
			get;
			set;
		}

		public DNFAnd Consequent {
			get;
			set;
		}
		public ISet<string> Vocabulary {
			get {
				return new HashSet<string> (Antecedant.Vocabulary.Union (Consequent.Vocabulary));
			}
		}

		public Fact (DNFAnd antecedant, DNFAnd consequent)
		{
			this.Antecedant = antecedant;
			this.Consequent = consequent;
		}

		public override string ToString ()
		{
			return string.Format ("{0} => {1}", Antecedant, Consequent);
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(Fact))
				return false;
			Fact other = (Fact)obj;
			return Antecedant.Equals (other.Antecedant) 
				&& Consequent.Equals (other.Consequent);
		}

		public override int GetHashCode ()
		{
			unchecked {
				return (Antecedant != null ? Antecedant.GetHashCode () : 0) 
					^ (Consequent != null ? Consequent.GetHashCode () : 0);
			}
		}
		
	}

	public class SmartFact
	{
		public DNFAnd Antecedant {
			get;
			set;
		}

		public List<DNFAnd> Consequents {
			get;
			set;
		}
		public ISet<string> Vocabulary {
			get {
				return new HashSet<string> (Antecedant.Vocabulary.Union (Consequents.SelectMany (x => x.Vocabulary)));
			}
		}

		public SmartFact (DNFAnd antecedant, IEnumerable<DNFAnd> consequents)
		{
			this.Antecedant = antecedant;
			this.Consequents = new List<DNFAnd> (consequents);
		}

		public override string ToString ()
		{
			return string.Join ("\n", Consequents.Select(y => string.Format ("{0} => {1}", Antecedant, y)));
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(SmartFact))
				return false;
			SmartFact other = (SmartFact)obj;
			return Antecedant.Equals (other.Antecedant) 
				&& Enumerable.SequenceEqual (Consequents, other.Consequents);
		}

		public override int GetHashCode ()
		{
			unchecked {
				int hash = 0;
				if (Consequents != null) {
					foreach (var c in Consequents) {
						hash ^= c.GetHashCode ();
					}
				}
				return (Antecedant != null ? Antecedant.GetHashCode () : 0) 
					^ hash;
			}
		}

	}
}
