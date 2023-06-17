// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.CodeFixes.Lightup
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Rename;

    internal readonly struct RenameDocumentActionSetWrapper
    {
        internal static readonly string WrappedTypeName = "Microsoft.CodeAnalysis.Rename.Renamer+RenameDocumentActionSet";
        internal static readonly Type WrapperType;
        internal static readonly MethodInfo? FromObjectMethod;

        private static readonly Func<RenameDocumentActionSetWrapper, Solution, CancellationToken, Task<Solution>>? UpdateSolutionFunc;

        private readonly object obj;

        static RenameDocumentActionSetWrapper()
        {
            var assembly = typeof(Renamer).GetTypeInfo().Assembly;
            WrapperType = assembly.GetType(WrappedTypeName);
            FromObjectMethod = typeof(RenameDocumentActionSetWrapper).GetTypeInfo().GetDeclaredMethod("FromObject");

            UpdateSolutionFunc = CreateUpdateSolutionAccessor();
        }

        private RenameDocumentActionSetWrapper(object obj)
        {
            this.obj = obj;
        }

        internal static bool SupportsUpdateSolution => UpdateSolutionFunc != null;

        public static RenameDocumentActionSetWrapper FromObject(object? obj)
        {
            if (obj == null)
            {
                return default;
            }

            if (!IsInstance(obj))
            {
                throw new InvalidCastException($"Cannot wrap '{obj.GetType().FullName}' to '{WrappedTypeName}'");
            }

            return new RenameDocumentActionSetWrapper(obj);
        }

        // NOTE: Referenced using reflection
        public object? ToObject()
        {
            return this.obj;
        }

        public async Task<Solution> UpdateSolutionAsync(Solution solution, CancellationToken cancellationToken)
        {
            if (UpdateSolutionFunc == null)
            {
                throw new InvalidOperationException("Unsupported method");
            }

            var result = await UpdateSolutionFunc(this, solution, cancellationToken).ConfigureAwait(false);
            return result;
        }

        private static bool IsInstance(object obj)
        {
            return obj != null && obj.GetType().GetTypeInfo().IsAssignableFrom(WrapperType.GetTypeInfo());
        }

        private static Func<RenameDocumentActionSetWrapper, Solution, CancellationToken, Task<Solution>>? CreateUpdateSolutionAccessor()
        {
            var renamerType = typeof(Renamer);
            var codeAnalysisWorkspacesAssembly = renamerType.GetTypeInfo().Assembly;
            var nativeRenameDocumentActionSetType = codeAnalysisWorkspacesAssembly.GetType("Microsoft.CodeAnalysis.Rename.Renamer+RenameDocumentActionSet");
            var wrapperRenameDocumentActionSetType = typeof(RenameDocumentActionSetWrapper);
            var returnType = typeof(Task<Solution>);
            var nativeRenameDocumentActionType = codeAnalysisWorkspacesAssembly.GetType("Microsoft.CodeAnalysis.Rename.Renamer+RenameDocumentAction");
            var nativeRenameDocumentActionArrayType = typeof(ImmutableArray<>).MakeGenericType(nativeRenameDocumentActionType);

            var updateSolutionMethods = nativeRenameDocumentActionSetType.GetTypeInfo().DeclaredMethods.Where(x => x.Name == "UpdateSolutionAsync");
            updateSolutionMethods = updateSolutionMethods.Where(x => IsCorrectUpdateSolutionMethod(x, returnType, nativeRenameDocumentActionArrayType));
            var updateSolutionMethod = updateSolutionMethods.SingleOrDefault();
            if (updateSolutionMethod == null)
            {
                return null;
            }

            var toObjectMethod = GetToObjectMethod(wrapperRenameDocumentActionSetType);
            var emptyArrayMethod = nativeRenameDocumentActionArrayType.GetTypeInfo().GetDeclaredField("Empty");

            var wrapperInstanceParameter = Expression.Parameter(typeof(RenameDocumentActionSetWrapper), "instance");
            var solutionParameter = Expression.Parameter(typeof(Solution), "solution");
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            var objectInstanceValue = Expression.Call(wrapperInstanceParameter, toObjectMethod);
            var nativeInstanceValue = Expression.Convert(objectInstanceValue, nativeRenameDocumentActionSetType);
            var updateSolutionCallExpression = Expression.Call(
                nativeInstanceValue,
                updateSolutionMethod,
                solutionParameter,
                Expression.Field(null, emptyArrayMethod),
                cancellationTokenParameter);
            var lambda = Expression.Lambda<Func<RenameDocumentActionSetWrapper, Solution, CancellationToken, Task<Solution>>>(
                updateSolutionCallExpression,
                wrapperInstanceParameter,
                solutionParameter,
                cancellationTokenParameter);
            var func = lambda.Compile();
            return func;
        }

        private static bool IsCorrectUpdateSolutionMethod(MethodInfo method, Type returnType, Type nativeRenameDocumentActionArrayType)
        {
            if (!method.IsPublic || method.IsStatic)
            {
                return false;
            }

            if (method.ReturnType != returnType)
            {
                return false;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 3)
            {
                return false;
            }

            if (parameters[0].ParameterType != typeof(Solution))
            {
                return false;
            }

            if (parameters[1].ParameterType != nativeRenameDocumentActionArrayType)
            {
                return false;
            }

            if (parameters[2].ParameterType != typeof(CancellationToken))
            {
                return false;
            }

            return true;
        }

        private static MethodInfo? GetToObjectMethod(Type wrapperRenameDocumentActionSetType)
        {
            var methods = wrapperRenameDocumentActionSetType.GetTypeInfo().DeclaredMethods;
            var method = methods.Single(x => x.Name == "ToObject");
            return method;
        }
    }
}
