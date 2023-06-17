// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.CodeFixes.Lightup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Options;
    using Microsoft.CodeAnalysis.Rename;

    internal static class RenamerExtensions
    {
        private static readonly Func<Document, string?, IReadOnlyList<string>?, CancellationToken, Task<RenameDocumentActionSetWrapper>>? RenameDocumentFunc;

        static RenamerExtensions()
        {
            RenameDocumentFunc = CreateRenameDocumentAccessor();
        }

        internal static bool SupportsRenameDocument => RenameDocumentFunc != null;

        // NOTE: To simplify usage, this method does not have the DocumentRenameOptions parameter
        internal static async Task<RenameDocumentActionSetWrapper> RenameDocumentAsync(
            Document document,
            string? newDocumentName,
            IReadOnlyList<string>? newDocumentFolders = null,
            CancellationToken cancellationToken = default)
        {
            if (RenameDocumentFunc == null)
            {
                throw new InvalidOperationException("Unsupported method");
            }

            var result = await RenameDocumentFunc(document, newDocumentName, newDocumentFolders, cancellationToken).ConfigureAwait(false);
            return result;
        }

        private static Func<Document, string?, IReadOnlyList<string>?, CancellationToken, Task<RenameDocumentActionSetWrapper>>? CreateRenameDocumentAccessor()
        {
            var renamerType = typeof(Renamer);
            var codeAnalysisWorkspacesAssembly = renamerType.GetTypeInfo().Assembly;
            var renameDocumentActionSetType = codeAnalysisWorkspacesAssembly.GetType("Microsoft.CodeAnalysis.Rename.Renamer+RenameDocumentActionSet");
            var documentRenameOptionsType = codeAnalysisWorkspacesAssembly.GetType("Microsoft.CodeAnalysis.Rename.DocumentRenameOptions");

            var nativeReturnType = typeof(Task<>).MakeGenericType(renameDocumentActionSetType);
            var wrapperReturnType = typeof(Task<RenameDocumentActionSetWrapper>);

            var renameDocumentMethods = renamerType.GetTypeInfo().DeclaredMethods.Where(x => x.Name == "RenameDocumentAsync");
            renameDocumentMethods = renameDocumentMethods.Where(x => IsCorrectRenameDocumentMethod(x, nativeReturnType, documentRenameOptionsType));
            var renameDocumentMethod = renameDocumentMethods.SingleOrDefault();
            if (renameDocumentMethod == null)
            {
                return null;
            }

            var continueWithMethods = nativeReturnType.GetTypeInfo().DeclaredMethods.Where(x => x.Name == "ContinueWith").ToList();
            continueWithMethods = continueWithMethods.Where(IsCorrectContinueWith).ToList();
            var continueWithMethod = continueWithMethods.SingleOrDefault()?.MakeGenericMethod(typeof(RenameDocumentActionSetWrapper));

            var taskParameter = Expression.Parameter(nativeReturnType, "task");
            var continuationLambda = Expression.Lambda(
                Expression.Call(
                    RenameDocumentActionSetWrapper.FromObjectMethod,
                    Expression.Property(taskParameter, "Result")),
                taskParameter);

            var documentParameter = Expression.Parameter(typeof(Document), "solution");
            var newDocumentNameParameter = Expression.Parameter(typeof(string), "newDocumentName");
            var newDocumentFoldersParameter = Expression.Parameter(typeof(IReadOnlyList<string>), "newDocumentFolders");
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            var renameCallExpression = Expression.Call(
                renameDocumentMethod,
                documentParameter,
                Expression.New(documentRenameOptionsType),
                newDocumentNameParameter,
                newDocumentFoldersParameter,
                cancellationTokenParameter);
            var continueWithCallExpression = Expression.Call(renameCallExpression, continueWithMethod, continuationLambda);
            var mainLambda = Expression.Lambda<Func<Document, string?, IReadOnlyList<string>?, CancellationToken, Task<RenameDocumentActionSetWrapper>>>(
                continueWithCallExpression,
                documentParameter,
                newDocumentNameParameter,
                newDocumentFoldersParameter,
                cancellationTokenParameter);
            var func = mainLambda.Compile();
            return func;
        }

        private static bool IsCorrectRenameDocumentMethod(MethodInfo method, Type nativeReturnType, Type documentRenameOptionsType)
        {
            if (!method.IsPublic || !method.IsStatic)
            {
                return false;
            }

            if (method.ReturnType != nativeReturnType)
            {
                return false;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 5)
            {
                return false;
            }

            if (parameters[0].ParameterType != typeof(Document))
            {
                return false;
            }

            if (parameters[1].ParameterType != documentRenameOptionsType)
            {
                return false;
            }

            if (parameters[2].ParameterType != typeof(string))
            {
                return false;
            }

            if (parameters[3].ParameterType != typeof(IReadOnlyList<string>))
            {
                return false;
            }

            if (parameters[4].ParameterType != typeof(CancellationToken))
            {
                return false;
            }

            return true;
        }

        private static bool IsCorrectContinueWith(MethodInfo info)
        {
            var genericParameters = info.GetGenericArguments();
            if (genericParameters.Length != 1)
            {
                return false;
            }

            var parameters = info.GetParameters();
            if (parameters.Length != 1)
            {
                return false;
            }

            if (parameters[0].Name != "continuationFunction" || parameters[0].ParameterType.Name != "Func`2")
            {
                return false;
            }

            return true;
        }
    }
}
