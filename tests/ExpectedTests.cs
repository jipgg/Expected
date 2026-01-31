using Expected;
namespace Tests;

[Expects<ObjFoo>, Unexpects<ObjBar>]
partial record Expected;
public class Expected_Tests {
   [Fact]
   public void HasValue_when_constructed_with_value() {
      var e = new Expected(new(42));
      Assert.True(e.HasValue);
      Assert.Equal(42, e.Value.X);
   }

   [Fact]
   public void HasError_when_constructed_with_unexpected() {
      Expected e = new Unexpected<ObjBar>(new("err"));
      Assert.False(e.HasValue);
      Assert.Equal("err", e.Error.Msg);
   }
}
public class Expected_MapTests {
   [Fact]
   public void Map_applies_only_on_value() {
      var e = new Expected(new(10))
          .Select(f => new Foo(f.X * 2));

      Assert.True(e.HasValue);
      Assert.Equal(20, e.Value.X);
   }

   [Fact]
   public void Map_does_not_apply_on_error() {
      var e = new Expected(default, new("err"))
      .Select(f => new Foo(f.X * 2));

      Assert.False(e.HasValue);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void MapError_applies_only_on_error() {
      var e = new Expected(default, new ObjBar("abc"))
         .SelectError(err => new Bar("xyz"));

      Assert.False(e.HasValue);
      Assert.Equal("xyz", e.Error.Msg);
   }
}
public class Expected_BindTests {
   static Expected Increment(ObjFoo v)
       => new ObjFoo(v.X + 1);

   [Fact]
   public void AndThen_left_identity() {
      var e = new Expected(new ObjFoo(5))
          .AndThen(v => Increment(v));

      Assert.Equal(6, e.Value.X);
   }

   [Fact]
   public void AndThen_right_identity() {
      var e = new Expected(new ObjFoo(7));
      var bound = e.AndThen(x => new Expected(x));

      Assert.True(bound.HasValue);
      Assert.Equal(7, bound.Value.X);
   }

   [Fact]
   public void OrElse_applies_on_error() {
      var e = new Expected(default, new("err"))
         .OrElse(_ => new Expected(new ObjFoo(99)));

      Assert.True(e.HasValue);
      Assert.Equal(99, e.Value.X);
   }

   [Fact]
   public void OrElse_skips_on_value() {
      var e = new Expected(new ObjFoo(1))
          .OrElse(_ => new ObjFoo(2));

      Assert.Equal(1, e.Value.X);
   }
}
