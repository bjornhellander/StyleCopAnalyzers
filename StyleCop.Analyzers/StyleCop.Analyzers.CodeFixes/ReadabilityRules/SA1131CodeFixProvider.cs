﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.ReadabilityRules
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
    /// Implements a code fix for <see cref="SA1131UseReadableConditions"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, switch the arguments of the comparison.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1131CodeFixProvider))]
    [Shared]
    internal class SA1131CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<BinaryExpressionSyntax>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1131UseReadableConditions.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SA1131CodeFix;

        /// <inheritdoc/>
        protected override BinaryExpressionSyntax? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var node = (BinaryExpressionSyntax)syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            return node;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            BinaryExpressionSyntax binaryExpression,
            object context,
            CancellationToken cancellationToken)
        {
            // NOTE: This code also changes the syntax node kind, besides the operator token. The modified source code would
            // have been the same without this, but we do this to make tests pass with the default CodeActionValidationMode.
            var newLeft = binaryExpression.Right.WithTriviaFrom(binaryExpression.Left);
            var newRight = binaryExpression.Left.WithTriviaFrom(binaryExpression.Right);
            GetReplacementInfo(binaryExpression.OperatorToken, out var newOperatorToken, out var newNodeKind);
            return SyntaxFactory.BinaryExpression(newNodeKind, newLeft, newOperatorToken, newRight);
        }

        private static void GetReplacementInfo(SyntaxToken operatorToken, out SyntaxToken newToken, out SyntaxKind newNodeKind)
        {
            switch (operatorToken.Kind())
            {
            case SyntaxKind.EqualsEqualsToken:
            case SyntaxKind.ExclamationEqualsToken:
                newToken = operatorToken;
                newNodeKind = operatorToken.Parent.Kind();
                break;

            case SyntaxKind.GreaterThanToken:
                newToken = SyntaxFactory.Token(operatorToken.LeadingTrivia, SyntaxKind.LessThanToken, operatorToken.TrailingTrivia);
                newNodeKind = SyntaxKind.LessThanExpression;
                break;

            case SyntaxKind.GreaterThanEqualsToken:
                newToken = SyntaxFactory.Token(operatorToken.LeadingTrivia, SyntaxKind.LessThanEqualsToken, operatorToken.TrailingTrivia);
                newNodeKind = SyntaxKind.LessThanOrEqualExpression;
                break;

            case SyntaxKind.LessThanToken:
                newToken = SyntaxFactory.Token(operatorToken.LeadingTrivia, SyntaxKind.GreaterThanToken, operatorToken.TrailingTrivia);
                newNodeKind = SyntaxKind.GreaterThanExpression;
                break;

            case SyntaxKind.LessThanEqualsToken:
                newToken = SyntaxFactory.Token(operatorToken.LeadingTrivia, SyntaxKind.GreaterThanEqualsToken, operatorToken.TrailingTrivia);
                newNodeKind = SyntaxKind.GreaterThanOrEqualExpression;
                break;

            default:
                newToken = SyntaxFactory.Token(SyntaxKind.None);
                newNodeKind = (SyntaxKind)operatorToken.Parent.RawKind;
                break;
            }
        }
    }
}
