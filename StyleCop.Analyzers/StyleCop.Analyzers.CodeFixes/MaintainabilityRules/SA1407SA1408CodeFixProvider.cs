// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.MaintainabilityRules
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
    /// Implements a code fix for <see cref="SA1407ArithmeticExpressionsMustDeclarePrecedence"/> and  <see cref="SA1408ConditionalExpressionsMustDeclarePrecedence"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, insert parenthesis within the arithmetic expression to declare the precedence of the operations.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1407SA1408CodeFixProvider))]
    [Shared]
    internal class SA1407SA1408CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<BinaryExpressionSyntax>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                SA1407ArithmeticExpressionsMustDeclarePrecedence.DiagnosticId,
                SA1408ConditionalExpressionsMustDeclarePrecedence.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => MaintainabilityResources.SA1407SA1408CodeFix;

        /// <inheritdoc/>
        protected override BinaryExpressionSyntax? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan) as BinaryExpressionSyntax;
            return node != null && !node.IsMissing ? node : null;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            BinaryExpressionSyntax originalNode,
            object context,
            CancellationToken cancellationToken)
        {
            return SyntaxFactory.ParenthesizedExpression(originalNode.WithoutTrivia())
                .WithTriviaFrom(originalNode)
                .WithoutFormatting();
        }
    }
}
