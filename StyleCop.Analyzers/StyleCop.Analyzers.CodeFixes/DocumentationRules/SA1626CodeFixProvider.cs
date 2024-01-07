// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace StyleCop.Analyzers.DocumentationRules
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

    /// <summary>
    /// Implements a code fix for <see cref="SA1626CodeFixProvider"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, remove a slash from the beginning of the comment so that it begins with
    /// only two slashes.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1626CodeFixProvider))]
    [Shared]
    internal class SA1626CodeFixProvider : CodeFixProvider
    {
        private static readonly FixAllProvider FixAllnstance
            = new DocumentTextChangeBasedFixAllProvider(
                DocumentationResources.SA1626CodeFix,
                GetTextChange);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(SA1626SingleLineCommentsMustNotUseDocumentationStyleSlashes.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return FixAllnstance;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        DocumentationResources.SA1626CodeFix,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(SA1626CodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var textChange = GetTextChange(diagnostic);
            return document.WithText(text.WithChanges(textChange));
        }

        private static TextChange GetTextChange(Diagnostic diagnostic)
        {
            return new TextChange(new TextSpan(diagnostic.Location.SourceSpan.Start, 1), string.Empty);
        }
    }
}
