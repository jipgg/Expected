namespace Expected.Generators.Templates;

static class ExpectedTemplate {
   const string AggressiveInlining = "MethodImpl(MethodImplOptions.AggressiveInlining)";
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
      const string R = "TResult";
      string makeMethods(string name, bool @implicit = true, bool @async = true, string vWhere = "", string eWhere = "") {
         var V = t.TValue;
         var E = t.TError;
         string generic(string? v = null, string? e = null) => $"{name}<{v ?? V}, {e ?? E}>";
         string unexpected(string? e = null) => $"new Unexpected<{e ?? E}>";
         var source = $$"""
               public {{generic(R, E)}} Select<{{R}}>(Func<{{V}}, {{R}}> selector) {{vWhere}}
                  => HasValue ? new(selector(_value)) : new({{unexpected()}}(_error));
               public {{generic(V, R)}} SelectError<{{R}}>(Func<{{E}}, {{R}}> selector) {{eWhere}}
                  => HasValue ? new(_value) : new({{unexpected(R)}}(selector(_error)));
               public {{generic()}} AndThen(Func<{{V}}, {{generic()}}> selector)
                  => HasValue ? selector(_value) : this;
               public {{generic(R, E)}} AndThen<{{R}}>(Func<{{V}}, {{generic(R, E)}}> selector) {{vWhere}}
                  => HasValue ? selector(_value) : new({{unexpected()}}(_error));
               public {{generic()}} OrElse(Func<{{E}}, {{generic()}}> selector)
                  => HasValue ? this : selector(_error);
               public {{generic(V, R)}} OrElse<{{R}}>(Func<{{E}}, {{generic(V, R)}}> selector) {{eWhere}}
                  => HasValue ? new(_value) : selector(_error);

            """;
         if (@implicit) {
            source += $$"""
               public static implicit operator {{generic()}}(scoped in {{info.GenericName}} e)
                  => e._hasValue ? new(e._value) : new({{unexpected()}}(e._error));
               public static implicit operator {{info.GenericName}}(scoped in {{generic()}} e)
                  => e.HasValue ? new(e.Value) : new({{unexpected()}}(e.Error));

            """;
         }
         if (@async) {
            source += $$"""
               public ValueTask<{{generic()}}> AndThen(Func<{{V}}, Task<{{generic()}}>> selector) 
                  => HasValue ? new(selector(_value)) : new(this);

               public ValueTask<{{generic(R, E)}}> AndThen<{{R}}>(Func<{{V}}, Task<{{generic(R, E)}}>> selector) {{vWhere}}
                  => HasValue ? new(selector(_value)) : new({{unexpected()}}(_error));

               public ValueTask<{{generic()}}> OrElse(Func<{{E}}, Task<{{generic()}}>> selector)
                  => HasValue ? new(this) : new(selector(_error));
               public ValueTask<{{generic(V, R)}}> OrElse<{{R}}>(Func<{{E}}, Task<{{generic(V, R)}}>> selector) {{eWhere}}
                  => HasValue ? new(_value) : new(selector(_error));

            """;

         }
         return source;
      }
      string resolveMethods() => t switch {
         { IsCanonical: true } => makeMethods(
               info.Name,
               @implicit: false,
               @async: info.Name is not "RefExpected",
               info.TypeParams.FirstOrDefault(e => e.Name == t.TValue)?.WhereClause(R) ?? "",
               info.TypeParams.FirstOrDefault(e => e.Name == t.TError)?.WhereClause(R) ?? ""
            ),
         { ClassInfo.IsStruct: true, ClassInfo.StructIsRef: true } => makeMethods(
               "RefExpected",
               @implicit: true,
               @async: false,
               $"where {R}: allows ref struct",
               $"where {R}: allows ref struct"
            ),
         { ClassInfo.IsStruct: true, ClassInfo.StructIsRef: false } => makeMethods("ValueExpected"),
         _ => makeMethods("Expected"),
      };
      return $$"""
         using System.Runtime.CompilerServices;
         using System.Diagnostics.CodeAnalysis;
         using Expected;
         namespace {{info.Namespace}};
         [CouldBeUnexpected]
         partial {{info.TypeMod}} {{info.GenericName}} {
         {{makeField(t.TValue, "_value")}}
         {{makeField(t.TError, "_error")}}
         {{makeField("bool", "_hasValue")}}
         {{makeProperty(t.TValue, "Value",
            get: "_hasValue ? _value : throw new BadExpectedAccess()",
            set: "_hasValue = true; _value = value"
         )}}
         {{makeProperty(t.TError, "Error",
            get: "_hasValue ? throw new BadExpectedAccess() : _error",
            set: "_hasValue = false; _error = value"
         )}}
            public {{@readonly}} bool HasValue => _hasValue; 
            public {{@readonly}} {{t.TValue}} ValueOr(scoped in {{t.TValue}} v) => HasValue ? _value : v;
            public {{@readonly}} {{t.TError}} ErrorOr(scoped in {{t.TError}} e) => HasValue ? e : _error;

            [{{AggressiveInlining}}, OverloadResolutionPriority(1)]
            public {{info.Name}}(scoped in {{t.TValue}} value) {
               _hasValue = true;
               _error = default!;
               _value = value;
            }
            [{{AggressiveInlining}}, OverloadResolutionPriority(1)]
            public {{info.Name}}({{t.TValue}} value) {
               _hasValue = true;
               _error = default!;
               _value = value;
            }
            [{{AggressiveInlining}}]
            public {{info.Name}}(scoped in Unexpected<{{t.TError}}> u) {
               _hasValue = false;
               _value = default!;
               _error = u.Error;
            }
            [{{AggressiveInlining}}]
            public static implicit operator {{info.GenericName}}(scoped in {{t.TValue}} v) => new(v);
            [{{AggressiveInlining}}]
            public static implicit operator {{info.GenericName}}(scoped in Unexpected<{{t.TError}}> u) => new(u);
            [{{AggressiveInlining}}]
            public static bool operator true({{paramMod}}{{info.GenericName}} r) => r.HasValue;
            [{{AggressiveInlining}}]
            public static bool operator false({{paramMod}}{{info.GenericName}} r) => !r.HasValue;
            [{{AggressiveInlining}}]
            public static bool operator !({{paramMod}}{{info.GenericName}} r) => !r.HasValue;
            public static {{t.TValue}} operator +({{paramMod}}{{info.GenericName}} r) => r.Value;
            public static {{t.TError}} operator -({{paramMod}}{{info.GenericName}} r) => r.Error;

         {{resolveMethods()}}
         }
         """;
   }
}
