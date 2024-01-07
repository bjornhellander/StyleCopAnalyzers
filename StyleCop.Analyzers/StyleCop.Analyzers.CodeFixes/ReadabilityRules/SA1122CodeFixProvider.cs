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
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Implements a code fix for <see cref="SA1122UseStringEmptyForEmptyStrings"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, replace the empty string literal with the static <see cref="string.Empty"/> field.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1122CodeFixProvider))]
    [Shared]
    internal class SA1122CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<SyntaxNode>
    {
        private static readonly SyntaxNode StringEmptyExpression;

        static SA1122CodeFixProvider()
        {
            var identifierNameSyntax = SyntaxFactory.IdentifierName(nameof(string.Empty));
            var stringKeyword = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
            StringEmptyExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stringKeyword, identifierNameSyntax)
                .WithoutFormatting();
        }

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1122UseStringEmptyForEmptyStrings.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SA1122CodeFix;

        /// <inheritdoc/>
        protected override SyntaxNode? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            return node;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            SyntaxNode nodeToReplace,
            object context,
            CancellationToken cancellationToken)
        {
            var newNode = StringEmptyExpression.WithTriviaFrom(nodeToReplace);
            return newNode;
        }
    }
}
