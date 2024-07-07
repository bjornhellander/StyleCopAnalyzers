// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Helpers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Formatting;

    internal static class DocumentExtensions
    {
        public static SyntaxTrivia GetEndOfLineTrivia(this Document document)
        {
            var newLineText = document.Project.Solution.Workspace.Options.GetOption(FormattingOptions.NewLine, LanguageNames.CSharp);
            var newLineTrivia = SyntaxFactory.EndOfLine(newLineText);
            return newLineTrivia;
        }
    }
}
