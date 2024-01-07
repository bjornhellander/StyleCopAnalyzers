// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace StyleCop.Analyzers.LayoutRules
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
    using StyleCop.Analyzers.Settings.ObjectModel;

    /// <summary>
    /// Implements a code fix for <see cref="SA1518UseLineEndingsCorrectlyAtEndOfFile"/>.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1518CodeFixProvider))]
    [Shared]
    internal class SA1518CodeFixProvider : CodeFixProvider
    {
        private static readonly FixAllProvider FixAllInstance
            = new DocumentTextChangeBasedFixAllProvider(
                LayoutResources.SA1518CodeFix,
                GetTextChange);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1518UseLineEndingsCorrectlyAtEndOfFile.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return FixAllInstance;
        }

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxTree = await context.Document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);
            var settings = SettingsHelper.GetStyleCopSettings(context.Document.Project.AnalyzerOptions, syntaxTree, context.CancellationToken);
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        LayoutResources.SA1518CodeFix,
                        cancellationToken => FixEndOfFileAsync(context.Document, diagnostic, settings, cancellationToken),
                        nameof(SA1518CodeFixProvider)),
                    diagnostic);
            }
        }

        /// <summary>
        /// Fixes the whitespace at the end of a document.
        /// </summary>
        /// <param name="document">The document to be changed.</param>
        /// <param name="diagnostic">The diagnostic to fix.</param>
        /// <param name="settings">The StyleCop settings to use.</param>
        /// <param name="cancellationToken">The cancellation token associated with the fix action.</param>
        /// <returns>The transformed document.</returns>
        private static async Task<Document> FixEndOfFileAsync(Document document, Diagnostic diagnostic, StyleCopSettings settings, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            return document.WithText(text.WithChanges(GetTextChange(diagnostic, settings)));
        }

        private static TextChange GetTextChange(Diagnostic diagnostic, StyleCopSettings settings)
        {
            var newlineAtEndOfFile = settings.LayoutRules.NewlineAtEndOfFile;
            var replacement = newlineAtEndOfFile == OptionSetting.Omit ? string.Empty : "\r\n";
            return new TextChange(diagnostic.Location.SourceSpan, replacement);
        }
    }
}
