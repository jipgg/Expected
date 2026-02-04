namespace Expected.Meta;

enum MessageImplOptions : byte {
   Name,
   Partial,
   FullName,
}
sealed record EnumInfo(
   string Name,
   ValueEqualityArray<string> Fields
);
static class ErrorCodeSourceTemplate {
   public sealed record Arguments(
      string? Namespace,
      EnumInfo Enum,
      string Visibility,
      string Title,
      MessageImplOptions MessageImpl
   );
   public static string ApplySourceTemplate(Arguments args) {

      var (type, fields) = args.Enum;

      const string errorCode = "global::Expected.ErrorCode";
      var category = $"{type}Category";

      string expandSwitch(string method, Func<string, string> selector) => $$"""
         public override string {{method}}(int value) => ({{type}})value switch {
            {{string.Join($"\n      ", fields.Select(selector))}}
            _ => throw new global::Expected.Unreachable(),
         };
      """;
      string expandStatic(int indent = 2) {
         var data = new char[indent * 3];
         data.AsSpan().Fill(' ');
         var tabs = new String(data);
         return $$"""
         {{tabs}}{{string.Join($"\n   {tabs}", fields.Select(
               e => $$"""public static {{errorCode}} {{e}} => new((int){{type}}.{{e}}, {{category}}.Instance);"""))}}
         """;
      }
      string value(string v) => $"{type}.{v}";

      var getMessageImpl = args.MessageImpl switch {
         MessageImplOptions.Name => expandSwitch("GetMessage",
            name => $"""{value(name)} => "{name}","""),
         MessageImplOptions.FullName => expandSwitch("GetMessage",
            name => $"""{value(name)} => "{(args.Namespace is null ? "" : $"{args.Namespace}.")}{value(name)}","""),
         MessageImplOptions.Partial or _ => "",
      };
      var @partial = args.MessageImpl is MessageImplOptions.Partial ? " partial" : "";

      const string errorCategory = "global::Expected.ErrorCategory";
      return $$"""""
      {{(args.Namespace is null ? "" : $"namespace {args.Namespace};")}}
      public sealed{{@partial}} class {{category}}: {{errorCategory}} {
         public override string Title => "{{args.Title}}";
      {{getMessageImpl}}
         static {{category}} _instance = null!;
         public static {{category}} Instance {
            get {
               if (_instance is null) _instance = new();
               return _instance;
            }
         }
      }
      {{args.Visibility}} static class ErrorCode{{type}}Extensions {
         public static {{errorCode}} AsCode(this {{type}} e) => new((int)e, {{category}}.Instance);
      #if NET10_0_OR_GREATER
         extension ({{errorCategory}}) {
            public static {{category}} {{type}} => {{category}}.Instance;
         }
         extension ({{errorCode}}) {
      {{expandStatic(3)}}
         }
         extension({{type}}) {
            public static bool operator ==({{errorCode}} a, {{type}} v) => a.Equals(v.AsCode());
            public static bool operator !=({{errorCode}} a, {{type}} v) => !a.Equals(v.AsCode());
         }
      #endif //NET10_0_OR_GREATER
      }
      """"";
   }
}

file struct Ty(string name, params Ty[] typeParams) {
   public readonly string Name = name;
   public readonly string[] Params = [.. typeParams.Select(e => e.ToString())];
   string? _cached;
   public readonly Ty this[params Ty[] typeParams] => new(Name, typeParams);
   public static implicit operator Ty(string name) => new(name);
   public static implicit operator string(Ty type) => type.ToString();
   public override string ToString() {
      if (_cached is not null) return _cached;
      if (Params.Length is 0) {
         _cached = Name;
         return Name;
      }
      var sb = new StringBuilder();
      sb.Append(Name).Append('<');
      foreach (var t in Params) sb.Append(t).Append(',');
      sb[sb.Length - 1] = '>';
      _cached = sb.ToString();
      return _cached;
   }
   public string ToDocString() => ToString().Replace('<', '{').Replace('>', '}');
}

static class ExpectedSourceTemplate {
   public sealed record Arguments(
      string HintName,
      string? Namespace,
      string Name,
      string TypeParams,
      TypeArguments TypeArgs,
      TypeSpec Type,
      bool Sealed,
      StorageStrategy StorageStrategy,
      bool NoImplicit
   ) {
      public string GenericName => $"{Name}{TypeParams}";
   }
   public static string ApplySourceTemplate(Arguments args) {
      var type = args.Type;
      const string R = "R";
      Ty V = args.TypeArgs.V;
      Ty E = args.TypeArgs.E;
      Ty Expected = new($"global::Expected.{nameof(Expected)}", ["V", "E"]);
      Ty Unexpected = $"global::Expected.{nameof(Unexpected)}";
      Ty Unexpect = $"global::Expected.{nameof(Unexpect)}";
      Ty IExpected = new("global::Expected.IExpected", [args.GenericName, V, E]);
      Ty IMutableExpected = new("global::Expected.IMutableExpected", [args.GenericName, V, E]);
      Ty ExpectedMarshal = new("global::Expected.ExpectedMarshal");
      const string aggressiveInlining
         = "global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)";
      const string unscopedRef = "global::System.Diagnostics.CodeAnalysis.UnscopedRef";
      const string detailDoc = "/// <summary> This is an implementation detail. </summary>";

      var @readonly = type is TypeSpec.Struct or TypeSpec.RefStruct ? "readonly" : "";

      string makeField(string typeName, string name) {
         var ro = type.IsReadOnly() ? "readonly " : "";
         return $"""
            {detailDoc}
            internal {ro}{typeName} {name};
         """;
      }

      string makeProperty(string typeName, string name, string get, string set) {
         var ro = type.IsStruct() ? "readonly" : "";
         if (type.IsStruct() && type.IsReadOnly() || type is TypeSpec.RecordClass) {
            return $"""
               /// <inheritdoc/>
               public {ro} {typeName} {name} => {get};
            """;
         } else {
            return $$"""
                  /// <inheritdoc/>
                  public {{typeName}} {{name}} {
                    {{ro}} get => {{get}};
                     set { {{set}}; }
                  }
               """;
         }
      }
      const string expectedNs = "global::Expected";
      const string throwBadAccess = $"throw new {expectedNs}.BadExpectedAccess()";
      const string hasValue = "_hasValue";
      var value = args.StorageStrategy switch {
         StorageStrategy.SameField => "_storage",
         StorageStrategy.Sequential => "_value",
         _ => "_storage.Value"
      };
      var error = args.StorageStrategy switch {
         StorageStrategy.SameField => "_storage",
         StorageStrategy.Sequential => "_error",
         _ => "_storage.Error"
      };
      string storage() {
         const string interop = "global::System.Runtime.InteropServices";
         const string @unsafe = "global::System.Runtime.CompilerServices.Unsafe";
         return args.StorageStrategy switch {
            StorageStrategy.Union => $$"""
                  {{detailDoc}}
                  [{{interop}}.StructLayout({{interop}}.LayoutKind.Explicit)]
                  internal {{(type.IsRefStruct() ? "ref " : "")}}struct Storage {
                     {{detailDoc}}
                     [{{interop}}.FieldOffset(0)]
                     public {{V}} Value;
                     {{detailDoc}}
                     [{{interop}}.FieldOffset(0)]
                     public {{E}} Error;
                  }
               {{makeField("Storage", "_storage")}}
               """,
            StorageStrategy.Object => $$"""
                  {{detailDoc}}
                  internal struct Storage {
                     {{detailDoc}}
                     internal object _storage;
                     {{detailDoc}}
                     [{{unscopedRef}}]
                     public ref {{V}} Value {
                        [{{aggressiveInlining}}]
                        get => ref {{@unsafe}}.As<object, {{V}}>(ref _storage);
                     }
                     {{detailDoc}}
                     [{{unscopedRef}}]
                     public ref {{E}} Error {
                        [{{aggressiveInlining}}]
                        get => ref {{@unsafe}}.As<object, {{E}}>(ref _storage);
                     }
                  }
               {{makeField("Storage", "_storage")}}
               """,
            StorageStrategy.SameField => $"""
               {makeField(V, "_storage")}
               """,
            _ => $"""
               {makeField(V, value)}
               {makeField(E, error)}
               """
         };
      }
      string fluentMethods(Ty Func, bool unscoped = false) {
         var attributes = $"[{aggressiveInlining}{(unscoped ? $",{unscopedRef}" : "")}]";
         const string argName = "f";
         return $$"""

               /// <inheritdoc/>
               {{attributes}}
               public {{Expected[R, E]}} Select<{{R}}>({{Func[V, R]}} {{argName}}) where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{argName}}({{value}})) : new(default, {{error}});

               /// <inheritdoc/>
               {{attributes}}
               public {{Expected[V, R]}} SelectError<{{R}}>({{Func[E, R]}} {{argName}}) where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{value}}) : new(default, {{argName}}({{error}}));

               /// <inheritdoc/>
               {{attributes}}
               public {{Expected[R, E]}} AndThen<{{R}}>({{Func[V, Expected[R, E]]}} {{argName}}) where {{R}}: allows ref struct
                  => {{hasValue}} ? {{argName}}({{value}}) : new(default, {{error}});

               /// <inheritdoc/>
               {{attributes}}
               public {{Expected[V, R]}} OrElse<{{R}}>({{Func[E, Expected[V, R]]}} {{argName}}) where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{value}}) : {{argName}}({{error}});

               /// <inheritdoc/>
               {{attributes}}
               public {{Expected[V, E]}} AndThen({{Func[V, Expected[V, E]]}} {{argName}})
                  => {{hasValue}} ? {{argName}}({{value}}) : new(default, {{error}});

               /// <inheritdoc/>
               {{attributes}}
               public {{Expected[V, E]}} OrElse({{Func[E, Expected[V, E]]}} {{argName}})
                  => {{hasValue}} ? new({{value}}) : {{argName}}({{error}});
            """;
      }
      string conversions() {
         const string argName = "v";
         var source = $$"""
               /// <inheritdoc/>
               [{{aggressiveInlining}}]
               public static implicit operator {{args.GenericName}}(scoped in {{Unexpected[E]}} unexpected)
                  => new(default, unexpected.Error);
               /// <inheritdoc/>
               [{{aggressiveInlining}}]
               public static implicit operator {{Expected[V, E]}}({{args.GenericName}} {{argName}})
                  => {{argName}}.{{hasValue}} ? new({{argName}}.{{value}}) : new(default, {{argName}}.{{error}});
               /// <inheritdoc/>
               [{{aggressiveInlining}}]
               public static implicit operator {{args.GenericName}}(scoped in {{Expected[V, E]}} {{argName}})
                  => {{argName}}.HasValue ? new({{ExpectedMarshal}}.GetValue(in {{argName}})) : new(default, {{ExpectedMarshal}}.GetError(in {{argName}}));
            """;
         if (!args.NoImplicit) {
            source += $"""
               [{aggressiveInlining}]
               public static implicit operator {args.GenericName}({V} value)
                  => new(value);
            """;
         }
         return source;
      }
      string equality() {
         if (!type.IsRecord()) return "";
         Ty EqualityComparer = new("global::System.Collections.Generic.EqualityComparer");
         var ro = type.IsStruct() ? "readonly " : "";
         return $$"""
               /// <inheritdoc/>
               public {{ro}}override int GetHashCode() {
                  var hash = new global::System.HashCode();
                  hash.Add({{hasValue}});
                  if ({{hasValue}}) hash.Add({{value}}, {{EqualityComparer[V]}}.Default);
                  else hash.Add({{error}}, {{EqualityComparer[E]}}.Default);
                  return hash.ToHashCode();
               }
               /// <inheritdoc/>
               public {{ro}}{{(args.Sealed || !type.IsClass() ? "" : "virtual ")}}bool Equals({{args.GenericName}} other) {
                  if ({{(type.IsClass() ? "other is null || " : "")}}{{hasValue}} != other.{{hasValue}}) return false;
                  return {{hasValue}}
                     ? {{EqualityComparer[V]}}.Default.Equals({{value}}, other.{{value}})
                     : {{EqualityComparer[E]}}.Default.Equals({{error}}, other.{{error}});
               }
            """;
      }
      return $$"""
         {{(args.Namespace is null ? "" : $"namespace {args.Namespace};")}}
         /// <summary>
         /// This is a source generated <see cref="{{Expected}}"/> type variant.
         ///
         /// V = <see cref="{{V.ToDocString()}}"/>
         ///
         /// E = <see cref="{{E.ToDocString()}}"/>
         ///
         /// </summary>
         [global::Expected.MaybeUnexpected]
         partial {{type.Keyword()}} {{args.GenericName}}: {{IExpected}}{{(type.IsReadOnly() ? "" : $", {IMutableExpected}")}} {
         {{makeField("bool", hasValue)}}
         {{storage()}}
         {{makeProperty(V, "Value",
            get: $"{hasValue} ? {value} : {throwBadAccess}",
            set: $"{hasValue} = true; {value} = value"
         )}}
         {{makeProperty(E, "Error",
            get: $"{hasValue} ? {throwBadAccess} : {error}",
            set: $"{hasValue} = false; {error} = value"
         )}}
            /// <inheritdoc/>
            public {{@readonly}} bool HasValue => {{hasValue}}; 
            /// <inheritdoc/>
            public {{@readonly}} {{V}} ValueOr({{V}} value) => {{hasValue}} ? {{value}} : value;
            public {{@readonly}} {{E}} ErrorOr({{E}} error) => {{hasValue}} ? error : {{error}};
            [{{aggressiveInlining}}]
            public {{args.Name}}({{V}} value) {
               {{hasValue}} = true;
               {{error}} = default!;
               {{value}} = value;
            }
            /// <summary>
            /// <paramref name="unexpect"/> should be passed as <see langword="default"/>(<see cref="{{Unexpect.ToDocString()}}"/>).
            /// </summary>
            [{{aggressiveInlining}}]
            public {{args.Name}}({{Unexpect}} unexpect, {{E}} error) {
               {{hasValue}} = false;
               {{value}} = default!;
               {{error}} = error;
            }
            /// <inheritdoc/>
            [{{aggressiveInlining}}]
            public {{Expected[V, E]}} AsExpected() => {{hasValue}} ? new({{value}}) : new(default, {{error}});
            /// <inheritdoc/>
            [{{aggressiveInlining}}]
            public static bool operator true(scoped in {{args.GenericName}} expected)
               => expected.{{hasValue}};
            /// <inheritdoc/>
            [{{aggressiveInlining}}]
            public static bool operator false(scoped in {{args.GenericName}} expected)
               => !expected.{{hasValue}};
            /// <inheritdoc/>
            [{{aggressiveInlining}}]
            public static bool operator !(scoped in {{args.GenericName}} expected)
               => !expected.{{hasValue}};
            /// <inheritdoc/>
            public static {{V}} operator +(scoped in {{args.GenericName}} expected)
               => expected.{{hasValue}} ? expected.{{value}} : {{throwBadAccess}};
            /// <inheritdoc/>
            public static {{E}} operator -(scoped in {{args.GenericName}} expected)
               => expected.{{hasValue}} ? {{throwBadAccess}} : expected.{{error}};
            {{fluentMethods("global::Expected.ScopedInFunc")}}
            {{fluentMethods("global::System.Func")}}
            {{conversions()}}
            {{equality()}}
         }
         """;
   }
}
