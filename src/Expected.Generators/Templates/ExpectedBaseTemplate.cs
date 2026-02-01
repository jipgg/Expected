namespace Expected.Generators.Templates;

struct Ty(string name, params Ty[] typeParams) {
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
}

static class ExpectedTemplate {
   public static string Apply(ExpectedParams t) {
      var type = t.Type;

      var @readonly = type is ResolvedType.Struct or ResolvedType.RefStruct ? "readonly" : "";

      string makeField(string typeName, string name) {
         var ro = type.IsReadOnly() ? "readonly " : "";
         return $"   internal {ro}{typeName} {name};";
      }

      string makeProperty(string typeName, string name, string get, string set) {
         var ro = type.IsStruct() ? "readonly" : "";
         if (type.IsStruct() && type.IsReadOnly() || type is ResolvedType.RecordClass) {
            return $"   public {ro} {typeName} {name} => {get};";
         } else {
            return $$"""
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
      var value = t.StorageStrategy switch {
         StorageStrategy.SameField => "_storage",
         StorageStrategy.Sequential => "_value",
         _ => "_storage.Value"
      };
      var error = t.StorageStrategy switch {
         StorageStrategy.SameField => "_storage",
         StorageStrategy.Sequential => "_error",
         _ => "_storage.Error"
      };
      const string R = "R";
      Ty V = t.TypeArgs.V;
      Ty E = t.TypeArgs.E;
      Ty Expected = $"global::Expected.{nameof(Expected)}";
      Ty Unexpected = $"global::Expected.{nameof(Unexpected)}";
      Ty Unexpect = $"global::Expected.{nameof(Unexpect)}";
      const string aggressiveInlining
         = "global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)";
      const string unscopedRef = "global::System.Diagnostics.CodeAnalysis.UnscopedRef";
      string storage() {
         const string interop = "global::System.Runtime.InteropServices";
         const string @unsafe = "global::System.Runtime.CompilerServices.Unsafe";
         return t.StorageStrategy switch {
            StorageStrategy.Union => $$"""
                  [{{interop}}.StructLayout({{interop}}.LayoutKind.Explicit)]
                  internal {{(type.IsRefStruct() ? "ref " : "")}}struct Storage {
                     [{{interop}}.FieldOffset(0)]
                     public {{V}} Value;
                     [{{interop}}.FieldOffset(0)]
                     public {{E}} Error;
                  }
               {{makeField("Storage", "_storage")}}
               """,
            StorageStrategy.Object => $$"""
                  internal struct Storage {
                     internal object _storage;
                     [{{unscopedRef}}]
                     public ref {{V}} Value {
                        [{{aggressiveInlining}}]
                        get => ref {{@unsafe}}.As<object, {{V}}>(ref _storage);
                     }
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
               {{attributes}}
               public {{Expected[R, E]}} Select<{{R}}>({{Func[V, R]}} {{argName}}) where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{argName}}({{value}})) : new(default, {{error}});
               {{attributes}}
               public {{Expected[V, R]}} SelectError<{{R}}>({{Func[E, R]}} {{argName}}) where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{value}}) : new(default, {{argName}}({{error}}));
               {{attributes}}
               public {{Expected[R, E]}} AndThen<{{R}}>({{Func[V, Expected[R, E]]}} {{argName}}) where {{R}}: allows ref struct
                  => {{hasValue}} ? {{argName}}({{value}}) : new(default, {{error}});
               {{attributes}}
               public {{Expected[V, R]}} OrElse<{{R}}>({{Func[E, Expected[V, R]]}} {{argName}}) where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{value}}) : {{argName}}({{error}});
               {{attributes}}
               public {{Expected[V, E]}} AndThen({{Func[V, Expected[V, E]]}} {{argName}})
                  => {{hasValue}} ? {{argName}}({{value}}) : new(default, {{error}});
               {{attributes}}
               public {{Expected[V, E]}} OrElse({{Func[E, Expected[V, E]]}} {{argName}})
                  => {{hasValue}} ? new({{value}}) : {{argName}}({{error}});
            """;
      }
      string conversions() {
         const string argName = "v";
         var source = $$"""
               [{{aggressiveInlining}}]
               public static implicit operator {{t.GenericName}}({{Unexpected[E]}} unexpected)
                  => new(default, unexpected.Error);
               [{{aggressiveInlining}}]
               public static implicit operator {{Expected[V, E]}}({{t.GenericName}} {{argName}})
                  => {{argName}}.{{hasValue}} ? new({{argName}}.{{value}}) : new(default, {{argName}}.{{error}});
               [{{aggressiveInlining}}]
               public static implicit operator {{t.GenericName}}({{Expected[V, E]}} {{argName}})
                  => {{argName}}.HasValue ? new({{argName}}.Value) : new(default, {{argName}}.Error);
            """;
         if (V.ToString() is not "global::System.Object") {
            source += $"""
               [{aggressiveInlining}]
               public static implicit operator {t.GenericName}({V} value)
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
               public {{ro}}override int GetHashCode() {
                  var hash = new global::System.HashCode();
                  hash.Add({{hasValue}});
                  if ({{hasValue}}) hash.Add({{value}}, {{EqualityComparer[V]}}.Default);
                  else hash.Add({{error}}, {{EqualityComparer[E]}}.Default);
                  return hash.ToHashCode();
               }
               public {{ro}}{{(t.Sealed || !type.IsClass() ? "" : "virtual ")}}bool Equals({{t.GenericName}} other) {
                  if ({{(type.IsClass() ? "other is null || " : "")}}{{hasValue}} != other.{{hasValue}}) return false;
                  return {{hasValue}}
                     ? {{EqualityComparer[V]}}.Default.Equals({{value}}, other.{{value}})
                     : {{EqualityComparer[E]}}.Default.Equals({{error}}, other.{{error}});
               }
            """;
      }
      Ty IExpected = new("global::Expected.IExpected");
      return $$"""
         {{(t.Namespace is null ? "" : $"namespace {t.Namespace};")}}
         [global::Expected.CouldBeUnexpected]
         partial {{type.Keyword()}} {{t.GenericName}}: {{IExpected[t.GenericName, V, E]}} {
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
            public {{@readonly}} bool HasValue => {{hasValue}}; 
            public {{@readonly}} {{V}} ValueOr({{V}} value) => {{hasValue}} ? {{value}} : value;
            public {{@readonly}} {{E}} ErrorOr({{E}} error) => {{hasValue}} ? error : {{error}};
            [{{aggressiveInlining}}]
            public {{t.Name}}({{V}} value) {
               {{hasValue}} = true;
               {{error}} = default!;
               {{value}} = value;
            }
            [{{aggressiveInlining}}]
            public {{t.Name}}({{Unexpect}} unexpect, {{E}} error) {
               {{hasValue}} = false;
               {{value}} = default!;
               {{error}} = error;
            }
            [{{aggressiveInlining}}]
            public {{Expected[V, E]}} AsExpected() => ({{Expected[V, E]}})this;
            [{{aggressiveInlining}}]
            public static bool operator true({{t.GenericName}} expected)
               => expected.{{hasValue}};
            [{{aggressiveInlining}}]
            public static bool operator false({{t.GenericName}} expected)
               => !expected.{{hasValue}};
            [{{aggressiveInlining}}]
            public static bool operator !({{t.GenericName}} expected)
               => !expected.{{hasValue}};
            public static {{V}} operator +({{t.GenericName}} expected)
               => expected.{{hasValue}} ? expected.{{value}} : {{throwBadAccess}};
            public static {{E}} operator -({{t.GenericName}} expected)
               => expected.{{hasValue}} ? {{throwBadAccess}} : expected.{{error}};
            {{fluentMethods("global::Expected.ScopedInFunc")}}
            {{fluentMethods("global::System.Func")}}
            {{conversions()}}
            {{equality()}}
         }
         """;
   }
}
