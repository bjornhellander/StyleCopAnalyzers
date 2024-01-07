// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Text;
    using StyleCop.Analyzers.Settings.ObjectModel;

    internal class DocumentTextChangeBasedFixAllProvider : DocumentBasedFixAllProvider
    {
        private readonly string codeActionTitle;
        private readonly Func<Diagnostic, Document, SourceText, SyntaxNode, StyleCopSettings, TextChange?> getTextChange;

        public DocumentTextChangeBasedFixAllProvider(string codeActionTitle, Func<Diagnostic, StyleCopSettings, TextChange> getTextChange)
        {
            this.codeActionTitle = codeActionTitle;
            this.getTextChange = (diagnostic, document, sourceText, syntaxTree, settings) => getTextChange(diagnostic, settings);
        }

        public DocumentTextChangeBasedFixAllProvider(string codeActionTitle, Func<Diagnostic, TextChange> getTextChange)
        {
            this.codeActionTitle = codeActionTitle;
            this.getTextChange = (diagnostic, document, sourceText, syntaxTree, settings) => getTextChange(diagnostic);
        }

        public DocumentTextChangeBasedFixAllProvider(string codeActionTitle, Func<Diagnostic, Document, TextChange> getTextChange)
        {
            this.codeActionTitle = codeActionTitle;
            this.getTextChange = (diagnostic, document, sourceText, syntaxTree, settings) => getTextChange(diagnostic, document);
        }

        public DocumentTextChangeBasedFixAllProvider(string codeActionTitle, Func<Diagnostic, SyntaxNode, TextChange> getTextChange)
        {
            this.codeActionTitle = codeActionTitle;
            this.getTextChange = (diagnostic, document, sourceText, syntaxTree, settings) => getTextChange(diagnostic, syntaxTree);
        }

        public DocumentTextChangeBasedFixAllProvider(string codeActionTitle, Func<Diagnostic, SyntaxNode, TextChange?> getTextChange)
        {
            this.codeActionTitle = codeActionTitle;
            this.getTextChange = (diagnostic, document, sourceText, syntaxTree, settings) => getTextChange(diagnostic, syntaxTree);
        }

        public DocumentTextChangeBasedFixAllProvider(string codeActionTitle, Func<Diagnostic, SourceText, TextChange> getTextChange)
        {
            this.codeActionTitle = codeActionTitle;
            this.getTextChange = (diagnostic, document, sourceText, syntaxTree, settings) => getTextChange(diagnostic, sourceText);
        }

        public DocumentTextChangeBasedFixAllProvider(string codeActionTitle, Func<Diagnostic, SourceText, StyleCopSettings, TextChange> getTextChange)
        {
            this.codeActionTitle = codeActionTitle;
            this.getTextChange = (diagnostic, document, sourceText, syntaxTree, settings) => getTextChange(diagnostic, sourceText, settings);
        }

        protected override string CodeActionTitle => this.codeActionTitle;

        protected override async Task<SyntaxNode?> FixAllInDocumentAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
        {
            if (diagnostics.IsEmpty)
            {
                return null;
            }

            var text = await document.GetTextAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
            var tree = await document.GetSyntaxTreeAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
            var root = await tree.GetRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
            var settings = SettingsHelper.GetStyleCopSettings(document.Project.AnalyzerOptions, tree, fixAllContext.CancellationToken);

            var changes = new List<TextChange>();
            foreach (var diagnostic in diagnostics)
            {
                var textChange = this.getTextChange(diagnostic, document, text, root, settings);
                if (textChange != null)
                {
                    changes.Add(textChange.Value);
                }
            }

            changes.Sort((left, right) => left.Span.Start.CompareTo(right.Span.Start));

            var newText = text.WithChanges(changes);
            var newTree = tree.WithChangedText(newText);
            var newRoot = await newTree.GetRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
            return newRoot;
        }
    }
}
