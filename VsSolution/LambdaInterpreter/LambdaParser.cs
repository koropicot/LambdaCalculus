using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;

namespace LambdaInterpreter
{
    static class LambdaParser
    {
        // Separator? (Application/Abstract/Atom) Separator?
        public static Parser<char, Term> LambdaTerm = ParserEx.Init(() =>
            from _ in Separator.Optional()
            from term in (
                Let.Or(
                Application.Select(app => (Term)app).Or(
                Abstract.Select(abs => (Term)abs)).Or(
                Atom.Select(var => (Term)var))))
            from __ in Separator.Optional()
            select term);

        // ' '/'\n'/'\r'/'\t'
        static Parser<char, object> WhiteSpace =
            from _ in
                Parser.Terminal(' ').Or(
                Parser.Terminal('\n').Or(
                Parser.Terminal('\r').Or(
                Parser.Terminal('\t'))))
            select (object)null;

        // WhiteSpace+
        static Parser<char, object> Separator =
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

        //  [a-z]/[A-Z]/'_'
        static Parser<char, char> IdentifierChar =
            Parser.CharRange('a', 'z').Or(
            Parser.CharRange('A', 'Z').Or(
            Parser.Terminal('_')));

        // (!KeyWord) (IdentifierChar (IdentifierChar/Numeral)*)
        static Parser<char, Var> Variable =
            KeyWord.Not().Seq(
            from head in IdentifierChar
            from tail in
                IdentifierChar.Or(
                Numeral).More0()
            select new Var(string.Concat(head.AddTail(tail))));

        // ('λ'/'^') Separator? Variable (Separator Variable)* Separator? '.'  M
        static Parser<char, Abs> Abstract = ParserEx.Init(() =>
            from _0 in
                (Parser.Sequence("λ").Or(Parser.Sequence("^"))).Seq(
                Separator.Optional())
            from paras in
                (
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
                Parser.Sequence("."))
            from term in LambdaTerm
            select Abs.VarsAbs(paras, term));

        // Atom Atom+
        static Parser<char, App> Application = ParserEx.Init(() =>
            from head in Atom
            from tail in
                (
                    from _ in Separator.Optional()
                    from e in Atom
                    select e
                ).More1()
            select App.Apps(head.AddTail(tail)));

        // Number/Variable/( '(' M ')' )
        static Parser<char, Term> Atom = (
                from num in Number
                select num)
            .Or(
                from v in Variable
                select (Term)v)
            .Or(
                from _0 in Parser.Sequence("(")
                from m in LambdaTerm
                from _1 in Parser.Sequence(")")
                select m);

        // "let" Separator? Variable Separator? "=" M "in" M
        static Parser<char, Term> Let =
            from _let in Parser.Sequence("let").Seq(Separator.Optional())
            from var in Variable
            from _equal in Separator.Optional().Seq(Parser.Sequence("="))
            from m in LambdaTerm
            from _in in Parser.Sequence("in")
            from n in LambdaTerm
            select (Term)new App(new Abs(var, n), m);
    }
}
