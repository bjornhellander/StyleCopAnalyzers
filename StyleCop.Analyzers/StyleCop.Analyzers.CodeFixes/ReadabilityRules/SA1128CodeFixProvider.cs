// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.ReadabilityRules
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using StyleCop.Analyzers.Helpers;
    using StyleCop.Analyzers.Settings.ObjectModel;

    /// <summary>
    /// Implements a code fix for <see cref="SA1128ConstructorInitializerMustBeOnOwnLine"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, place the constructor initializer on its own line.</para>
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SA1128CodeFixProvider))]
    [Shared]
    internal class SA1128CodeFixProvider : SimpleSyntaxNodeBasedCodeFixProvider<ConstructorDeclarationSyntax, Tuple<IndentationSettings, SyntaxTrivia>>
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(SA1128ConstructorInitializerMustBeOnOwnLine.DiagnosticId);

        /// <inheritdoc/>
        protected override string CodeActionTitle => ReadabilityResources.SA1128CodeFix;

        /// <inheritdoc/>
        protected override Task<Tuple<IndentationSettings, SyntaxTrivia>?> CreateContextAsync(
            Document document,
            SyntaxNode syntaxRoot,
            CancellationToken cancellationToken)
        {
            var settings = SettingsHelper.GetStyleCopSettings(document.Project.AnalyzerOptions, syntaxRoot.SyntaxTree, cancellationToken);
            var newLine = FormattingHelper.GetNewLineTrivia(document);
            var result = Tuple.Create(settings.Indentation, newLine);
            return Task.FromResult<Tuple<IndentationSettings, SyntaxTrivia>?>(result);
        }

        /// <inheritdoc/>
        protected override ConstructorDeclarationSyntax GetNodeToReplace(Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            var constructorInitializer = (ConstructorInitializerSyntax)syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
            var constructorDeclaration = (ConstructorDeclarationSyntax)constructorInitializer.Parent;
            return constructorDeclaration;
        }

        /// <inheritdoc/>
        protected override SyntaxNode GetReplacementNode(
            Diagnostic diagnostic,
            ConstructorDeclarationSyntax constructorDeclaration,
            Tuple<IndentationSettings, SyntaxTrivia> context,
            CancellationToken cancellationToken)
        {
            var indentationSettings = context.Item1;
            var newLine = context.Item2;

            var constructorInitializer = constructorDeclaration.Initializer;

            var newParameterList = constructorDeclaration.ParameterList
                .WithTrailingTrivia(constructorDeclaration.ParameterList.GetTrailingTrivia().WithoutTrailingWhitespace().Add(newLine));

            var indentationSteps = IndentationHelper.GetIndentationSteps(indentationSettings, constructorDeclaration);
            var indentation = IndentationHelper.GenerateWhitespaceTrivia(indentationSettings, indentationSteps + 1);

            var newColonTrailingTrivia = constructorInitializer.ColonToken.TrailingTrivia.WithoutTrailingWhitespace();

            var newColonToken = constructorInitializer.ColonToken
                .WithLeadingTrivia(indentation)
                .WithTrailingTrivia(newColonTrailingTrivia);

            var newInitializer = constructorInitializer
                .WithColonToken(newColonToken)
                .WithThisOrBaseKeyword(constructorInitializer.ThisOrBaseKeyword.WithLeadingTrivia(SyntaxFactory.Space));

            return constructorDeclaration
                .WithParameterList(newParameterList)
                .WithInitializer(newInitializer);
        }
    }
}
