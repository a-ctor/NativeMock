namespace NativeMock.Analyzer
{
  using System;
  using System.Collections.Immutable;
  using System.Composition;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CodeActions;
  using Microsoft.CodeAnalysis.CodeFixes;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  using Shared;

  [ExportCodeFixProvider (LanguageNames.CSharp, Name = nameof(NativeMockCallbackSignatureMismatchCodeFixProvider))]
  [Shared]
  public class NativeMockCallbackSignatureMismatchCodeFixProvider : CodeFixProvider
  {
    private static readonly NativeMockCallbackProvider s_nativeMockCallbackProvider = new NativeMockCallbackProvider();
    private static readonly PInvokeMemberProvider s_pInvokeMemberProvider = new PInvokeMemberProvider();

    private static readonly SyntaxTriviaList s_singleSpaceTriviaList = SyntaxTriviaList.Create (SyntaxFactory.Space);

    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create (RuleIds.DeclaringFunctionSignatureMismatchRuleId);

    public sealed override FixAllProvider GetFixAllProvider()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync (CodeFixContext context)
    {
      var model = await context.Document.GetSemanticModelAsync (context.CancellationToken).ConfigureAwait (false);
      var root = await context.Document.GetSyntaxRootAsync (context.CancellationToken).ConfigureAwait (false);

      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      var methodDeclaration = root.FindToken (diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();
      var methodDeclarationSymbol = model.GetDeclaredSymbol (methodDeclaration);

      var nativeMockInterfaceAttribute = AttributeHelper.GetNativeMockInterfaceAttribute (methodDeclarationSymbol.ContainingType);
      var moduleName = AttributeHelper.GetModuleNameFromAttribute (nativeMockInterfaceAttribute);
      var defaultDeclaringType = AttributeHelper.GetDeclaringTypeSymbolFromAttribute (nativeMockInterfaceAttribute);
      if (moduleName == null)
        return;

      var nativeMockCallback = s_nativeMockCallbackProvider.GetNativeMockCallback (methodDeclarationSymbol, moduleName, defaultDeclaringType);
      if (nativeMockCallback?.DeclaringType == null)
        return;

      var pInvokeMembers = s_pInvokeMemberProvider.GetPInvokeMembers (nativeMockCallback.DeclaringType);
      var matchingPInvokeMember = pInvokeMembers.FirstOrDefault (e => e.Name == nativeMockCallback.Name);
      if (matchingPInvokeMember == null)
        return;

      var methodDeclaringSyntaxReferences = matchingPInvokeMember.Method.DeclaringSyntaxReferences;
      if (methodDeclaringSyntaxReferences.Length != 1)
        return;

      var pInvokeMethodDeclaration = root.FindNode (methodDeclaringSyntaxReferences[0].Span) as MethodDeclarationSyntax;
      if (pInvokeMethodDeclaration == null)
        return;

      context.RegisterCodeFix (
        CodeAction.Create (
          CodeFixResources.ChangeSignatureToMatchDeclaringFunction,
          c => MakeUppercaseAsync (context.Document, methodDeclaration, pInvokeMethodDeclaration, c),
          nameof(CodeFixResources.ChangeSignatureToMatchDeclaringFunction)),
        diagnostic);
    }

    private async Task<Document> MakeUppercaseAsync (Document document, MethodDeclarationSyntax oldMethod, MethodDeclarationSyntax newMethod, CancellationToken cancellationToken)
    {
      MethodDeclarationSyntax patchedMethod = oldMethod;

      // Fix the return type
      if (!oldMethod.ReturnType.IsEquivalentTo (newMethod.ReturnType, true))
      {
        var newReturnType = newMethod.ReturnType
          .WithLeadingTrivia (oldMethod.ReturnType.GetLeadingTrivia())
          .WithTrailingTrivia (oldMethod.ReturnType.GetTrailingTrivia());
        patchedMethod = patchedMethod.WithReturnType (newReturnType);
      }

      var oldParameters = oldMethod.ParameterList.Parameters;
      var newParameters = newMethod.ParameterList.Parameters;
      var patchedParameters = oldParameters;

      // Compare the parameters that both have in common.
      var sameParameterCount = Math.Min (oldParameters.Count, newParameters.Count);
      for (var i = 0; i < sameParameterCount; i++)
      {
        var oldParameter = oldParameters[i];
        var newParameter = newParameters[i];
        var patchedParameter = oldParameter;

        if (!oldParameter.Type.IsEquivalentTo (newParameter.Type))
        {
          var newType = newParameter.Type.WithTriviaFrom (oldParameter.Type);
          patchedParameter = patchedParameter.WithType (newType);
        }

        if (!SyntaxFactory.AreEquivalent (oldParameter.Modifiers, newParameter.Modifiers))
        {
          patchedParameter = patchedParameter.WithModifiers (newParameter.Modifiers);
        }

        if (oldParameter != patchedParameter)
          patchedParameters = patchedParameters.Replace (oldParameter, patchedParameter);
      }

      if (oldParameters.Count < newParameters.Count) // We need to add parameters
      {
        for (var i = oldParameters.Count; i < newParameters.Count; i++)
        {
          var newParameter = newParameters[i];
          var patchedParameter = newParameter.Update (
            SyntaxFactory.List<AttributeListSyntax>(),
            PatchModifierWhitespace (newParameter.Modifiers),
            newParameter.Type.WithoutTrivia().WithTrailingTrivia (s_singleSpaceTriviaList),
            newParameter.Identifier.WithoutTrivia(),
            null);

          patchedParameters = patchedParameters.Add (patchedParameter);
        }
      }
      else if (oldParameters.Count > newParameters.Count) // We need to removed parameters
      {
        for (var i = newParameters.Count; i < oldParameters.Count; i++)
          patchedParameters = patchedParameters.RemoveAt (patchedParameters.Count - 1);
      }

      if (oldParameters != patchedParameters)
        patchedMethod = patchedMethod.WithParameterList (patchedMethod.ParameterList.WithParameters (patchedParameters));

      var root = await document.GetSyntaxRootAsync (cancellationToken);
      var newRoot = root.ReplaceNode (oldMethod, patchedMethod);
      var newDocument = document.WithSyntaxRoot (newRoot);

      return newDocument;
    }

    private SyntaxTokenList PatchModifierWhitespace (SyntaxTokenList list)
    {
      return SyntaxFactory.TokenList (list.Select (e => e.WithoutTrivia().WithTrailingTrivia (s_singleSpaceTriviaList)));
    }
  }
}
