namespace Expected.Meta;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
sealed class MaybeUnexpectedAnalyzer : DiagnosticAnalyzer {
   static readonly DiagnosticDescriptor MaybeUnexpected = new(
        id: "Expected_MaybeUnexpected",
      title: "Maybe unexpected",
      messageFormat: "Result '{0}' might be unexpected",
      category: "Expected",
      DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
        description: "Ignoring this return value could result in an unhandled error."
   );

   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MaybeUnexpected];

   public override void Initialize(AnalysisContext context) {
      context.EnableConcurrentExecution();
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

      context.RegisterOperationAction(Analyze,
         OperationKind.Invocation,
         OperationKind.Await
      );
   }


   static void Analyze(OperationAnalysisContext context) {
      static bool hasAttribute(ITypeSymbol? type)
         => type?.GetAttributes()
            .FirstOrDefault(static e => e.AttributeClass?
                  .ToDisplayString() is "Expected.MaybeUnexpectedAttribute") is not null;

      static bool ignored(IOperation op) {
         if (op.Parent is IExpressionStatementOperation) return true;
         if (op.Parent is IAwaitOperation await && await.Parent is IExpressionStatementOperation) return true;
         return false;
      }
      var op = context.Operation;
      if (!ignored(op)) return;
      var type = op switch {
         IInvocationOperation i => i.TargetMethod.ReturnType,
         _ => op.Type,
      };
      if (type is null) return;
      if (hasAttribute(type)) {
         var formatted = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
         context.ReportDiagnostic(Diagnostic.Create(MaybeUnexpected, op.Syntax.GetLocation(), formatted));
      }
   }
}
