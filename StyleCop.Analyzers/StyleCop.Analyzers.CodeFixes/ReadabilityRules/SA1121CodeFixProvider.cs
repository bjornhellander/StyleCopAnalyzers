// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace StyleCop.Analyzers.ReadabilityRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Implements a code fix for <see cref="SA1121UseBuiltInTypeAlias"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, ensure that the comma is followed by a single space, and is not preceded
    /// by any space.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1121CodeFixProvider))]
    [Shared]
    internal class SA1121CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<SyntaxNode, SemanticModel>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1121UseBuiltInTypeAlias.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SA1121CodeFix;

        /// <inheritdoc/>
        protected override async Task<SemanticModel> CreateContextAsync(
            Document document,
            SyntaxNode syntaxRoot,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            return semanticModel;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
            return node;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            SyntaxNode node,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (node.Parent is MemberAccessExpressionSyntax memberAccess)
            {
                if (node == memberAccess.Name)
                {
                    node = memberAccess;
                }
            }

            var type = semanticModel.GetSymbolInfo(node, cancellationToken).Symbol as INamedTypeSymbol;

            PredefinedTypeSyntax typeSyntax;
            if (!SpecialTypeHelper.TryGetPredefinedType(type.SpecialType, out typeSyntax))
            {
                return node;
            }

            SyntaxNode newNode;
            if (node is CrefSyntax)
            {
                newNode = SyntaxFactory.TypeCref(typeSyntax);
            }
            else
            {
                newNode = typeSyntax;
            }

            return newNode.WithTriviaFrom(node).WithoutFormatting();
        }
    }
}
