namespace Expected.Generators.Templates;

static class ExpectedTemplate {
   public static string Apply(ExpectedParams t) {
      var info = t.ClassInfo;

      var paramMod = info.IsStruct ? "scoped in " : " ";
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
      static string unexpected(string tError) => $"{expectedNs}.Unexpected<{tError}>";
      const string hasValue = "_hasValue";
      const string value = "_value";
      const string error = "_error";
      const string R = "TResult";
      string makeMethods(string name, bool @implicit = true, bool @async = true, string vWhere = "", string eWhere = "") {
         var V = t.TValue;
         var E = t.TError;
         static string func(string first, params string[] rest) => $"global::System.Func<{first}, {string.Join(", ", rest)}>";
         string expected(string v, string e) => $"{name}<{v}, {e}>";
         var source = $$"""
               public {{expected(R, E)}} Select<{{R}}>({{func(V, R)}} selector) {{vWhere}}
                  => {{hasValue}} ? new(selector({{value}})) : new(new {{unexpected(E)}}({{error}}));
               public {{expected(V, R)}} SelectError<{{R}}>({{func(E, R)}} selector) {{eWhere}}
                  => {{hasValue}} ? new({{value}}) : new(new {{unexpected(R)}}(selector({{error}})));
               public {{expected(V, E)}} AndThen({{func(V, expected(V, E))}} selector)
                  => {{hasValue}} ? selector({{value}}) : this;
               public {{expected(R, E)}} AndThen<{{R}}>(Func<{{V}}, {{expected(R, E)}}> selector) {{vWhere}}
                  => {{hasValue}} ? selector(_value) : new(new {{unexpected(E)}}(_error));
               public {{expected(V, E)}} OrElse(Func<{{E}}, {{expected(V, E)}}> selector)
                  => {{hasValue}} ? this : selector(_error);
               public {{expected(V, R)}} OrElse<{{R}}>(Func<{{E}}, {{expected(V, R)}}> selector) {{eWhere}}
                  => {{hasValue}} ? new(_value) : selector(_error);

            """;
         if (@implicit) {
            source += $$"""
               public static implicit operator {{expected(V, E)}}(scoped in {{info.GenericName}} expected)
                  => expected.{{hasValue}} ? new(expected.{{value}}) : new(new {{unexpected(E)}}(expected.{{error}}));
               public static implicit operator {{info.GenericName}}(scoped in {{expected(V, E)}} expected)
                  => expected ? new(+expected) : new(new {{unexpected(E)}}(-expected));

            """;
         }
         if (@async) {
            const string tasksNamespace = "global::System.Threading.Tasks";
            static string valueTask(string t) => $"{tasksNamespace}.ValueTask<{t}>";
            static string task(string t) => $"{tasksNamespace}.Task<{t}>";
            source += $$"""
               public {{valueTask(expected(V, E))}} AndThen({{func(V, task(expected(V, E)))}} selector) 
                  => {{hasValue}} ? new(selector({{value}})) : new(this);

               public {{valueTask(expected(R, E))}} AndThen<{{R}}>({{func(V, task(expected(R, E)))}} selector) {{vWhere}}
                  => {{hasValue}} ? new(selector({{value}})) : new(new {{unexpected(E)}}({{error}}));

               public {{valueTask(expected(V, E))}} OrElse({{func(E, task(expected(V, E)))}} selector)
                  => {{hasValue}} ? new(this) : new(selector({{error}}));
               public {{valueTask(expected(V, R))}} OrElse<{{R}}>({{func(E, task(expected(V, R)))}} selector) {{eWhere}}
                  => {{hasValue}} ? new({{value}}) : new(selector({{error}}));

            """;

         }
         return source;
      }
      string resolveMethods() => t switch {
         { IsCanonical: true } => makeMethods(
               info.Name,
               @implicit: false,
               @async: !info.StructIsRef,
               info.TypeParams.FirstOrDefault(e => e.Name == t.TValue)?.WhereClause(R) ?? "",
               info.TypeParams.FirstOrDefault(e => e.Name == t.TError)?.WhereClause(R) ?? ""
            ),
         { ClassInfo.IsStruct: true, ClassInfo.StructIsRef: true } => makeMethods(
               $"{expectedNs}.RefExpected",
               @implicit: true,
               @async: false,
               $"where {R}: allows ref struct",
               $"where {R}: allows ref struct"
            ),
         { ClassInfo.IsStruct: true } => makeMethods($"{expectedNs}.ValueExpected"),
         _ => makeMethods($"{expectedNs}.Expected"),
      };
      const string overloadResolution = "global::System.Runtime.CompilerServices.OverloadResolutionPriority(1)";
      const string aggressiveInlining
         = "global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)";
      return $$"""
         {{(info.Namespace is null ? "" : $"namespace {info.Namespace};")}}
         [{{expectedNs}}.CouldBeUnexpected]
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
            public {{@readonly}} {{t.TValue}} ValueOr(scoped in {{t.TValue}} value) => {{hasValue}} ? {{value}} : value;
            public {{@readonly}} {{t.TError}} ErrorOr(scoped in {{t.TError}} error) => {{hasValue}} ? error : {{error}};

            [{{aggressiveInlining}}]
            [{{overloadResolution}}]
            public {{info.Name}}(scoped in {{t.TValue}} value) {
               {{hasValue}} = true;
               {{error}} = default!;
               {{value}} = value;
            }
            [{{aggressiveInlining}}]
            [{{overloadResolution}}]
            public {{info.Name}}({{t.TValue}} value) {
               {{hasValue}} = true;
               {{error}} = default!;
               {{value}} = value;
            }
            [{{aggressiveInlining}}]
            public {{info.Name}}(scoped in {{unexpected(t.TError)}} unexpected) {
               {{hasValue}} = false;
               {{value}} = default!;
               {{error}} = unexpected.Error;
            }
            [{{aggressiveInlining}}]
            public static implicit operator {{info.GenericName}}(scoped in {{t.TValue}} value)
               => new(value);
            [{{aggressiveInlining}}]
            public static implicit operator {{info.GenericName}}(scoped in {{unexpected(t.TError)}} unexpected)
               => new(unexpected);
            [{{aggressiveInlining}}]
            public static bool operator true({{paramMod}}{{info.GenericName}} expected)
               => expected.{{hasValue}};
            [{{aggressiveInlining}}]
            public static bool operator false({{paramMod}}{{info.GenericName}} expected)
               => !expected.{{hasValue}};
            [{{aggressiveInlining}}]
            public static bool operator !({{paramMod}}{{info.GenericName}} expected)
               => !expected.{{hasValue}};
            public static {{t.TValue}} operator +({{paramMod}}{{info.GenericName}} expected)
               => expected.{{hasValue}} ? expected.{{value}} : {{throwBadAccess}};
            public static {{t.TError}} operator -({{paramMod}}{{info.GenericName}} expected)
               => expected.{{hasValue}} ? {{throwBadAccess}} : expected.{{error}};

         {{resolveMethods()}}
         }
         """;
   }
}
