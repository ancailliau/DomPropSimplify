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

		public List<Fact> Facts {
			get;
			set;
		}

		public List<DNFOr> Solutions {
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

		public Reducer (DNFOr formula, List<Fact> facts, bool heuristic = false, int verbose = 0)
		{
			Verbose = verbose;

			Formula = formula;
			Facts = facts;

			Solutions = new List<DNFOr> ();

			if (heuristic) {
				ReduceHeuristically (0);
				var best1 = BestSolution;
				ReduceHeuristically (1);
				var best2 = BestSolution;

				BestSolution = best1.Length > best2.Length ? best2 : best1;

			} else {
				ReduceExplicit ();
			}
		}

		void ReduceHeuristically (int variant)
		{
			// The list of all possible solutions, starting with the formula itself obviously
			Solutions.Add (Formula);

			bool done = false;
			var roundSolutions = new List<DNFOr> (Solutions);
			
			if (Verbose > 0) {
				Console.WriteLine ("** First pass **");
				Console.WriteLine ();
			}

			// First, try to expand it as much as possible
			if (variant > 0) {
			while (!done) {
				done = true;
				roundSolutions = new List<DNFOr> ();

				// We try to simplify all already-found solutions
				foreach (var ff in Solutions.ToList ()) {
					var candidates = 
						from clause in ff.Clauses
							from f in Facts
							where f.Antecedant.Terminals.All (clause.Terminals.Contains)
							select f;

					foreach (var candidate in candidates) {
						var solution = ff.Copy ();
						var clausesToExtend = 
							from x in solution.Clauses
								where candidate.Antecedant.Terminals.All (x.Terminals.Contains)
								select x;
						foreach (var clauseToExtend in clausesToExtend) {
							foreach (var v in candidate.Consequent.Terminals) {
								clauseToExtend.Terminals.Add (v);
							}

							solution = solution.Simplify ();
							if (!Solutions.Contains (solution) & !roundSolutions.Contains (solution)) {
								roundSolutions.Add (solution);

								if (Verbose > 1) {
									Console.WriteLine (string.Format ("New candidate found: {0}", solution));
									Console.WriteLine ("<- Adding '{0}' because '{1}' is present", 
									                   string.Join (",", candidate.Consequent.Terminals.Select (x => x.ToString ())),
									                   string.Join (",", candidate.Antecedant.Terminals.Select(y => y.ToString ())));
									Console.WriteLine ();
								}

								done = false;
							}
						}
					}


				if (roundSolutions.Count() > 0) {
					Solutions.Clear ();
						Solutions.Add (roundSolutions.OrderBy (x => x.Length).Last ());
						if (Verbose > 0) {
							Console.WriteLine ("Keeping {0} as longest candidate ({1})", roundSolutions.OrderBy (x => x.Length).Last (), roundSolutions.OrderBy (x => x.Length).Last ().Length);
						}
				}
			}
			}
			}

			if (Verbose > 0) {
				Console.WriteLine ();
				Console.WriteLine ("** Second pass **");
				Console.WriteLine ();
			}

			done = false;

			// Second, try to reduce it as much as possible
			while (!done) {
				done = true;
				roundSolutions = new List<DNFOr> ();

				// We try to simplify all already-found solutions
				foreach (var currentFormula in Solutions.ToList ()) {
					// A candidate is a fact such that the ff formula can be rewriten using the antecedant. So
					// - there is a clause in the consequent that is contained in a clause of the formula. 
					//   e.g. Fact: a => b&c, Formula: a&b&c because b&c are in the formula
					// - the antecedant is contained in the same clause
					//   e.g. Fact: a => b&c, Formula: a&b&c because a is in the formula
					var candidates = 
						from clause in currentFormula.Clauses
							from fact in Facts
							where fact.Consequent.Terminals.All (clause.Terminals.Contains) 
								& fact.Antecedant.Terminals.All (clause.Terminals.Contains)
							select fact;

					// For each candidate, we add a new potential solution where we remove the matched variables
					foreach (var candidate in candidates) {
						var solution = currentFormula.Copy ();
						var clausesToSimplify = 
							from c in solution.Clauses
								where candidate.Consequent.Terminals.All (c.Terminals.Contains) 
								& candidate.Antecedant.Terminals.All (c.Terminals.Contains)
								select c;

						foreach (var c in clausesToSimplify) {
							foreach (var v in candidate.Consequent.Terminals) {
								c.Terminals.Remove (v);
							}
						}

						solution = solution.Simplify ();
							if (!roundSolutions.Contains (solution)) {
								roundSolutions.Add (solution);

								// We found a new solution, let's try again because the fixpoint has not been reached
								if (Verbose > 1) {
									Console.WriteLine (string.Format ("New candidate found: {0}", solution));
									Console.WriteLine ("<- Removing '{0}' because '{1}' is present", 
									                   string.Join (",", candidate.Consequent.Terminals.Select (x => x.ToString ())),
								                   	   string.Join (",", candidate.Antecedant.Terminals.Select(y => y.ToString ())));
									Console.WriteLine ();
								}

								done = false;
							}
					}
				}

				if (roundSolutions.Count() > 0) {
					Solutions.Clear ();
					Solutions.Add (roundSolutions.OrderBy (x => x.Length).First ());
					if (Verbose > 0) {
						Console.WriteLine ("Keeping {0} as shortest candidate ({1})", roundSolutions.OrderBy (x => x.Length).First (), roundSolutions.OrderBy (x => x.Length).First ().Length);
					}
				}
			}

			BestSolution = Solutions.OrderBy (x => x.Length).First ();
		}

		void ReduceExplicit ()
		{
			// The list of all possible solutions, starting with the formula itself obviously
			Solutions.Add (Formula);
			bool done = false;
			int i = 0;
			var roundSolutions = new List<DNFOr> (Solutions);
			while (!done) {
				done = true;
				var previousRound = roundSolutions;
				roundSolutions = new List<DNFOr> ();

				if (Verbose > 0) {
					Console.WriteLine ("** Round {0} **", i++);
					Console.WriteLine ("Size of pool: {0} / {1}", previousRound.Count, Solutions.Count);
					Console.WriteLine ("Potential size of current pool: {0}", previousRound.Count * Facts.Count);
				}

				int j,lastPercentDisplayed;
				
				// We could not remove variables from a clause. Let's try to use the facts in the other direction,
				// adding the consequent when the antecedant is satisfied. This might generate new solutions that
				// might be reduced next.
				j = 0;
				lastPercentDisplayed = 0;
				foreach (var ff in previousRound.ToList ()) {
					int newPercent = (int) Math.Round(((double) j)/previousRound.Count*10)*10;
					if (Verbose > 0 & lastPercentDisplayed != newPercent) {
						Console.WriteLine ("Round progress: " + newPercent + "%");
						lastPercentDisplayed = newPercent;
					}
					if (Verbose > 1)
						Console.WriteLine ("Formula checked: {0}/{1}", j, previousRound.Count ());
					j++;

					var candidates = 
						from clause in ff.Clauses
							from f in Facts
							where clause.Vocabulary.Any (x => f.Vocabulary.Contains (x))
							where f.Antecedant.Terminals.All (clause.Terminals.Contains)
							select f;

					if (Verbose > 1)
						Console.WriteLine ("Size of candidate for extension: " + candidates.Count ());

					foreach (var candidate in candidates) {
						var solution = ff.Copy ();
						var clausesToExtend = 
							from x in solution.Clauses
								where candidate.Antecedant.Terminals.All (x.Terminals.Contains)
								select x;
						foreach (var clauseToExtend in clausesToExtend) {
							foreach (var v in candidate.Consequent.Terminals) {
								clauseToExtend.Terminals.Add (v);
							}

							//solution = solution.Simplify ();
							if (!Solutions.Contains (solution) & !roundSolutions.Contains (solution)) {
								roundSolutions.Add (solution);

								if (Verbose > 2) {
									Console.WriteLine (string.Format ("New candidate found: {0}", solution));
									Console.WriteLine ("<- Adding '{0}' because '{1}' is present", 
									                   string.Join (",", candidate.Consequent.Terminals.Select (x => x.ToString ())),
									                   string.Join (",", candidate.Antecedant.Terminals.Select(y => y.ToString ())));
									Console.WriteLine ();
								}

								done = false;
							}
						}
					}

					// A candidate is a fact such that the ff formula can be rewriten using the antecedant. So
					// - there is a clause in the consequent that is contained in a clause of the formula. 
					//   e.g. Fact: a => b&c, Formula: a&b&c because b&c are in the formula
					// - the antecedant is contained in the same clause
					//   e.g. Fact: a => b&c, Formula: a&b&c because a is in the formula
					candidates = 
						from clause in ff.Clauses
							from f in Facts
							where clause.Vocabulary.Any (x => f.Vocabulary.Contains (x))
							where f.Consequent.Terminals.All (clause.Terminals.Contains) 
							& f.Antecedant.Terminals.All (clause.Terminals.Contains)
							select f;

					if (Verbose > 1)
						Console.WriteLine ("Size of candidate for reduction: " + candidates.Count ());

					int positiveCandidates = 0;
					int alreadyComputedCandidates = 0;

					// For each candidate, we add a new potential solution where we remove the matched variables
					foreach (var candidate in candidates) {
						var solution = ff.Copy ();
						var clausesToSimplify = 
							from c in solution.Clauses
								where candidate.Consequent.Terminals.All (c.Terminals.Contains) 
								& candidate.Antecedant.Terminals.All (c.Terminals.Contains)
								select c;

						foreach (var c in clausesToSimplify) {
							foreach (var v in candidate.Consequent.Terminals) {
								c.Terminals.Remove (v);
							}
						}

						//solution = solution.Simplify ();

						if (!Solutions.Contains (solution) & !roundSolutions.Contains (solution)) {
							roundSolutions.Add (solution);
							// We found a new solution, let's try again because the fixpoint has not been reached

							if (Verbose > 2) {
								Console.WriteLine (string.Format ("New candidate found: {0}", solution));
								Console.WriteLine ("<- Removing '{0}' because '{1}' is present", 
								                   string.Join (",", candidate.Consequent.Terminals.Select (x => x.ToString ())),
								                   string.Join (",", candidate.Antecedant.Terminals.Select(y => y.ToString ())));
								Console.WriteLine ();
							}

							positiveCandidates++;
							done = false;
						} else {
							alreadyComputedCandidates++;

							if (Verbose > 1) {
								Console.WriteLine ("Applying " + candidate + " for removal");
								Console.WriteLine (solution + " is duplicated");
								Console.WriteLine ();
							}
						}


					}
					if (Verbose > 0) {
						Console.WriteLine ("Rejection rate: " + positiveCandidates + "/" + candidates.Count ());
						Console.WriteLine ("Recomputation rate: " + alreadyComputedCandidates + "/" + candidates.Count ());
					}
				}

				Solutions.AddRange (roundSolutions);

				if (Verbose > 0) {
					Console.WriteLine ("Round done");
					Console.WriteLine ();
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
}
