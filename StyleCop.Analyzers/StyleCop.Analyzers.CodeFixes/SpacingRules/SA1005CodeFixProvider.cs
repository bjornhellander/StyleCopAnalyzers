// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace StyleCop.Analyzers.SpacingRules
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
    /// Implements a code fix for <see cref="SA1005SingleLineCommentsMustBeginWithSingleSpace"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, ensure that the comment begins with a single space. If the comment is
    /// being used to comment out a line of code, ensure that the comment begins with four forward slashes, in which
    /// case the leading space can be omitted.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1005CodeFixProvider))]
    [Shared]
    internal class SA1005CodeFixProvider : CodeFixProvider
    {
        private static readonly FixAllProvider FixAllInstance
            = new DocumentTextChangeBasedFixAllProvider(
                SpacingResources.SA1005CodeFix,
                GetTextChange);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1005SingleLineCommentsMustBeginWithSingleSpace.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return FixAllInstance;
        }

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        SpacingResources.SA1005CodeFix,
                        cancellationToken => GetTransformedDocumentAsync(diagnostic, context.Document, cancellationToken),
                        nameof(SA1005CodeFixProvider)),
                    diagnostic);
            }
        }

        private static async Task<Document> GetTransformedDocumentAsync(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            return document.WithText(text.WithChanges(GetTextChange(diagnostic, text)));
        }

        private static TextChange GetTextChange(Diagnostic diagnostic, SourceText text)
        {
            var sourceSpan = diagnostic.Location.SourceSpan;
            var subText = text.GetSubText(sourceSpan).ToString();

            int i = 2;
            for (; i < subText.Length; i++)
            {
                if (!char.IsWhiteSpace(subText[i]))
                {
                    break;
                }
            }

            return new TextChange(new TextSpan(sourceSpan.Start + 2, i - 2), " ");
        }
    }
}
