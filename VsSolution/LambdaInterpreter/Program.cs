using System;
using System.Linq;
using System.Text;
using Data;

namespace LambdaInterpreter
{
    class Program
    {
        #region lambda
        // ' '/'\n'/'\r'/'\t'
        static Parser<char,object> WhiteSpace = 
            from _ in 
                Parser.Terminal(' ').Or(
                Parser.Terminal('\n').Or(
                Parser.Terminal('\r').Or(
                Parser.Terminal('\t'))))
            select (object)null;

        // WhiteSpace+
        static Parser<char,object> Separator = 
            from _ in WhiteSpace.More1()
            select (object)null;

        // [0-9]
        static Parser<char, char> Numeral =
            Parser.CharRange('0', '9');

        // Numeral+
        static Parser<char, Term> Number =
            from num in Numeral.More1()
            select TermEx.Number(int.Parse(string.Concat(num)));

        // "let"/"="/"in"
        static Parser<char, string> KeyWord =
            Parser.Sequence("let").Select(string.Concat).Or(
            Parser.Sequence("=").Select(string.Concat).Or(
            Parser.Sequence("in").Select(string.Concat)));

        // [a-z]/[A-Z]/'_'
        static Parser<char, char> IdentifierChar =
            KeyWord.Not().Seq(
            Parser.CharRange('a', 'z').Or(
            Parser.CharRange('A', 'Z').Or(
            Parser.Terminal('_'))));

        // IdentifierChar (IdentifierChar/Numeral)*
        static Parser<char, Var> Variable =
            from head in IdentifierChar
            from tail in 
                IdentifierChar.Or(
                Numeral).More0()
            select new Var(string.Concat(head.AddTail(tail)));

        // ('λ'/'^') Separator? Variable (Separator Variable)* Separator? '.'  M
        static Parser<char, Abs> Abstract = ParserEx.Init(() =>
            from _0 in 
                (Parser.Terminal('λ').Or(Parser.Terminal('^'))).Seq(
                Separator.Optional())
            from paras in (
                from head in Variable
                from tail in
                    (
                        from _ in Separator
                        from v in Variable
                        select v
                    ).More0()
                select head.AddTail(tail))
            from _1 in 
                Separator.Optional().Seq(
                Parser.Terminal('.'))
            from term in M
            select Abs.VarsAbs(paras, term));
        
        // Atom (Separator Atom)+
        static Parser<char,App> Application = ParserEx.Init(()=>
            from head in Atom
            from tail in 
                (
                    from _ in Separator
                    from e in Atom
                    select e
                ).More1()
            select App.Apps(head.AddTail(tail)));

        // Separator? (Application/Abstract/Atom) Separator?
        static Parser<char, Term> M = ParserEx.Init(() =>
            from _ in Separator.Optional()
            from term in (
                Let.Or(
                Application.Select(app => (Term)app).Or(
                Abstract.Select(abs => (Term)abs)).Or(
                Atom.Select(var => (Term)var))))
            from __ in Separator.Optional()
            select term);

        // Number/Variable/( '(' M ')' )
        static Parser<char, Term> Atom =(
                from num in Number
                select num)
            .Or(
                from v in Variable
                select (Term)v)
            .Or(
                from _0 in Parser.Terminal('(')
                from m in M
                from _1 in Parser.Terminal(')')
                select m);

        static Parser<char, Term> Let =
            from _let in Parser.Sequence("let").Seq(Separator.Optional())
            from var in Variable
            from _equal in Separator.Optional().Seq(Parser.Sequence("=").Seq(Separator.Optional()))
            from m in M
            from _in in Separator.Optional().Seq(Parser.Sequence("in").Seq(Separator.Optional()))
            from n in M
            select (Term)new App(new Abs(var, n), m);
        #endregion

        static void Main(string[] args)
        {
            var input = 
                "let plus = λm n f x. m f (n f x) in\n"+
                "let plus2 = plus 2 in\n"+
                "plus2 3";
            Console.WriteLine("Input:" + input);
            var output = M(input.ToConsList()).Match(
                Some: a => new { success = true, result = a },
                None: () => new { success = false, result = Result.New<char,Term>(ConsList.Nill<char>(),null) });
            Console.WriteLine(output.success ? "Success" : "Failure"); 
            Console.WriteLine("Rest:"+string.Concat(output.result.ResidualSource));
            Console.WriteLine("Parsed:"+output.result.Return);
            if (output.success)
            {
                var ret = output.result.Return;
                Console.WriteLine("FreeVars:" + string.Concat(ret.FreeVars));

                var steps = ret.AddTail(Ex.Unfold(ret, step => step.BetaReduct().Match(
                        Some: a => Option.Some(Tuple.Create(a, a)),
                        None: () => Option.None<Tuple<Term, Term>>())));
                steps
                    .Select((term, i) => string.Format("Step{0}:{1}", i, term))
                    .ForEach(result => Console.WriteLine(result));
                Console.WriteLine("End of Step");
                Console.WriteLine("Result:{0}", steps.Last());
            }

            Console.Read();
        }
    }
}
