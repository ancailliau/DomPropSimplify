using System;
using System.Collections.Generic;
using System.Linq;

namespace DomPropSimplify
{
	public class DNFOr
	{
		public int Length {
			get { return Clauses.Sum (x => x.Length); }
		}

		public ISet<DNFAnd> Clauses {
			get;
			set;
		}

		public ISet<string> Vocabulary {
			get {
				return new HashSet<string> (Clauses.SelectMany (x => x.Vocabulary));
			}
		}

		public DNFOr (params DNFAnd[] nodes)
		{
			Clauses = new HashSet<DNFAnd> (nodes);
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(DNFOr))
				return false;
			DNFOr other = (DNFOr)obj;
			return Clauses.All (x => other.Clauses.Any (y => y.Equals (x))) 
				&& other.Clauses.All (x => Clauses.Any (y => y.Equals (x)));
		}

		public override int GetHashCode ()
		{
			unchecked {
				if (Clauses == null)
					return 0;

				int hash = 0;
				foreach (var t in Clauses) 
					hash ^= t.GetHashCode ();
				return hash;
			}
		}

		public override string ToString ()
		{
			return string.Format ("({0})", string.Join ("|", Clauses.Select (x => x.ToString ())));
		}

		public DNFOr Copy ()
		{
			return new DNFOr { Clauses = new HashSet<DNFAnd> (Clauses.Select (x => x.Copy ())) };
		}

		public IEnumerable<DNFVar> Variables 
		{
			get {
				return Clauses.SelectMany (x => x.Variables);
			}
		}

		public DNFOr Simplify ()
		{
			var nvar = this.Length;

			var map = new Dictionary <string, int> ();
			var reverseMap = new Dictionary <int, string> ();
			int i = 0;
			foreach (var v in Variables) {
				if (!map.ContainsKey (v.Name)) {
					map.Add (v.Name, i);
					reverseMap.Add (i, v.Name);
					i++;
				}
			}

			List<Term> terms = new List<Term> ();
			foreach (var c in Clauses) {
				var values = new byte[nvar];
				for (int j = 0; j < nvar; j++) {
					values[j] = Term.DontCare;
				}
				foreach (var v in c.Terminals) {
					if (v is DNFVar) {
						values[map[((DNFVar) v).Name]] = Term.True;
					} else if (v is DNFNot) {
						values[map[((DNFNot) v).Enclosed.Name]] = Term.False;
					} else {
						throw new NotSupportedException ();
					}
				}
				terms.Add (new Term (values));
			}

			var f = new QMCFormula (terms);
			f.ReduceToPrimeImplicants ();
			f.ReducePrimeImplicantsToSubset ();

			var simplified = new DNFOr ();
			foreach (var t in f.Terms) {
				var and = new DNFAnd ();
				for (int j = 0; j < nvar; j++) {
					if (t.Values[j] == Term.True) {
						and.Terminals.Add (new DNFVar (reverseMap [j]));
					} else if (t.Values[j] == Term.False) {
						and.Terminals.Add (new DNFNot (new DNFVar (reverseMap [j])));
					}
				}
				simplified.Clauses.Add (and);
			}

			return simplified;
		}

		public Formula Convert () 
		{
			return new Or (Clauses.Select (x => x.Convert ()).ToArray ());
		}
	}

	public class DNFAnd
	{
		public int Length {
			get { return Terminals.Sum (x => x.Length); }
		}

		public ISet<DNFTerminal> Terminals {
			get;
			set;
		}
		public ISet<string> Vocabulary {
			get {
				return new HashSet<string> (Terminals.SelectMany (x => x.Vocabulary));
			}
		}

		public IEnumerable<DNFVar> Variables {
			get {
				return Terminals.SelectMany (x => x.Variables);
			}
		}

		public DNFAnd (params DNFTerminal[] nodes)
		{
			Terminals = new HashSet<DNFTerminal> (nodes);
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(DNFAnd))
				return false;
			DNFAnd other = (DNFAnd)obj;
			return Terminals.All (other.Terminals.Contains)
				& other.Terminals.All (Terminals.Contains);
		}

		public override int GetHashCode ()
		{
			unchecked {
				if (Terminals == null)
					return 0;

				int hash = 0;
				foreach (var t in Terminals) 
					hash ^= t.GetHashCode ();
				return hash;
			}
		}

		public override string ToString ()
		{
			return string.Format ("({0})", string.Join ("&", Terminals.Select (x => x.ToString ())));
		}

		public DNFAnd Copy ()
		{
			return new DNFAnd () { Terminals = new HashSet<DNFTerminal> (Terminals) };
		}

		
		public Formula Convert () 
		{
			return new And (Terminals.Select (x => x.Convert ()).ToArray ());
		}

	}

	public interface DNFTerminal {
		IEnumerable<DNFVar> Variables {
			get;
		}
		int Length {
			get;
		}
		Formula Convert ();

		DNFTerminal Negate ();
		ISet<string> Vocabulary { get; }
	}

	public class DNFNot : DNFTerminal
	{
		public DNFVar Enclosed {
			get;
			set;
		}
		public ISet<string> Vocabulary {
			get {
				return new HashSet<string> (Enclosed.Vocabulary);
			}
		}

		public IEnumerable<DNFVar> Variables {
			get {
				return new DNFVar[] { Enclosed };
			}
		}

		public int Length {
			get {
				return Enclosed.Length;
			}
		}

		public DNFNot (DNFVar enclosed)
		{
			this.Enclosed = enclosed;
		}

		public override string ToString ()
		{
			return string.Format ("!{0}", Enclosed);
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(DNFNot))
				return false;
			DNFNot other = (DNFNot)obj;
			return Enclosed.Equals(other.Enclosed);
		}
		

		public override int GetHashCode ()
		{
			unchecked {
				return (Enclosed != null ? Enclosed.GetHashCode () : 0);
			}
		}

		
		public Formula Convert () 
		{
			return new Not (Enclosed.Convert ());
		}

		public DNFTerminal Negate ()
		{
			return this.Enclosed;
		}
	}

	public class DNFVar : DNFTerminal
	{
		public int Length {
			get { return 1; }
		}

		public string Name {
			get;
			set;
		}
		public ISet<string> Vocabulary {
			get {
				return new HashSet<string> (new string[] { Name });
			}
		}

		public IEnumerable<DNFVar> Variables {
			get {
				return new DNFVar[] { this };
			}
		}

		public DNFVar (string name)
		{
			Name = name;
		}

		public override string ToString ()
		{
			return string.Format ("{0}", Name);
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(DNFVar))
				return false;
			DNFVar other = (DNFVar)obj;
			return Name == other.Name;
		}

		public override int GetHashCode ()
		{
			unchecked {
				return (Name != null ? Name.GetHashCode () : 0);
			}
		}
		public Formula Convert () 
		{
			return new Var (Name);
		}

		public DNFTerminal Negate ()
		{
			return new DNFNot (this);
		}
	}
}

