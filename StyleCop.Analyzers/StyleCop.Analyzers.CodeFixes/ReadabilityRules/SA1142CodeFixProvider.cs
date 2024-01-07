// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.ReadabilityRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using StyleCop.Analyzers.Helpers;
    using StyleCop.Analyzers.Lightup;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1142CodeFixProvider))]
    [Shared]
    internal class SA1142CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<SyntaxNode, SemanticModel>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1142ReferToTupleElementsByName.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SA1142CodeFix;

        /// <inheritdoc/>
        protected override async Task<SemanticModel?> CreateContextAsync(
            Document document,
            SyntaxNode syntaxRoot,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            return semanticModel;
        }

        /// <inheritdoc/>
        protected override SyntaxNode? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            return node;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            SyntaxNode fieldName,
            SemanticModel semanticModel,
            CancellationToken cancellation)
        {
            var fieldSymbol = (IFieldSymbol)semanticModel.GetSymbolInfo(fieldName.Parent).Symbol;
            var fieldNameSymbol = fieldSymbol.ContainingType.GetMembers().OfType<IFieldSymbol>().Single(fs => !Equals(fs, fieldSymbol) && Equals(fs.CorrespondingTupleField(), fieldSymbol));

            return SyntaxFactory.IdentifierName(fieldNameSymbol.Name).WithTriviaFrom(fieldName);
        }
    }
}
