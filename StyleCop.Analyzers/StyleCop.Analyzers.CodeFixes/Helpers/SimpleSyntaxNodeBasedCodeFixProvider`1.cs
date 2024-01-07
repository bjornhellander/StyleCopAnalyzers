// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Helpers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    /// <inheritdoc/>
    internal abstract class SimpleSyntaxNodeBasedCodeFixProvider<TNode> : SimpleSyntaxNodeBasedCodeFixProvider<TNode, object>
        where TNode : SyntaxNode
    {
        private static readonly object DummyContext = new();

        /// <inheritdoc/>
        protected override Task<object?> CreateContextAsync(
            Document document,
            SyntaxNode syntaxRoot,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<object?>(DummyContext);
        }
    }
}
