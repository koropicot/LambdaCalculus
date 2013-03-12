using System;
using System.Collections.Generic;
using System.Linq;

namespace Data
{
    public abstract class ConsList<T> : IEnumerable<T>
    {
        public abstract TResult Match<TResult>(Func<TResult> Nill, Func<T, ConsList<T>, TResult> Cons);
        public static ConsList<T> NewNill() { return new Nill(); }
        public static ConsList<T> NewCons(T head, ConsList<T> tail) { return new Cons(head, tail); }

        class Nill : ConsList<T>
        {
            public override TResult Match<TResult>(Func<TResult> Nill, Func<T, ConsList<T>, TResult> Cons)
            {
                return Nill();
            }
        }
        class Cons : ConsList<T>
        {
            T head;
            ConsList<T> tail;
            public Cons(T head, ConsList<T> tail)
            {
                this.head = head;
                this.tail = tail;
            }

            public override TResult Match<TResult>(Func<TResult> Nill, Func<T, ConsList<T>, TResult> Cons)
            {
                return Cons(head, tail);
            }
        }
    
        public IEnumerator<T> GetEnumerator()
        {
            return 
                Ex.Unfold(this,cl => cl.Match(
                    Nill: () => Option.None<Tuple<T, ConsList<T>>>(),
                    Cons: (h, t) => Option.Some(Tuple.Create(h, t))))
                .GetEnumerator();
        }  
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public static class ConsList
    {
        public static ConsList<T> Cons<T>(T head, ConsList<T> tail)
        {
            return ConsList<T>.NewCons(head, tail);
        }
        public static ConsList<T> Nill<T>()
        {
            return ConsList<T>.NewNill();
        }
        public static ConsList<T> ToConsList<T>(this IEnumerable<T> seq)
        {
            return seq.Any()
                ? ConsList.Cons(seq.First(), seq.Skip(1).ToConsList())
                : ConsList.Nill<T>();
        }
    }
}
