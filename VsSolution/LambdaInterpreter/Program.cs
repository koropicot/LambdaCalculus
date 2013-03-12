using System;
using System.Linq;
using System.Text;
//using System.Collections.Generic;
//using System.Text;
using Data;

namespace LambdaInterpreter
{
    class Program
    {
        #region lambda
        static Parser<char,object> WhiteSpace = 
            from _ in Parser.Terminal(' ').Or(Parser.Terminal('\n')).Or(Parser.Terminal('\r'))
            select (object)null;

        static Parser<char,object> Separator = 
            from _ in WhiteSpace.More1()
            select (object)null;

        static Parser<char, char> IdentifierChar =
            Parser.CharRange('0', '9').Or(
            Parser.CharRange('a', 'z').Or(
            Parser.CharRange('A', 'Z').Or(
            Parser.Terminal('_'))));

        static Parser<char, Var> Variable =
            from ident in IdentifierChar.More1()
            select new Var(string.Concat(ident));

        static Parser<char, Abs> Abstract = ParserEx.Init(() =>
            from _0 in Parser.Terminal('λ').Or(Parser.Terminal('^'))
            from _1 in Separator.Optional()
            from paras in
                (
                    from head in Variable
                    from tail in
                        (
                            from _ in Separator
                            from v in Variable
                            select v).More0()
                    select head.AddTail(tail))
            from _2 in Separator.Optional()
            from _3 in Parser.Terminal('.')
            from _4 in Separator.Optional()
            from term in M
            select Abs.VarsAbs(paras, term));
        
        static Parser<char,App> Application = ParserEx.Init(()=>
            from head in Atom
            from tail in (
                from _ in Separator
                from e in Atom
                select e).More1()
            select App.Apps(head.AddTail(tail)));

        static Parser<char, Term> M = ParserEx.Init(() =>
            from _ in Separator.Optional()
            from term in (
                Application.Select(app => (Term)app).Or(
                Abstract.Select(abs => (Term)abs)).Or(
                Atom.Select(var => (Term)var)))
            from __ in Separator.Optional()
            select term);

        static Parser<char, Term> Atom =(  
                from v in Variable
                select (Term)v)
            .Or(
                from _0 in Parser.Terminal('(')
                from m in M
                from _1 in Parser.Terminal(')')
                select m);
        #endregion

        static void Main(string[] args)
        {
            var input = "(^x y.y x) y";
            Console.WriteLine("Input:" + input);
            var output = M(input.ToConsList()).Match(
                Some: a => new { success = true, result = a },
                None: () => new { success = false, result = Result.New<char,Term>(ConsList.Nill<char>(),null) });
            Console.WriteLine(output.success ? "Success" : "Failure"); 
            Console.WriteLine("Rest:"+string.Concat(output.result.ResidualSource));
            Console.WriteLine("Parsed:"+output.result.Return);
            var ret = output.result.Return;
            Console.WriteLine("FreeVars:" + string.Concat(ret.FreeVars));

            var steps = Ex.Unfold(ret, step => step.BetaReduct().Match(
                    Some: a => Option.Some(Tuple.Create(a,a)),
                    None: () => Option.None<Tuple<Term, Term>>()));
            steps
                .Select((term, i) => string.Format("Step{0}:{1}", i + 1, term))
                .ForEach(result => Console.WriteLine(result));
            Console.WriteLine("End of Step");
            Console.WriteLine("Result:{0}", steps.Last());

            Console.Read();
        }
    }
}
