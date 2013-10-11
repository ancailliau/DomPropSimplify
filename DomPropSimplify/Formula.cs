using System;
using System.Collections.Generic;
using System.Linq;

namespace DomPropSimplify
{
	public interface Formula
	{
		DNFOr DNF { get; }
		Formula Negate ();
	}

	public class Or : Formula {
		public List<Formula> Enclosed { get; set; }
		public Or (params Formula[] enclosed)
		{
			Enclosed = new List<Formula> (enclosed);
		}
		public DNFOr DNF {
			get {
				return new DNFOr (Enclosed.Select (x => x.DNF).SelectMany (x => x.Clauses).ToArray ());
			}
		}
		public Formula Negate ()
		{
			return new And (Enclosed.Select (x => x.Negate ()).ToArray ());
		}
	}
	
	public class And : Formula {
		public List<Formula> Enclosed { get; set; }
		public And (params Formula[] enclosed)
		{
			Enclosed = new List<Formula> (enclosed);
		}
		public DNFOr DNF {
			get {
				var firstOR = Enclosed.FirstOrDefault (x => x.GetType () == typeof(Or));
				if (firstOR != null) {
					var otherNodes = Enclosed.Where (x => x != firstOR);
					return new DNFOr (
						firstOR.DNF.Clauses
						.Select (clause => new And (new Formula[] { clause.Convert () }.Union (otherNodes).ToArray ()).DNF)
						.SelectMany (x => x.Clauses)
						.ToArray ()
					);
				}

				return new DNFOr (new DNFAnd (Enclosed.SelectMany (x => x.DNF.Clauses.Single ().Terminals).ToArray ()));
			}
		}
		public Formula Negate ()
		{
			return new Or (Enclosed.Select (x => x.Negate ()).ToArray ());
		}
	}

	public class Not : Formula {
		public Formula Enclosed { get; set; }
		public Not (Formula enclosed)
		{
			Enclosed = enclosed;
		}
		public DNFOr DNF {
			get {
				if (Enclosed is Var) {
					return new DNFOr (new DNFAnd (new DNFNot (new DNFVar (((Var) Enclosed).Name))));
				} else {
					return Enclosed.Negate ().DNF;
				}
			}
		}
		public Formula Negate ()
		{
			return Enclosed;
		}
	}

	public class Var : Formula {
		public string Name { get; set; }
		public Var (string name)
		{
			Name = name;
		}
		public DNFOr DNF {
			get {
				return new DNFOr (new DNFAnd (new DNFVar (Name)));
			}
		}
		public Formula Negate ()
		{
			return new Not (this);
		}
	}
}

