// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.LayoutRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Implements a code fix for <see cref="SA1503BracesMustNotBeOmitted"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, the violating statement will be converted to a block statement.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1503CodeFixProvider))]
    [Shared]
    internal class SA1503CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<StatementSyntax>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1503BracesMustNotBeOmitted.DiagnosticId, SA1519BracesMustNotBeOmittedFromMultiLineChildStatement.DiagnosticId, SA1520UseBracesConsistently.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => LayoutResources.SA1503CodeFix;

        /// <inheritdoc/>
        protected override StatementSyntax? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, false, true) as StatementSyntax;
            if (node is null || node.IsMissing)
            {
                return null;
            }

            // If the parent of the statement contains a conditional directive, stuff will be really hard to fix correctly, so don't offer a code fix.
            if (ContainsConditionalDirectiveTrivia(node.Parent))
            {
                return null;
            }

            return node;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            StatementSyntax node,
            object context,
            CancellationToken cancellationToken)
        {
            return SyntaxFactory.Block(node);
        }

        private static bool ContainsConditionalDirectiveTrivia(SyntaxNode node)
        {
            for (var currentDirective = node.GetFirstDirective(); currentDirective != null && node.Contains(currentDirective); currentDirective = currentDirective.GetNextDirective())
            {
                switch (currentDirective.Kind())
                {
                case SyntaxKind.IfDirectiveTrivia:
                case SyntaxKind.ElseDirectiveTrivia:
                case SyntaxKind.ElifDirectiveTrivia:
                case SyntaxKind.EndIfDirectiveTrivia:
                    return true;
                }
            }

            return false;
        }
    }
}
