﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace StyleCop.Analyzers.Test.CSharp9.LayoutRules
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Testing;
    using StyleCop.Analyzers.Test.CSharp8.LayoutRules;
    using StyleCop.Analyzers.Test.Helpers;
    using Xunit;
    using static StyleCop.Analyzers.Test.Verifiers.StyleCopCodeFixVerifier<
        StyleCop.Analyzers.LayoutRules.SA1508ClosingBracesMustNotBePrecededByBlankLine,
        StyleCop.Analyzers.LayoutRules.SA1508CodeFixProvider>;

    public partial class SA1508CSharp9UnitTests : SA1508CSharp8UnitTests
    {
        [Theory]
        [MemberData(nameof(CommonMemberData.RecordTypeDeclarationKeywords), MemberType = typeof(CommonMemberData))]
        [WorkItem(3272, "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3272")]
        public async Task TestSingleLineRecordAsync(string keyword)
        {
            var testCode = $@"namespace TestNamespace
{{
    public {keyword} TestRecord;
}}
";

            await VerifyCSharpFixAsync(testCode, DiagnosticResult.EmptyDiagnosticResults, testCode, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(CommonMemberData.RecordTypeDeclarationKeywords), MemberType = typeof(CommonMemberData))]
        [WorkItem(3272, "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3272")]
        public async Task TestSingleLineRecordWithParameterAsync(string keyword)
        {
            var testCode = $@"namespace TestNamespace
{{
    public {keyword} TestRecord(int Count);
}}
";

            await VerifyCSharpFixAsync(testCode, DiagnosticResult.EmptyDiagnosticResults, testCode, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(CommonMemberData.RecordTypeDeclarationKeywords), MemberType = typeof(CommonMemberData))]
        [WorkItem(3272, "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3272")]
        public async Task TestMultiLineRecordAsync(string keyword)
        {
            var testCode = $@"namespace TestNamespace
{{
    public {keyword} TestRecord
    {{
        public int Count {{ get; init; }}
    }}
}}
";

            await VerifyCSharpFixAsync(testCode, DiagnosticResult.EmptyDiagnosticResults, testCode, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(CommonMemberData.RecordTypeDeclarationKeywords), MemberType = typeof(CommonMemberData))]
        [WorkItem(3272, "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3272")]
        public async Task TestMultiLineRecordWithParameterAsync(string keyword)
        {
            var testCode = $@"namespace TestNamespace
{{
    public {keyword} TestRecord(int Count)
    {{
        public int Count2 {{ get; init; }} = 0;
    }}
}}
";

            await VerifyCSharpFixAsync(testCode, DiagnosticResult.EmptyDiagnosticResults, testCode, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
