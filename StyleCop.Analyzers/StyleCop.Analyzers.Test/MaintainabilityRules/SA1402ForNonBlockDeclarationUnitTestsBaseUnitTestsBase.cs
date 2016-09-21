// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.MaintainabilityRules
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.MaintainabilityRules;
    using TestHelper;

    public abstract class SA1402ForNonBlockDeclarationUnitTestsBaseUnitTestsBase : CodeFixVerifier
    {
        public abstract string Keyword { get; }

        public abstract Task TestOneElementAsync();

        public abstract Task TestTwoElementsAsync();

        public abstract Task TestThreeElementsAsync();

        public abstract Task TestPreferFilenameTypeAsync();

        protected override string GetSettings()
        {
            var settings = $@"
{{
  ""settings"": {{
    ""maintainabilityRules"": {{
      ""topLevelTypes"": [""{this.Keyword}""]
    }}
  }}
}}";

            return settings;
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new SA1402FileMayOnlyContainASingleClass();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new SA1402CodeFixProvider();
        }
    }
}
