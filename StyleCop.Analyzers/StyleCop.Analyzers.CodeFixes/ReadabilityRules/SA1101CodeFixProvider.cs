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
    /// This class provides a code fix for <see cref="SA1101PrefixLocalCallsWithThis"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, insert the <c>this.</c> prefix before the call to the class
    /// member.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1101CodeFixProvider))]
    [Shared]
    internal class SA1101CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<SimpleNameSyntax>
    {
        private static readonly ThisExpressionSyntax ThisExpressionSyntax = SyntaxFactory.ThisExpression();

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1101PrefixLocalCallsWithThis.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SA1101CodeFix;

        /// <inheritdoc/>
        protected override SimpleNameSyntax? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var nodeToReplace = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) as SimpleNameSyntax;
            return nodeToReplace;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            SimpleNameSyntax nodeToReplace,
            object context,
            CancellationToken cancellation)
        {
            var replacementNode =
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpressionSyntax,
                    nodeToReplace.WithoutTrivia().WithoutFormatting())
                .WithTriviaFrom(nodeToReplace)
                .WithoutFormatting();
            return replacementNode;
        }
    }
}
