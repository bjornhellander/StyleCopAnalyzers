// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.DocumentationRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Implements the code fix for property summary documentation.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertySummaryDocumentationCodeFixProvider))]
    [Shared]
    internal class PropertySummaryDocumentationCodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<XmlTextSyntax>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                PropertySummaryDocumentationAnalyzer.SA1623Descriptor.Id,
                PropertySummaryDocumentationAnalyzer.SA1624Descriptor.Id);

        /// <inheritdoc/>
        protected override string CodeActionTitle => DocumentationResources.PropertySummaryStartTextCodeFix;

        /// <inheritdoc/>
        protected override XmlTextSyntax? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            if (diagnostic.Properties.ContainsKey(PropertySummaryDocumentationAnalyzer.NoCodeFixKey))
            {
                return null;
            }

            var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
            var documentation = node.GetDocumentationCommentTriviaSyntax();

            var summaryElement = (XmlElementSyntax)documentation.Content.GetFirstXmlElement(XmlCommentHelper.SummaryXmlTag);
            var textElement = XmlCommentHelper.TryGetFirstTextElementWithContent(summaryElement);
            return textElement;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            XmlTextSyntax textElement,
            object context,
            CancellationToken cancellationToken)
        {
            var textToken = textElement.TextTokens.First(token => token.IsKind(SyntaxKind.XmlTextLiteralToken));
            var text = textToken.ValueText;

            // preserve leading whitespace
            int index = 0;
            while (text.Length > index && char.IsWhiteSpace(text, index))
            {
                index++;
            }

            var preservedWhitespace = text.Substring(0, index);

            // process the current documentation string
            string modifiedText;
            string textToRemove;
            if (diagnostic.Properties.TryGetValue(PropertySummaryDocumentationAnalyzer.TextToRemoveKey, out textToRemove))
            {
                modifiedText = text.Substring(text.IndexOf(textToRemove) + textToRemove.Length).TrimStart();
            }
            else
            {
                modifiedText = text.Substring(index);
            }

            if (modifiedText.Length > 0)
            {
                modifiedText = char.ToLowerInvariant(modifiedText[0]) + modifiedText.Substring(1);
            }

            // create the new text string
            var textToAdd = diagnostic.Properties[PropertySummaryDocumentationAnalyzer.ExpectedTextKey];
            var newText = $"{preservedWhitespace}{textToAdd} {modifiedText}";

            // replace the token
            var newXmlTextLiteral = SyntaxFactory.XmlTextLiteral(textToken.LeadingTrivia, newText, newText, textToken.TrailingTrivia);
            var newTextTokens = textElement.TextTokens.Replace(textToken, newXmlTextLiteral);
            var newTextElement = textElement.WithTextTokens(newTextTokens);
            return newTextElement;
        }
    }
}
