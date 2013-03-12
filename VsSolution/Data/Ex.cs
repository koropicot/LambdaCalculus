using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data
{
    public static class Ex
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            source.Select(t => { action(t); return 0; }).ToArray();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            source.Select((t, i) => { action(t, i); return 0; }).ToArray();
        }

        public static T Act<T>(this T arg, Action<T> action)
        {
            action(arg);
            return arg;
        }

        public static TResult Apply<T, TResult>(this T arg, Func<T, TResult> func)
        {
            return func(arg);
        }

        public static IEnumerable<T> Generate<T, TState>(TState seed, Func<TState, Tuple<T, TState>> func)
        {
            var state = seed;
            while (true)
            {
                var ret = func(state);
                yield return ret.Item1;
                state = ret.Item2;
            }
        }

        public static IEnumerable<T> Unfold<T,TState>(TState seed,Func<TState,Option<Tuple<T,TState>>> func)
        {
            var state = seed;
            var loop=false;
            do{
                var ret = func(state).Match(
                    Some: a => { loop = true; state = a.Item2; return a.Item1; },
                    None: () => { loop = false; return default(T); });
                if (loop)
                    yield return ret;
            } while (loop);
        }

        public static IEnumerable<T> AddTail<T>(this T head, IEnumerable<T> tail)
        {
            yield return head;
            foreach (var x in tail)
                yield return x;
        }

        public static IEnumerable<TState> Scan<TSource, TState>
            (this IEnumerable<TSource> source, TState seed, Func<TState, TSource, TState> func)
        {
            var state = seed;
            return source.Select(item => state = func(state, item));
        }
    }
}
