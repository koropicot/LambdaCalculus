using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;

namespace LambdaInterpreter
{
    public class Result<T, TReturn>
    {
        public ConsList<T> ResidualSource { get; private set; }
        public TReturn Return { get; private set; }
        public Result(ConsList<T> rest, TReturn ret)
        {
            ResidualSource = rest;
            Return = ret;
        }
    }
    public static class Result
    {
        public static Result<T, TReturn> New<T, TReturn>(ConsList<T> rest, TReturn ret)
        {
            return new Result<T, TReturn>(rest, ret);
        }
    }

    public delegate Option<Result<T,TReturn>> Parser<T, TReturn>(ConsList<T> source);
    //public delegate Result<char,TReturn> StringParser<TReturn>(ConsList<char> source);

    public static class Parser
    {
        public static Parser<T, T> Terminal<T>(T t)
            where T : IEquatable<T>
        {
            return source => source.Match(
                Cons: (head, tail) => head.Equals(t)
                    ? Option.Some(Result.New(tail, t))
                    : Option.None<Result<T,T>>(),
                Nill: () => Option.None<Result<T, T>>());
        }

        public static Parser<T, T> NullTerminal<T>()
        {
            return source => Option.Some(Result.New(source, default(T)));
        }

        public static Parser<char, char> CharRange(char start, char end)
        {
            return source => source.Match(
                Cons: (head, tail) => start <= head && head <= end
                    ? Option.Some(Result.New(tail, head))
                    : Option.None<Result<char, char>>(),
                Nill: () => Option.None<Result<char, char>>());
        }

        public static Parser<T, IEnumerable<T>> Sequence<T>(IEnumerable<T> seq)
            where T : IEquatable<T>
        {
            return
                from head in Parser.Terminal(seq.First())
                from tail in Sequence(seq.Skip(1))
                select head.AddTail(tail);
        }
    }

    public static class ParserEx
    {
        public static Parser<T,TReturn> Init<T,TReturn>(Func<Parser<T,TReturn>> func)
        {
            return source => func()(source);
        }

        public static Parser<T,TResult> Select<T,TSource, TResult>(
            this Parser<T,TSource> parser,
            Func<TSource, TResult> selector
            )
        {
            return parser.Bind(a => selector(a).Return<T,TResult>());
        }

        public static Parser<T, TResult> SelectMany<T, TLeft, TRight, TResult>(
            this Parser<T, TLeft> parser,
            Func<TLeft, Parser<T, TRight>> parserSelector,
            Func<TLeft, TRight, TResult> resultSelector
            )
        {
            return parser.Bind(l => parserSelector(l).Select(r => resultSelector(l, r)));
        }

        public static Parser<T, TReturn> Or<T, TReturn>(this Parser<T, TReturn> leftParser, Parser<T, TReturn> rightParser)
        {
            return source => leftParser(source).Apply(resultL =>
                resultL.Match(
                    Some: a => resultL,
                    None: () => rightParser(source).Apply(resultR =>
                        resultR.Match(
                            Some: a => resultR,
                            None: () => Option.None<Result<T,TReturn>>()))));
        }

        public static Parser<T, TReturn> Or<T, TReturn>(this IEnumerable<Parser<T, TReturn>> parsers)
        {
            return parsers.First().Or(parsers.Skip(1).Or());
        }

        public static Parser<T, TReturn> Optional<T, TReturn>(this Parser<T, TReturn> parser)
        {
            return
                parser.Or(
                from _ in Parser.NullTerminal<T>()
                select default(TReturn));
        }

        public static Parser<T, IEnumerable<TReturn>> More0<T, TReturn>(this Parser<T, TReturn> parser)
        {
            return seed => 
                Ex.Unfold(
                    seed,
                    source => parser(source).Match(
                        Some:a => Option.Some(Tuple.Create(a,a.ResidualSource)),
                        None:() => Option.None<Tuple<Result<T,TReturn>,ConsList<T>>>()))
                .Apply(results => 
                    Option.Some(Result.New(
                        results.Any() ? results.Last().ResidualSource : seed , 
                        results.Select(result => result.Return))));
        }

        public static Parser<T, IEnumerable<TReturn>> More1<T, TReturn>(this Parser<T, TReturn> parser)
        {
            return
                from head in parser
                from tail in parser.More0()
                select head.AddTail(tail);
        }

        public static Parser<T, TReturn> Bind<T, TLeft, TReturn>(this Parser<T, TLeft> parser, Func<TLeft, Parser<T, TReturn>> func)
        {
            return source => parser(source).Match(
                    Some: l => func(l.Return)(l.ResidualSource),
                    None: () => Option.None<Result<T, TReturn>>());
        }

        public static Parser<T, TReturn> Seq<T, TLeft, TReturn>(this Parser<T, TLeft> left, Parser<T, TReturn> right)
        {
            return left.Bind(_ => right);
        }

        public static Parser<T, TReturn> Seq<T, TReturn>(this IEnumerable<Parser<T, TReturn>> parsers)
        {
            return parsers.First().Seq(parsers.Skip(1).Seq());
        }

        public static Parser<T, TReturn> Return<T, TReturn>(this TReturn ret)
        {
            return source => Option.Some(Result.New(source, ret));
        }
    }
}
