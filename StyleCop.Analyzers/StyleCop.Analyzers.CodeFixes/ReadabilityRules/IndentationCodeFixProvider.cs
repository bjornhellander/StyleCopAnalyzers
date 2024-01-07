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
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Text;
    using StyleCop.Analyzers.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IndentationCodeFixProvider))]
    [Shared]
    internal class IndentationCodeFixProvider : CodeFixProvider
    {
        private static readonly FixAllProvider FixAllInstance
            = new DocumentTextChangeBasedFixAllProvider(
                ReadabilityResources.IndentationCodeFix,
                GetTextChange);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1137ElementsShouldHaveTheSameIndentation.DiagnosticId);

        /// <inheritdoc/>
        public sealed override FixAllProvider GetFixAllProvider() =>
            FixAllInstance;

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        ReadabilityResources.IndentationCodeFix,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(IndentationCodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);

            var textChange = GetTextChange(diagnostic, syntaxRoot);
            if (textChange == null)
            {
                return document;
            }

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            return document.WithText(text.WithChanges(textChange.Value));
        }

        private static TextChange? GetTextChange(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            string replacement;
            if (!diagnostic.Properties.TryGetValue(SA1137ElementsShouldHaveTheSameIndentation.ExpectedIndentationKey, out replacement))
            {
                return null;
            }

            var trivia = syntaxRoot.FindTrivia(diagnostic.Location.SourceSpan.Start);

            TextSpan originalSpan;
            if (trivia == default)
            {
                // The warning was reported on a token because the line is not indented
                originalSpan = new TextSpan(diagnostic.Location.SourceSpan.Start, 0);
            }
            else
            {
                originalSpan = trivia.Span;
            }

            return new TextChange(originalSpan, replacement);
        }
    }
}
