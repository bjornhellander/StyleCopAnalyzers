// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.ReadabilityRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Implements a code fix for <see cref="SA1107CodeMustNotContainMultipleStatementsOnOneLine"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, add a line break.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1107CodeFixProvider))]
    [Shared]
    internal class SA1107CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<SyntaxNode>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1107CodeMustNotContainMultipleStatementsOnOneLine.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SA1107CodeFix;

        /// <inheritdoc/>
        protected override SyntaxNode? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var nodeToReplace = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
            return nodeToReplace?.Parent is BlockSyntax ? nodeToReplace : null;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            SyntaxNode nodeToReplace,
            object context,
            CancellationToken cancellation)
        {
            Debug.Assert(!nodeToReplace.HasLeadingTrivia, "The trivia should be trailing trivia of the previous node");

            return nodeToReplace.WithLeadingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
        }
    }
}
