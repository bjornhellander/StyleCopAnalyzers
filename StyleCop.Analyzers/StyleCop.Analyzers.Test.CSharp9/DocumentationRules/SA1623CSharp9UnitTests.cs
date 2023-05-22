// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.CSharp9.DocumentationRules
{
    using System.Threading.Tasks;
    using StyleCop.Analyzers.DocumentationRules;
    using StyleCop.Analyzers.Test.CSharp8.DocumentationRules;
    using StyleCop.Analyzers.Test.Verifiers;
    using Xunit;
    using static StyleCop.Analyzers.Test.Verifiers.StyleCopCodeFixVerifier<
        StyleCop.Analyzers.DocumentationRules.PropertySummaryDocumentationAnalyzer,
        StyleCop.Analyzers.DocumentationRules.PropertySummaryDocumentationCodeFixProvider>;

    public class SA1623CSharp9UnitTests : SA1623CSharp8UnitTests
    {
        [Theory]
        [InlineData("{ get; init; }", "Gets or sets")]
        [InlineData("{ get; private init; }", "Gets")]
        [InlineData("{ init { backingField = value; } }", "Sets")]
        [WorkItem(3657, "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3657")]
        public async Task InitAccessorAsync(string accessors, string expectedArgument)
        {
            var testCode = $@"
public class TestClass
{{
    private int backingField;

    /// <summary>
    /// The first test value.
    /// </summary>
    public int {{|#0:TestProperty|}} {accessors}
}}";

            var fixedTestCode = $@"
public class TestClass
{{
    private int backingField;

    /// <summary>
    /// {expectedArgument} the first test value.
    /// </summary>
    public int TestProperty {accessors}
}}";

            var expected = Diagnostic(PropertySummaryDocumentationAnalyzer.SA1623Descriptor).WithLocation(0).WithArguments(expectedArgument);
            var test = new CSharpTest
            {
                TestCode = testCode,
                ExpectedDiagnostics = { expected },
                FixedCode = fixedTestCode,
                ReferenceAssemblies = GenericAnalyzerTest.ReferenceAssembliesNet50,
            };
            await test.RunAsync().ConfigureAwait(false);
        }
    }
}
