using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CouldBeUnexpectedAnalyzer : DiagnosticAnalyzer {
   static readonly DiagnosticDescriptor Rule = new(
		id: "EX001",
      title: "Could be unexpected",
      messageFormat: "This result could be unexpected",
      category: "Usage",
      DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
		description: "Ignoring this return value could result in an unhandled error."
   );

   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

   public override void Initialize(AnalysisContext context) {
      context.EnableConcurrentExecution();
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

      context.RegisterOperationAction(Analyze, OperationKind.Invocation);
      context.RegisterOperationAction(Analyze, OperationKind.ObjectCreation);
      context.RegisterOperationAction(Analyze, OperationKind.PropertyReference);
      context.RegisterOperationAction(AnalyzeAwait, OperationKind.Await);
   }

   static bool IsExpectedType(ITypeSymbol? type) {
      if (type is not INamedTypeSymbol named) return false;

      var fullName = named.ContainingNamespace + "." + named.Name;
      ReadOnlySpan<string> expectedTypeNames = ["Expected.Expected", "Expected.ValueExpected", "Expected.RefExpected"];
      return expectedTypeNames.IndexOf(fullName) is not -1;
   }

   static bool IsIgnored(IOperation op) {
      if (op.Parent is IExpressionStatementOperation) return true;
      if (op.Parent is IAwaitOperation awaitOp && awaitOp.Parent is IExpressionStatementOperation) return true;
      return false;
   }

   static void Analyze(OperationAnalysisContext context) {
      if (context.Operation is not IInvocationOperation invocation) return;

      var returnType = invocation.TargetMethod.ReturnType;

      if (!IsExpectedType(returnType)) return;

      if (IsIgnored(invocation)) {
         var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), returnType.ToDisplayString());
         context.ReportDiagnostic(diagnostic);
      }
   }

   static void AnalyzeAwait(OperationAnalysisContext context) {
      if (context.Operation is not IAwaitOperation awaitOp) return;

      var type = awaitOp.Type;
      if (!IsExpectedType(type)) return;

      if (awaitOp.Parent is IExpressionStatementOperation) {
         var diagnostic = Diagnostic.Create(Rule, awaitOp.Syntax.GetLocation(), type?.ToDisplayString());
         context.ReportDiagnostic(diagnostic);
      }
   }
}
