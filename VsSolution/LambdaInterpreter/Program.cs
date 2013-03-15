using System;
using System.Linq;
using System.Text;
using Data;

namespace LambdaInterpreter
{
    class Program
    {
        static string header = @"
let Succ = λn f x.f (n f x) in
let Pred = λn f x.n (λg h. h (g f)) (λu. x) (λu. u) in
let Plus = λm n.m Succ n in
let Mult = λm n f. m (n f) in
let True = λx y.x in
let False = λx y.y in
let And = λp q. p q False in
let Or = λp q. p True q in
let Not = λp. p False True in
let IsZero = λn. n (λx. False) True in
let Cons = λs b f. f s b in
let Fst = λp. p True in
let Snd = λp. p False in
let Y = λg. (λx. g (x x)) (λx. g (x x)) in
";

        static void Main(string[] args)
        {
            var input = header + @"
let g = λf n.IsZero n 1 (Mult n (f (Pred n))) in
let fact = Y g in
fact 3
";
            Console.WriteLine("Input:{0}", input);
            var output = LambdaParser.LambdaTerm(input.ToConsList()).Match(
                Some: a => new { success = true, result = a },
                None: () => new { success = false, result = Result.New<char, Term>(ConsList.Nill<char>(), null) });
            Console.WriteLine(output.success ? "Success" : "Failure");
            Console.WriteLine("Rest:{0}", string.Concat(output.result.ResidualSource));
            Console.WriteLine("Parsed:{0}", output.result.Return);
            if (output.success)
            {
                var result = output.result.Return;
                TermEx.BetaReductionSteps(result)
                    .ForEach((term, i) =>
                        {
                            result = term;
                            Console.WriteLine(string.Format("Step{0}:{1}", i, term)); 
                        });
                Console.WriteLine("End of Step");
                Console.WriteLine("Result:{0}", result);
            }

            Console.Read();
        }
    }
}
