using System;

namespace Data
{
    public abstract class Option<T> 
    {  
        public abstract TResult Match<TResult>(Func<T, TResult> Some, Func<TResult> None);
        public static Option<T> NewSome(T value) { return new Some(value); }
        public static Option<T> NewNone() { return new None(); }

        class Some : Option<T>
        {
            T value;
            public Some(T value) { this.value = value; }
            public override TResult Match<TResult>(Func<T, TResult> Some, Func<TResult> None)
            {
                return Some(value);
            }
            public override string ToString() { return string.Format("Some{{{0}}}", value); }
        }
        class None : Option<T>
        {
            public override TResult Match<TResult>(Func<T, TResult> Some, Func<TResult> None)
            {
                return None();
            }
            public override string ToString() { return "None"; }
        }
    }

    public static class Option
    {
        public static Option<T> Some<T>(T value) { return Option<T>.NewSome(value); }
        public static Option<T> None<T>() { return Option<T>.NewNone(); }
    }
    //public class Some<T> : Option<T>
    //{
    //    public T Value { get; private set; }
    //    public Some(T value)
    //    {
    //        Value = value;
    //    }
    //    public override string ToString()
    //    {
    //        return string.Format("Some{{{0}}}", Value);
    //    }
    //}
    //public class None<T> : Option<T> 
    //{
    //    public override string ToString()
    //    {
    //        return "None";
    //    }
    //}
}
