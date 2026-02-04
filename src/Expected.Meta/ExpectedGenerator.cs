#pragma warning disable CS8524 // for exhaustiveness in enum switch
using System.Runtime.InteropServices;
namespace Expected.Meta;

using static ExpectedSourceTemplate;
using static AnalysisHelpers;

file enum SemanticError {
   NoError = default,
   Unknown,
   CycleInStructLayout,
   Arity2AttributeMismatch,
   RefLikeNotAllowed,
}
file enum SyntaxError {
   NoError = default,
   PartialMissing,
}
file readonly record struct AnalysisInfo<E>(E Error, object? Data) where E : Enum;
file static class AnalysisHelpers {
   public static bool Analyze(INamedTypeSymbol symbol, out AnalysisInfo<SemanticError> err) {
      var typeArgs = ResolveTypeArguments(symbol, out var error);
      if (typeArgs is null) {
         if (error is not SemanticError.NoError) {
            err = new(error, null);
            return false;
         } else {
            err = default;
            return true;
         }
      }
      return Analyze(symbol, typeArgs!, out err);
   }
   public static bool Analyze(TypeDeclarationSyntax syntax, out AnalysisInfo<SyntaxError> err) {
      if (syntax.Modifiers.Any(SyntaxKind.PartialKeyword)) {
         err = default;
         return true;
      }
      foreach (var list in syntax.AttributeLists) {
         foreach (var attr in list.Attributes) {
            var name = attr.Name.ToFullString();
            if (name.Contains("Expected") || name.Contains("Unexpected")) {
               err = new(SyntaxError.PartialMissing, syntax);
               return false;
            }
         }
      }
      err = default;
      return true;
   }
   public static bool Analyze(INamedTypeSymbol symbol, TypeSymbols typeArgs, out AnalysisInfo<SemanticError> err) {
      if (!symbol.IsRefLikeType) {
         switch (typeArgs) {
            case { V.IsRefLikeType: true }:
            case { V: ITypeParameterSymbol and { AllowsRefLikeType: true } }:
               err = new(SemanticError.RefLikeNotAllowed, typeArgs.V);
               return false;
            case { E.IsRefLikeType: true }:
            case { E: ITypeParameterSymbol and { AllowsRefLikeType: true } }:
               err = new(SemanticError.RefLikeNotAllowed, typeArgs.E);
               return false;
         }
      }
      if (symbol.TypeKind is TypeKind.Struct) {
         var comparer = SymbolEqualityComparer.Default;
         if (comparer.Equals(symbol, typeArgs.V)) {
            err = new(SemanticError.CycleInStructLayout, typeArgs.V);
            return false;
         } else if (comparer.Equals(symbol, typeArgs.E)) {
            err = new(SemanticError.CycleInStructLayout, typeArgs.E);
            return false;
         }
      }
      err = default;
      return true;
   }
   public static TypeSymbols? ResolveTypeArguments(INamedTypeSymbol symbol, out SemanticError err) {
      var marker = symbol.Interfaces
         .FirstOrDefault(e => e.MetadataName == "IExpected`3");
      if (marker is not null) {
         err = default;
         return new(marker.TypeArguments[1], marker.TypeArguments[2]);
      }
      static AttributeData? findAttr(INamedTypeSymbol symbol, string metadataName) {
         return symbol.GetAttributes()
            .Where(e => e.AttributeClass?.MetadataName == metadataName
               && e.AttributeClass?.ContainingNamespace.ToDisplayString() is "Expected")
            .SingleOrDefault();
      }
      var expectedV = findAttr(symbol, "ExpectedAttribute`1")?.AttributeClass?.TypeArguments[0];
      var unexpectedE = findAttr(symbol, "UnexpectedAttribute`1")?.AttributeClass?.TypeArguments[0];
      if (symbol.Arity is 0) {
         var expectedVE = findAttr(symbol, "ExpectedAttribute`2")?.AttributeClass?.TypeArguments;
         if (expectedVE is { } expectedArgs) {
            err = default;
            return new TypeSymbols(expectedArgs[0], expectedArgs[1]);
         }
         if (expectedV is { } && unexpectedE is { }) {
            err = default;
            return new(expectedV, unexpectedE);
         }
         if (expectedV is null && unexpectedE is null) {
            err = default;
            return null;
         }
         if (expectedV is null || unexpectedE is null) {
            err = SemanticError.Arity2AttributeMismatch;
            return null;
         }
         err = default;
         return new(expectedV, unexpectedE);
      } else if (symbol.Arity is 1) {
         if (expectedV is not null) {
            err = default;
            return new(expectedV, symbol.TypeArguments[0]);
         } else if (unexpectedE is not null) {
            err = default;
            return new(symbol.TypeArguments[0], unexpectedE);
         }
         err = default;
         return null;
      } else if (symbol.Arity is 2) {
         var expected = findAttr(symbol, "ExpectedAttribute");
         if (expected is null) {
            err = default;
            return null;
         }
         err = default;
         return new(symbol.TypeArguments[0], symbol.TypeArguments[1]);
      }
      err = SemanticError.NoError;
      return null;
   }
}

[Generator]
sealed class ExpectedGenerator : IIncrementalGenerator {
   public void Initialize(IncrementalGeneratorInitializationContext context) {
      var provider = context.SyntaxProvider.CreateSyntaxProvider(
         static (node, token) => {
            if (node is not TypeDeclarationSyntax type) return false;
            if (type is InterfaceDeclarationSyntax) return false;
            foreach (var modifier in type.Modifiers) {
               if (modifier.IsKind(SyntaxKind.PartialKeyword)) return true;
            }
            return false;
         },
         static (context, token) => {
            var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (declaredSymbol is not INamedTypeSymbol symbol) return null;
            if (context.Node is not TypeDeclarationSyntax node) return null;
            var typeArgs = ResolveTypeArguments(symbol, out var error);
            if (typeArgs is null) return null;
            if (!Analyze(symbol, typeArgs, out _)) return null;
            var ns = symbol.ContainingNamespace;
            var typeParams = symbol.TypeParameters.Length is 0 ? ""
               : $"<{string.Join(",", symbol.TypeParameters.Select(e => e.Name))}>";

            return new Arguments(
               HintName: Formatting.ToHintName(symbol, typeArgs),
               Namespace: ns.IsGlobalNamespace ? null : ns.ToDisplayString(),
               Name: symbol.Name,
               TypeParams: typeParams,
               Type: ResolveTypeSpec(node),
               TypeArgs: typeArgs,
               Sealed: node.Modifiers.Any(SyntaxKind.SealedKeyword),
               StorageStrategy: ResolveStorageStrategy(symbol, typeArgs),
               NoImplicit: !CanGenerateImplicitConversions(symbol, typeArgs)
            );
         }
      ).Where(static r => r is not null);
      context.RegisterSourceOutput(provider, static (context, result) => {
         context.AddSource(result!.HintName, ApplySourceTemplate(result!));
      });
   }
   static bool CanGenerateImplicitConversions(INamedTypeSymbol symbol, TypeSymbols typeArgs) {
      bool dont(ITypeSymbol type) {
         if (type is not INamedTypeSymbol named) return false;
         if (named.TypeKind is TypeKind.Interface || symbol.InheritsFrom(named)) return true;
         return SymbolEqualityComparer.Default.Equals(symbol, named);
      }
      return !(dont(typeArgs.V) || dont(typeArgs.E));
   }
   static TypeSpec ResolveTypeSpec(TypeDeclarationSyntax node) {
      TypeSpec typeSpec;
      switch (node) {
         case StructDeclarationSyntax:
            var isReadonly = node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
            var isRef = node.Modifiers.Any(SyntaxKind.RefKeyword);
            if (isReadonly && isRef) typeSpec = TypeSpec.ReadOnlyRefStruct;
            else if (isRef) typeSpec = TypeSpec.RefStruct;
            else if (isReadonly) typeSpec = TypeSpec.ReadOnlyStruct;
            else typeSpec = TypeSpec.Struct;
            break;
         case ClassDeclarationSyntax:
            typeSpec = TypeSpec.Class;
            break;
         case RecordDeclarationSyntax r:
            typeSpec = r.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
               ? TypeSpec.RecordStruct
               : TypeSpec.RecordClass;
            break;
         default:
            typeSpec = default;
            break;
      }
      return typeSpec;
   }
   static StorageStrategy ResolveStorageStrategy(INamedTypeSymbol symbol, TypeSymbols types) {
      if (types.V.ToDisplayString().Equals(types.E.ToDisplayString())) return StorageStrategy.SameField;
      var v = types.V as INamedTypeSymbol;
      var e = types.E as INamedTypeSymbol;
      var vParam = types.V as ITypeParameterSymbol;
      var eParam = types.V as ITypeParameterSymbol;
      if (v?.TypeKind is TypeKind.Class && e?.TypeKind is TypeKind.Class) {
         return StorageStrategy.Object;
      } else if (v?.TypeKind is TypeKind.Class && eParam?.HasReferenceTypeConstraint is true) {
         return StorageStrategy.Object;
      } else if (e?.TypeKind is TypeKind.Class && vParam?.HasReferenceTypeConstraint is true) {
         return StorageStrategy.Object;
      } else if (vParam?.HasReferenceTypeConstraint is true && eParam?.HasReferenceTypeConstraint is true) {
         return StorageStrategy.Object;
      } else if (symbol.IsGenericType is false && v?.IsUnmanagedType is true && e?.IsUnmanagedType is true) {
         return StorageStrategy.Union;
      }
      return StorageStrategy.Sequential;
   }
}
[DiagnosticAnalyzer(LanguageNames.CSharp)]
sealed class ExpectedGeneratorAnalyzer : DiagnosticAnalyzer {
   static readonly DiagnosticDescriptor IncompatibleTypeArgument = new(
       id: "Expected_Incompatible",
       title: "Incompatible type argument for target type",
       messageFormat: "incompatible '{0}': {1}",
       category: "Expected",
       defaultSeverity: DiagnosticSeverity.Error,
       isEnabledByDefault: true
   );
   static readonly DiagnosticDescriptor PartialMissing = new(
      "Expected_PartialMissing",
      "Partial specifier is missing",
      "Partial specified is missing on '{0}'",
      "Expected",
      DiagnosticSeverity.Warning,
      true
   );
   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [IncompatibleTypeArgument, PartialMissing];

   public override void Initialize(AnalysisContext context) {
      context.EnableConcurrentExecution();
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.RegisterSymbolAction(static context => {
         var symbol = (INamedTypeSymbol)context.Symbol;
         void report(DiagnosticDescriptor descriptor, object? data, string message) {
            var diagnostic = Diagnostic.Create(
               descriptor,
                symbol.Locations.FirstOrDefault(),
                data switch {
                   ITypeSymbol s => s.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                   object o => o.ToString(),
                   null => "",
                },
                message
            );
            context.ReportDiagnostic(diagnostic);
         }
         foreach (var attribute in symbol.GetAttributes()) {
            if (attribute.AttributeClass is not { } attrClass) continue;
            if (attrClass.ContainingNamespace.Name is not "Expected") continue;
            if (AnalysisHelpers.Analyze(symbol, out var bad)) continue;
            report(IncompatibleTypeArgument, bad.Data, bad.Error switch {
               SemanticError.RefLikeNotAllowed => "target must be a ref struct",
               SemanticError.CycleInStructLayout => "causes a cycle in the struct layout",
               SemanticError.Arity2AttributeMismatch => "expects either Expected<V,E> or both Expected<V> and Unexpected<E> attributes",
               SemanticError.NoError => "No error",
               SemanticError.Unknown => "Unknown error",
            });
            continue;
         }

      }, SymbolKind.NamedType);
      context.RegisterSyntaxNodeAction(static context => {
         if (context.Node is not TypeDeclarationSyntax syntax) return;
         if (!Analyze(syntax, out var error)) {
            context.ReportDiagnostic(Diagnostic.Create(
               PartialMissing,
               syntax.GetLocation(),
               error.Data
            ));
         }
      }, SyntaxKind.StructKeyword, SyntaxKind.RecordKeyword, SyntaxKind.ClassKeyword);
   }
}
