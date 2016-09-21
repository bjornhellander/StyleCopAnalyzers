// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.MaintainabilityRules
{
    using System.Threading;
    using System.Threading.Tasks;
    using TestHelper;
    using Xunit;

    public class SA1402ForEnumUnitTests : SA1402ForNonBlockDeclarationUnitTestsBaseUnitTestsBase
    {
        public override string Keyword => "enum";

        [Fact]
        public override async Task TestOneElementAsync()
        {
            var testCode = @"
enum Foo
{
    A, B, C
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public override async Task TestTwoElementsAsync()
        {
            var testCode = @"
enum Foo
{
    A, B, C
}

enum Bar
{
    D, E
}";

            var fixedCode = new[]
            {
                @"
enum Foo
{
    A, B, C
}",
                @"
enum Bar
{
    D, E
}"
            };

            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(4, this.Keyword.Length + 2);

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(new[] { testCode }, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public override async Task TestThreeElementsAsync()
        {
            var testCode = @"
enum For
{
    A, B, C
}

enum Bar
{
    D, E
}

enum ForBar
{
    F, G, H
}";

            var fixedCode = new[]
            {
                @"
enum Foo
{
    A, B, C
}
",
                @"
enum Bar
{
    D, E
}
",
                @"
enum FooBar
{
    F, G, H
}"
            };

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithLocation(4, this.Keyword.Length + 2),
                this.CSharpDiagnostic().WithLocation(7, this.Keyword.Length + 2)
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(new[] { testCode }, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public override async Task TestPreferFilenameTypeAsync()
        {
            var testCode = @"
enum Foo
{
    A, B, C
}

enum Test0
{
    D, E
}";

            var fixedCode = new[]
            {
                @"
enum Test0
{
    A, B, C
}",
                @"
enum Foo
{
    D, E
}"
            };

            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(4, this.Keyword.Length + 2);

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(new[] { testCode }, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }
    }
}
