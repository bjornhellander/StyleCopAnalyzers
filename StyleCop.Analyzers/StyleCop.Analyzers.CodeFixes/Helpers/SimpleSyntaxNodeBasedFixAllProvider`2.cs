// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;

    /// <summary>
    /// A fix all provider that works by replacing syntax nodes.
    /// Intended to be used by <see cref="SimpleSyntaxNodeBasedCodeFixProvider{TNode}"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of syntax node that is replaced.</typeparam>
    /// <typeparam name="TContext">The type of context that is passed to the <see cref="getReplacementNode"/> method.</typeparam>
    internal class SimpleSyntaxNodeBasedFixAllProvider<TNode, TContext> : DocumentBasedFixAllProvider
        where TNode : SyntaxNode
    {
        private readonly string codeActionTitle;
        private readonly Func<Document, SyntaxNode, CancellationToken, Task<TContext?>> createContext;
        private readonly Func<Diagnostic, SyntaxNode, TNode?> getNodeToReplace;
        private readonly Func<Diagnostic, TNode, TContext, CancellationToken, SyntaxNode> getReplacementNode;

        public SimpleSyntaxNodeBasedFixAllProvider(
            string codeActionTitle,
            Func<Document, SyntaxNode, CancellationToken, Task<TContext?>> createContext,
            Func<Diagnostic, SyntaxNode, TNode?> getNodeToReplace,
            Func<Diagnostic, TNode, TContext, CancellationToken, SyntaxNode> getReplacementNode)
        {
            this.codeActionTitle = codeActionTitle;
            this.getNodeToReplace = getNodeToReplace;
            this.createContext = createContext;
            this.getReplacementNode = getReplacementNode;
        }

        protected override string CodeActionTitle => this.codeActionTitle;

        protected override async Task<SyntaxNode?> FixAllInDocumentAsync(
            FixAllContext fixAllContext,
            Document document,
            ImmutableArray<Diagnostic> diagnostics)
        {
            if (diagnostics.IsEmpty)
            {
                return null;
            }

            var syntaxRoot = await document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
            if (syntaxRoot == null)
            {
                return null;
            }

            var context = await this.createContext(document, syntaxRoot, fixAllContext.CancellationToken).ConfigureAwait(false);
            if (context == null)
            {
                return null;
            }

            var diagnosticPerNode = new Dictionary<SyntaxNode, Diagnostic>(diagnostics.Length);
            foreach (var diagnostic in diagnostics)
            {
                var nodeToReplace = this.getNodeToReplace(diagnostic, syntaxRoot);
                if (nodeToReplace != null)
                {
                    diagnosticPerNode.Add(nodeToReplace, diagnostic);
                }
            }

            var newSyntaxRoot = syntaxRoot.ReplaceNodes(
                diagnosticPerNode.Keys,
                (originalNode, rewrittenNode) =>
                {
                    var diagnostic = diagnosticPerNode[originalNode];
                    return this.getReplacementNode(diagnostic, (TNode)rewrittenNode, context, fixAllContext.CancellationToken);
                });
            return newSyntaxRoot;
        }
    }
}
