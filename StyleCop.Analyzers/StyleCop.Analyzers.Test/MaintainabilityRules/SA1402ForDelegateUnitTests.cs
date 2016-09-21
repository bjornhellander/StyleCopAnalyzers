// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.MaintainabilityRules
{
    using System.Threading;
    using System.Threading.Tasks;
    using TestHelper;
    using Xunit;

    public class SA1402ForDelegateUnitTests : SA1402ForNonBlockDeclarationUnitTestsBaseUnitTestsBase
    {
        public override string Keyword => "delegate";

        [Fact]
        public override async Task TestOneElementAsync()
        {
            var testCode = @"public delegate void Foo();";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public override async Task TestTwoElementsAsync()
        {
            var testCode = @"
public delegate void Foo();

public delegate void Bar();";

            var fixedCode = new[]
            {
                @"
public delegate void Foo();",

                @"
public delegate void Bar();"
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
public delegate void Foo();

public delegate void Bar();

public delegate void FooBar();";

            var fixedCode = new[]
            {
                @"
public delegate void Foo();",

                @"
public delegate void Bar();",

                @"
public delegate void FooBar();"
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
public delegate void Foo();

public delegate void Test0();";

            var fixedCode = new[]
            {
                $@"
public delegate void Test0();",

                $@"
public delegate void Foo();"
            };

            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(1, 9 + this.Keyword.Length);

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(new[] { testCode }, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }
    }
}
