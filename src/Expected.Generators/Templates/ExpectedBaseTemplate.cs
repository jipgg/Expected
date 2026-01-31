namespace Expected.Generators.Templates;

struct Ty(string name, params Ty[] typeParams) {
   public readonly string Name = name;
   public readonly string[] Params = [.. typeParams.Select(e => e.ToString())];
   string? _cached;
   public readonly Ty this[params Ty[] typeParams] => new(Name, typeParams);
   public static implicit operator Ty(string name) => new(name);
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
      var info = t.ClassInfo;

      var @readonly = info.IsStruct ? "readonly" : "";

      string makeField(string type, string name)
         => $"   internal {(info.StructIsReadOnly ? @readonly : "")} {type} {name};";

      string makeProperty(string type, string name, string get, string set) {
         if (info.IsStruct && info.StructIsReadOnly) {
            return $"   public {@readonly} {type} {name} => {get};";
         } else {
            return $$"""
                  public {{type}} {{name}} {
                    {{@readonly}} get => {{get}};
                     set { {{set}}; }
                  }
               """;
         }
      }
      const string expectedNs = "global::Expected";
      const string throwBadAccess = $"throw new {expectedNs}.BadExpectedAccess()";
      const string hasValue = "_hasValue";
      const string value = "_value";
      const string error = "_error";
      const string R = "R";
      Ty V = t.TValue;
      Ty E = t.TError;
      Ty Expect = "global::Expected.Expect";
      Ty Unexpected = "global::Expected.Unexpected";
      Ty Unexpect = "global::Expected.Unexpect";
      const string aggressiveInlining
         = "global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)";
      const string unscopedRef = "global::System.Diagnostics.CodeAnalysis.UnscopedRef";
      string fluentMethods(Ty Func, bool unscoped = false) {
         var attributes = $"[{aggressiveInlining}{(unscoped ? $",{unscopedRef}" : "")}]";
         const string argName = "f";
         return $$"""

               {{attributes}}
               public {{Expect[R, E]}} Select<{{R}}>({{Func[V, R]}} {{argName}})
               where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{argName}}({{value}})) : new(default, {{error}});
               {{attributes}}
               public {{Expect[V, R]}} SelectError<{{R}}>({{Func[E, R]}} {{argName}})
               where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{value}}) : new(default, {{argName}}({{error}}));
               {{attributes}}
               public {{Expect[R, E]}} AndThen<{{R}}>({{Func[V, Expect[R, E]]}} {{argName}})
               where {{R}}: allows ref struct
                  => {{hasValue}} ? {{argName}}({{value}}) : new(default, {{error}});
               {{attributes}}
               public {{Expect[V, R]}} OrElse<{{R}}>({{Func[E, Expect[V, R]]}} {{argName}})
               where {{R}}: allows ref struct
                  => {{hasValue}} ? new({{value}}) : {{argName}}({{error}});
               {{attributes}}
               public {{Expect[V, E]}} AndThen({{Func[V, Expect[V, E]]}} {{argName}})
                  => {{hasValue}} ? {{argName}}({{value}}) : new(default, {{error}});
               {{attributes}}
               public {{Expect[V, E]}} OrElse({{Func[E, Expect[V, E]]}} {{argName}})
                  => {{hasValue}} ? new({{value}}) : {{argName}}({{error}});
            """;
      }
      string conversions() {
         const string argName = "v";
         return $$"""

               [{{aggressiveInlining}}]
               public static implicit operator {{Expect[V, E]}}({{info.GenericName}} {{argName}})
                  => {{argName}}.{{hasValue}} ? new({{argName}}.{{value}}) : new(default, {{argName}}.{{error}});
               [{{aggressiveInlining}}]
               public static implicit operator {{info.GenericName}}({{Expect[V, E]}} {{argName}})
                  => {{argName}}.HasValue ? new({{argName}}.Value) : new(default, {{argName}}.Error);

            """;

      }
      return $$"""
         {{(info.Namespace is null ? "" : $"namespace {info.Namespace};")}}
         [global::Expected.CouldBeUnexpected]
         partial {{info.TypeMod}} {{info.GenericName}} {
         {{makeField(t.TValue, value)}}
         {{makeField(t.TError, error)}}
         {{makeField("bool", hasValue)}}
         {{makeProperty(t.TValue, "Value",
            get: $"{hasValue} ? {value} : {throwBadAccess}",
            set: $"{hasValue} = true; {value} = value"
         )}}
         {{makeProperty(t.TError, "Error",
            get: $"{hasValue} ? {throwBadAccess} : {error}",
            set: $"{hasValue} = false; {error} = value"
         )}}
            public {{@readonly}} bool HasValue => {{hasValue}}; 
            public {{@readonly}} {{t.TValue}} ValueOr({{t.TValue}} value) => {{hasValue}} ? {{value}} : value;
            public {{@readonly}} {{t.TError}} ErrorOr({{t.TError}} error) => {{hasValue}} ? error : {{error}};

            [{{aggressiveInlining}}]
            public {{info.Name}}({{t.TValue}} value) {
               {{hasValue}} = true;
               {{error}} = default!;
               {{value}} = value;
            }
            [{{aggressiveInlining}}]
            public {{info.Name}}({{Unexpect}} unexpect, {{E}} error) {
               {{hasValue}} = false;
               {{value}} = default!;
               {{error}} = error;
            }
            [{{aggressiveInlining}}]
            public static implicit operator {{info.GenericName}}({{V}} value)
               => new(value);
            [{{aggressiveInlining}}]
            public static implicit operator {{info.GenericName}}({{Unexpected[E]}} unexpected)
               => new(default, unexpected.Error);
            [{{aggressiveInlining}}]
            public static bool operator true({{info.GenericName}} expected)
               => expected.{{hasValue}};
            [{{aggressiveInlining}}]
            public static bool operator false({{info.GenericName}} expected)
               => !expected.{{hasValue}};
            [{{aggressiveInlining}}]
            public static bool operator !({{info.GenericName}} expected)
               => !expected.{{hasValue}};

            public static {{t.TValue}} operator +({{info.GenericName}} expected)
               => expected.{{hasValue}} ? expected.{{value}} : {{throwBadAccess}};
            public static {{t.TError}} operator -({{info.GenericName}} expected)
               => expected.{{hasValue}} ? {{throwBadAccess}} : expected.{{error}};

            {{fluentMethods("global::Expected.ScopedInFunc")}}
            {{fluentMethods("global::System.Func")}}
            {{conversions()}}
         }
         """;
   }
}
