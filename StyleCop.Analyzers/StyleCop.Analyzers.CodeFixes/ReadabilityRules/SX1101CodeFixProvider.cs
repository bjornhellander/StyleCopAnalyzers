// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.ReadabilityRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using StyleCop.Analyzers.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SX1101CodeFixProvider))]
    [Shared]
    internal class SX1101CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<MemberAccessExpressionSyntax>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SX1101DoNotPrefixLocalMembersWithThis.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SX1101CodeFix;

        /// <inheritdoc/>
        protected override MemberAccessExpressionSyntax? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) as ThisExpressionSyntax;
            if (node == null || node.IsMissing)
            {
                return null;
            }

            return node.Parent as MemberAccessExpressionSyntax;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            MemberAccessExpressionSyntax node,
            object context,
            CancellationToken cancellationToken)
        {
            return node.Name.WithTriviaFrom(node);
        }
    }
}
