using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = PrimitiveObsessionAnalyzer.Test.CSharpAnalyzerVerifier<
    PrimitiveObsessionAnalyzer.PrimitiveObsessionAnalyzer>;

namespace PrimitiveObsessionAnalyzer.Test
{
    [TestClass]
    public class PrimitiveObsessionAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task WhenNothingToAnalyze_Nothing()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task WhenVariableOfString_ExpectAnalyzerToHit()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class Foo
        {   
            public Foo()
            {
                var name = ""AName"";
            }
        }
    }"; 

            var expected = VerifyCS.Diagnostic("PrimitiveObsessionAnalyzer").WithSpan(15, 17, 15, 36).WithArguments("name");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task WhenPropertyOfInt_ExpectAnalyzerToHit()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class Foo
        {   
           public int Age { get; } = 12;
        }
    }";

            var expected = VerifyCS.Diagnostic("PrimitiveObsessionAnalyzer").WithSpan(13, 19, 13, 22).WithArguments("Age");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }


        [TestMethod]
        public async Task WhenReturnTypeAndParameter_ExpectAnalyzerToHit()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class Foo
        {   
           public int AgeCalculator(int birthYear)
            {
                return DateTime.Now.Year - birthYear;
            }
        }
    }";

            var expectedMethod = VerifyCS.Diagnostic("PrimitiveObsessionAnalyzer").WithSpan(13, 19, 13, 22).WithArguments("AgeCalculator");
            var expectedParameter = VerifyCS.Diagnostic("PrimitiveObsessionAnalyzer").WithSpan(13, 37, 13, 40).WithArguments("birthYear");
            await VerifyCS.VerifyAnalyzerAsync(test, expectedMethod, expectedParameter);
        }

        [TestMethod]
        public async Task WhenValueIsStructOrClass_ExpectAnalyzerToIgnore()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public struct Animal
        {
        }
        public class Foo
        {   
           public void AgeCalculator()
            {
               var a = new Animal();
               var f = new Foo();
            }
        }
    }";
            
            await VerifyCS.VerifyAnalyzerAsync(test);
        }


    }
}
