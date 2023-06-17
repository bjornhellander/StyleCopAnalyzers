// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.CSharp11
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    internal class RenameEx
    {
        // Should be in the CodeFix project, but developing it here, to see how the code should look
        internal async Task<Solution> RenameDocumentAsync(Solution solution, Document document, string newDocumentName, CancellationToken cancellationToken = default)
        {
#if false
            var actions = await Microsoft.CodeAnalysis.Rename.Renamer.RenameDocumentAsync(
                document,
                newDocumentName,
                newDocumentFolders: null,
                optionSet: null,
                cancellationToken).ConfigureAwait(false);
            solution = await actions.UpdateSolutionAsync(solution, cancellationToken).ConfigureAwait(false);
            return solution;
#else
            Microsoft.CodeAnalysis.Rename.DocumentRenameOptions options = new Microsoft.CodeAnalysis.Rename.DocumentRenameOptions(
                RenameMatchingTypeInComments: false,
                RenameMatchingTypeInStrings: false);
            Microsoft.CodeAnalysis.Rename.Renamer.RenameDocumentActionSet actions = await Microsoft.CodeAnalysis.Rename.Renamer.RenameDocumentAsync(
                document,
                options,
                newDocumentName,
                newDocumentFolders: null,
                cancellationToken).ConfigureAwait(false);
            solution = await actions.UpdateSolutionAsync(solution, cancellationToken).ConfigureAwait(false);
            return solution;
#endif
        }
    }
}
