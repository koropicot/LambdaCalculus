using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;

namespace LambdaInterpreter
{
    abstract class Term
    {
        public abstract T Match<T>(Func<string, T> Var, Func<Var, Term, T> Abs, Func<Term, Term, T> App);
        public abstract IEnumerable<Var> FreeVars{get; }
        public abstract Term Substitute(Var var, Term subs);
        public abstract Option<Term> BetaReduct();
    }

    class Var : Term , IEquatable<Var>
    {
        public string Identifier { get; private set; }
        public Var(string ident)
        {
            Identifier = ident;
        }
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }
        public bool Equals(Var other)
        {
            return Identifier == other.Identifier;
        }
        public override bool Equals(object obj)
        {
            return (obj != null ||obj is Var) ? Equals((Var)obj) : false;
        }

        public override string ToString()
        {
            return Identifier;
        }
        public override T Match<T>(Func<string, T> Var, Func<Var, Term, T> Abs, Func<Term, Term, T> App)
        {
            return Var(Identifier);
        }
        public override IEnumerable<Var> FreeVars
        {
            get { return Enumerable.Repeat(this, 1); }
        }
        public override Term Substitute(Var var, Term subs)
        {
            return this.Equals(var) ? subs : this;
        }

        public override Option<Term> BetaReduct()
        {
            return Option.None<Term>();
        }
    }

    class Abs : Term
    {
        public Var Param { get; private set; }
        public Term Term { get; private set; }
        public Abs(Var param, Term term)
        {
            Param = param;
            Term = term;
        }
        public override string ToString()
        {
            return string.Format("(λ{0}.{1})", Param, Term);
        }
        public static Abs VarsAbs(IEnumerable<Var> vars,Term term)
        {
            if (vars.Count() == 0)
                throw new Exception();
            return vars.Count() == 1
                ? new Abs(vars.First(), term)
                : new Abs(vars.First(), VarsAbs(vars.Skip(1), term));
        }

        public override T Match<T>(Func<string, T> Var, Func<Var, Term, T> Abs, Func<Term, Term, T> App)
        {
            return Abs(Param, Term);
        }
        public override IEnumerable<Var> FreeVars
        {
            get { return Term.FreeVars.Where(var => ! var.Equals(Param)); }
        }

        public override Term Substitute(Var var, Term subs)
        {
            return var.Equals(Param)
                ? this //このλ抽象に代入先と同じ変数が束縛されているので代入不能
                : (!subs.FreeVars.Contains(Param) //代入する項の自由変数が仮引数とかぶるとおかしな束縛が起きる
                    ? new Abs(Param,Term.Substitute(var,subs)) //かぶってないので項へ代入して良い
                    : Ex.Unfold(var.Identifier,ident => Option.Some(Tuple.Create(new Var(ident+"'"),ident+"'"))) //仮引数とかぶってる
                        .First(ident => ! (Term.FreeVars.Union(subs.FreeVars).Contains(ident))) //λ抽象の項と、代入する項の自由変数にかぶらない新しい変数を作る
                        .Apply(newParam => new Abs(newParam,Term.Substitute(Param,newParam).Substitute(var,subs))));
        }

        public override Option<Term> BetaReduct()
        {
            return Term.BetaReduct().Match(
                Some: a => Option.Some<Term>(new Abs(Param, a)),
                None: () => Option.None<Term>());
        }
    }

    class App : Term
    {
        public Term Left { get; private set; }
        public Term Right { get; private set; }
        public App(Term left, Term right)
        {
            Left = left;
            Right = right;
        }
        public override string ToString()
        {
            return string.Format("({0} {1})", Left, Right);
        }

        public static App Apps(IEnumerable<Term> terms)
        {
            var c = terms.Count();
            if (c < 2)
                throw new Exception();
            return c == 2
                ? new App(terms.First(), terms.Last())
                : new App(Apps(terms.Take(c-1)), terms.Last());
        }

        public override T Match<T>(Func<string, T> Var, Func<Var, Term, T> Abs, Func<Term, Term, T> App)
        {
            return App(Left, Right);
        }
        public override IEnumerable<Var> FreeVars
        {
            get { return Left.FreeVars.Union(Right.FreeVars); }
        }

        public override Term Substitute(Var var, Term subs)
        {
            return new App(Left.Substitute(var, subs), Right.Substitute(var, subs));
        }

        public override Option<Term> BetaReduct()
        {
            Func<Option<Term>> noneBetaRedex = () =>
                Left.BetaReduct().Match(
                    Some: l => Option.Some<Term>(new App(l, Right)),
                    None: () => Right.BetaReduct().Match(
                        Some: r => Option.Some<Term>(new App(Left, r)),
                        None: () => Option.None<Term>()));

            return Left.Match(
                Var: _ => noneBetaRedex(),
                App: (_, __) => noneBetaRedex(),
                Abs: (p, M) => Option.Some<Term>(M.Substitute(p, Right))); //β簡約
        }
    }
}
