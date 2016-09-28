// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.MaintainabilityRules
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.MaintainabilityRules;
    using TestHelper;

    public abstract class SA1402ForNonBlockDeclarationUnitTestsBase : CodeFixVerifier
    {
        public abstract string Keyword { get; }

        protected bool DisableRule { get; set; } = false;

        public abstract Task TestOneElementAsync();

        public abstract Task TestTwoElementsAsync();

        public abstract Task TestTwoElementsWithRuleDisabledAsync();

        public abstract Task TestThreeElementsAsync();

        public abstract Task TestPreferFilenameTypeAsync();

        protected override string GetSettings()
        {
            var keywords = new List<string> { "class", "interface", "struct", "enum", "delegate" };
            if (this.DisableRule)
            {
                keywords.Remove(this.Keyword);
            }

            var keywordsStr = string.Join(", ", keywords.Select(x => "\"" + x + "\""));

            var settings = $@"
{{
  ""settings"": {{
    ""maintainabilityRules"": {{
      ""topLevelTypes"": [{keywordsStr}]
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
