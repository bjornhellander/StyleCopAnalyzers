// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Helpers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;

    /// <summary>
    /// Base class for code fix providers which work by replacing syntax nodes.
    /// Also automatically provides a custom fix all provider.
    /// </summary>
    /// <typeparam name="TNode">The type of syntax node that is replaced.</typeparam>
    /// <typeparam name="TContext">The type of context that is passed to the <see cref="GetReplacementNode"/> method.</typeparam>
    internal abstract class SimpleSyntaxNodeBasedCodeFixProvider<TNode, TContext> : CodeFixProvider
        where TNode : SyntaxNode
    {
        private FixAllProvider? fixAllInstance;

        /// <summary>
        /// Gets the title used in the created code actions.
        /// </summary>
        /// <value>The title used in the created code actions.</value>
        protected abstract string CodeActionTitle { get; }

        /// <summary>
        /// Gets the equivalence key used in the created code actions.
        /// </summary>
        /// <value>The equivalence key used in the created code actions.</value>
        protected virtual string EquivalenceKey => this.GetType().Name;

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            this.fixAllInstance ??= new SimpleSyntaxNodeBasedFixAllProvider<TNode, TContext>(
                this.CodeActionTitle,
                this.CreateContextAsync,
                this.GetNodeToReplace,
                this.GetReplacementNode);
            return this.fixAllInstance;
        }

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (syntaxRoot == null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                var nodeToReplace = this.GetNodeToReplace(diagnostic, syntaxRoot);
                if (nodeToReplace != null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            this.CodeActionTitle,
                            cancellationToken => this.GetTransformedDocumentAsync(diagnostic, nodeToReplace, context.Document, syntaxRoot, cancellationToken),
                            this.EquivalenceKey),
                        diagnostic);
                }
            }
        }

        /// <summary>
        /// Creates the context object passed to the <see cref="GetReplacementNode"/> method.
        /// If it is not possible to retrieve the information necessary to create the context,
        /// then null can be returned. If this happens, the diagnostic can not be fixed.
        /// </summary>
        /// <param name="document">The document to fix diagnostics in.</param>
        /// <param name="syntaxRoot">The syntax root of the document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly created context object.</returns>
        protected abstract Task<TContext?> CreateContextAsync(
            Document document,
            SyntaxNode syntaxRoot,
            CancellationToken cancellationToken);

        /// <summary>
        /// Returns the node to replace to fix the diagnostic.
        /// If it is not possible to fix the diagnostic, then null can be returned.
        /// </summary>
        /// <param name="diagnostic">The diagnostic to fix.</param>
        /// <param name="syntaxRoot">The syntax root which the diagnostic belongs to.</param>
        /// <returns>Returns the node to replace.</returns>
        protected abstract TNode? GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot);

        /// <summary>
        /// Creates and returns the replacement node. If it's not possible to fix the diagnostic,
        /// then the node to replace can be returned. This will result in no change being made.
        /// </summary>
        /// <param name="diagnostic">The diagnostic to fix.</param>
        /// <param name="nodeToReplace">The node to replace.</param>
        /// <param name="context">The fix context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The replacement node.</returns>
        protected abstract SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            TNode nodeToReplace,
            TContext context,
            CancellationToken cancellationToken);

        private async Task<Document> GetTransformedDocumentAsync(
            Diagnostic diagnostic,
            TNode nodeToReplace,
            Document document,
            SyntaxNode syntaxRoot,
            CancellationToken cancellationToken)
        {
            var context = await this.CreateContextAsync(document, syntaxRoot, cancellationToken).ConfigureAwait(false);
            if (context == null)
            {
                return document;
            }

            var replacementNode = this.GetReplacementNode(diagnostic, nodeToReplace, context, cancellationToken);
            var newSyntaxRoot = syntaxRoot.ReplaceNode(nodeToReplace, replacementNode);
            var newDocument = document.WithSyntaxRoot(newSyntaxRoot);
            return newDocument;
        }
    }
}
