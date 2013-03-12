using System;

namespace Data
{
    public abstract class Either<TLeft, TRight> {
        public abstract TResult Match<TResult>(Func<TLeft, TResult> Left, Func<TRight,TResult> Right);
        public static Either<TLeft, TRight> NewLeft(TLeft value) { return new Left(value); }
        public static Either<TLeft, TRight> NewRight(TRight value) { return new Right(value); }
        class Left : Either<TLeft, TRight>
        {
            TLeft value;
            public Left(TLeft value) { this.value = value; }

            public override TResult Match<TResult>(Func<TLeft, TResult> Left, Func<TRight, TResult> Right)
            {
                return Left(value);
            }
        }
        class Right : Either<TLeft, TRight>
        {
            TRight value;
            public Right(TRight value) { this.value = value; }

            public override TResult Match<TResult>(Func<TLeft, TResult> Left, Func<TRight, TResult> Right)
            {
                return Right(value);
            }
        }
    }

    public static class Either
    {
        public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value)
        {
            return Either<TLeft, TRight>.NewLeft(value);
        }
        public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value)
        {
            return Either<TLeft, TRight>.NewRight(value);
        }
    }

}
