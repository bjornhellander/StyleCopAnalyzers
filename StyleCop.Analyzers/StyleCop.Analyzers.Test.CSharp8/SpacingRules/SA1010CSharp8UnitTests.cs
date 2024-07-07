// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.CSharp8.SpacingRules
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Testing;
    using StyleCop.Analyzers.Test.CSharp7.SpacingRules;
    using Xunit;
    using static StyleCop.Analyzers.Test.Verifiers.StyleCopCodeFixVerifier<
        StyleCop.Analyzers.SpacingRules.SA1010OpeningSquareBracketsMustBeSpacedCorrectly,
        StyleCop.Analyzers.SpacingRules.TokenSpacingCodeFixProvider>;

    public partial class SA1010CSharp8UnitTests : SA1010CSharp7UnitTests
    {
        [Fact]
        [WorkItem(3008, "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3008")]
        public async Task ArrayIndexingUsingIndexFromEndOperatorAsync()
        {
            var testCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(int[] a)
        {
            _ = a[|[|] ^1];
        }
    }
}
";

            var fixedCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(int[] a)
        {
            _ = a[^1];
        }
    }
}
";

            await VerifyCSharpFixAsync(testCode, DiagnosticResult.EmptyDiagnosticResults, fixedCode, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("0..1")]
        [InlineData("..1")]
        [InlineData("0..")]
        [InlineData("..")]
        [WorkItem(3008, "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3008")]
        public async Task ArrayIndexingUsingRangeOperatorAsync(string expr)
        {
            var testCode = $@"
namespace TestNamespace
{{
    public class TestClass
    {{
        public void TestMethod(int[] a)
        {{
            _ = a[|[|] {expr}];
        }}
    }}
}}
";

            var fixedCode = $@"
namespace TestNamespace
{{
    public class TestClass
    {{
        public void TestMethod(int[] a)
        {{
            _ = a[{expr}];
        }}
    }}
}}
";

            await VerifyCSharpFixAsync(testCode, DiagnosticResult.EmptyDiagnosticResults, fixedCode, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
