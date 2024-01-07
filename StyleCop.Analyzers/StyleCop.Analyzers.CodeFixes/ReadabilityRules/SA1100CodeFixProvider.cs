// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
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
    /// This class provides a code fix for <see cref="SA1100DoNotPrefixCallsWithBaseUnlessLocalImplementationExists"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, change the <c>base.</c> prefix to <c>this.</c>.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1100CodeFixProvider))]
    [Shared]
    internal class SA1100CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<BaseExpressionSyntax>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1100DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SA1100CodeFix;

        /// <inheritdoc/>
        protected override BaseExpressionSyntax GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            return (BaseExpressionSyntax)syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            BaseExpressionSyntax nodeToReplace,
            object context,
            CancellationToken cancellation)
        {
            var thisToken = SyntaxFactory.Token(SyntaxKind.ThisKeyword).WithTriviaFrom(nodeToReplace.Token);
            return SyntaxFactory.ThisExpression(thisToken);
        }
    }
}
