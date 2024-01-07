// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.MaintainabilityRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Implements a code fix for <see cref="SA1119StatementMustNotUseUnnecessaryParenthesis"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, insert parenthesis within the arithmetic expression to declare the precedence of the operations.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1119CodeFixProvider))]
    [Shared]
    internal class SA1119CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<ParenthesizedExpressionSyntax>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                SA1119StatementMustNotUseUnnecessaryParenthesis.DiagnosticId,
                SA1119StatementMustNotUseUnnecessaryParenthesis.ParenthesesDiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => MaintainabilityResources.SA1119CodeFix;

        /// <inheritdoc/>
        protected override ParenthesizedExpressionSyntax? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            if (diagnostic.Id != SA1119StatementMustNotUseUnnecessaryParenthesis.DiagnosticId)
            {
                return null;
            }

            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true, findInsideTrivia: true) as ParenthesizedExpressionSyntax;
            return node != null && !node.IsMissing ? node : null;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            ParenthesizedExpressionSyntax oldNode,
            object context,
            CancellationToken cancellation)
        {
            var leadingTrivia = SyntaxFactory.TriviaList(oldNode.OpenParenToken.GetAllTrivia().Concat(oldNode.Expression.GetLeadingTrivia()));
            var trailingTrivia = oldNode.Expression.GetTrailingTrivia().AddRange(oldNode.CloseParenToken.GetAllTrivia());

            // Workaround for Roslyn not handling elastic markers for directive trivia correctly.
            if (!leadingTrivia.Any())
            {
                var previousToken = oldNode.OpenParenToken.GetPreviousToken();
                if (!(previousToken.IsKind(SyntaxKind.OpenParenToken) || previousToken.IsKind(SyntaxKind.CloseParenToken))
                    && (TriviaHelper.IndexOfTrailingWhitespace(previousToken.TrailingTrivia) == -1))
                {
                    leadingTrivia = SyntaxFactory.TriviaList(SyntaxFactory.Space);
                }
            }

            return oldNode.Expression
                .WithLeadingTrivia(leadingTrivia)
                .WithTrailingTrivia(trailingTrivia.Any() ? trailingTrivia : SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
        }
    }
}
