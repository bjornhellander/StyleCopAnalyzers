// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.CSharp12.ReadabilityRules
{
    using Microsoft.CodeAnalysis.Testing;
    using System.Threading.Tasks;
    using System.Threading;
    using StyleCop.Analyzers.Test.CSharp11.ReadabilityRules;
    using Xunit;

    using static StyleCop.Analyzers.Test.Verifiers.StyleCopCodeFixVerifier<
        StyleCop.Analyzers.ReadabilityRules.SA1137ElementsShouldHaveTheSameIndentation,
        StyleCop.Analyzers.ReadabilityRules.IndentationCodeFixProvider>;

    public partial class SA1137CSharp12UnitTests : SA1137CSharp11UnitTests
    {
        [Fact]
        public async Task TestCollectionExpressionAsync()
        {
            string testCode = @"
class TestClass
{
    private int[] x =
    [
        1,
[|2|],
    ];
}";
            string fixedCode = @"
class TestClass
{
    private int[] x =
    [
        1,
        2,
    ];
}";

            await VerifyCSharpFixAsync(testCode, DiagnosticResult.EmptyDiagnosticResults, fixedCode, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
