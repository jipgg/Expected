namespace Expected.Generators.Templates;

class ErrorCodeTemplate {
   public static string Apply(ErrorCodeParams args) {
      var (@namespace, @enum, category, codes, title, messageImpl) = args;
      var (type, fields) = @enum;

      const string errorCode = "global::Expected.ErrorCode";

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

      var getMessageImpl = messageImpl switch {
         MessageImplOptions.Name => expandSwitch("GetMessage",
            name => $"""{value(name)} => "{name}","""),
         MessageImplOptions.FullName => expandSwitch("GetMessage",
            name => $"""{value(name)} => "{(@namespace is null ? "" : $"{@namespace}.")}{value(name)}","""),
         MessageImplOptions.Partial or _ => "",
      };
      var @partial = messageImpl is MessageImplOptions.Partial ? " partial" : "";

      const string errorCategory = "global::Expected.ErrorCategory";
      return $$"""""
      {{(@namespace is null ? "" : $"namespace {@namespace};")}}
      public sealed{{@partial}} class {{category}}: {{errorCategory}} {
         public override string Title => "{{title}}";
      {{getMessageImpl}}
         static {{category}} _instance = null!;
         public static {{category}} Instance {
            get {
               if (_instance is null) _instance = new();
               return _instance;
            }
         }
      }
      {{(codes is not null ? $$"""
      public static class {{codes}} {
      {{expandStatic(1)}}
      }
      """ : "")}}
      public static class ErrorCode{{type}}Extensions {
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
