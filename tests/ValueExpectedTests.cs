using Expected;
namespace Tests;

using ValueExpected = ValueExpected<Foo, Bar>;
public class ValueExpected_Tests {
   [Fact]
   public void HasValue_when_constructed_with_value() {
      var e = new ValueExpected(new Foo(42));
      Assert.True(e.HasValue);
      Assert.Equal(42, e.Value.X);
   }

   [Fact]
   public void HasError_when_constructed_with_unexpected() {
      ValueExpected e = new Unexpected<Bar>(new("err"));
      Assert.False(e.HasValue);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void Default_is_error_with_default_payload() {
      var e = default(ValueExpected);
      Assert.False(e.HasValue);
   }
   [Fact]
   public void Explicit_cast_throws_when_empty() {
      var o = default(ValueExpected);
      Action act = () => { var _ = +o; };
      Assert.ThrowsAny<InvalidOperationException>(act);
   }
}
public class ValueExpected_MapTests {
   [Fact]
   public void Map_applies_only_on_value() {
      var e = new ValueExpected(new Foo(10))
          .Select(f => new Foo(f.X * 2));

      Assert.True(e.HasValue);
      Assert.Equal(20, e.Value.X);
   }

   [Fact]
   public void Map_does_not_apply_on_error() {
      var e = new ValueExpected(default, new("err"))
         .Select(f => new Foo(f.X * 2));

      Assert.False(e.HasValue);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void MapError_applies_only_on_error() {
      var e = new ValueExpected(default, new Bar("abc"))
         .SelectError(err => new Bar("xyz"));

      Assert.False(e.HasValue);
      Assert.Equal("xyz", e.Error.Msg);
   }
}
public class ValueExpected_BindTests {
   static ValueExpected Increment(Foo v)
       => new ValueExpected(new Foo(v.X + 1));

   [Fact]
   public void AndThen_left_identity() {
      var e = new ValueExpected(new Foo(5))
          .AndThen(e => Increment(e));
      Assert.Equal(6, e.Value.X);
   }

   [Fact]
   public void AndThen_right_identity() {
      var e = new ValueExpected(new Foo(7));
      var bound = e.AndThen(x => new ValueExpected(x));

      Assert.True(bound.HasValue);
      Assert.Equal(7, bound.Value.X);
   }

   [Fact]
   public void OrElse_applies_on_error() {
      var e = new ValueExpected(default, new("err"))
         .OrElse(_ => new ValueExpected(new Foo(99)));

      Assert.True(e.HasValue);
      Assert.Equal(99, e.Value.X);
   }

   [Fact]
   public void OrElse_skips_on_value() {
      var e = new ValueExpected(new Foo(1))
          .OrElse(_ => new ValueExpected(new Foo(2)));

      Assert.Equal(1, e.Value.X);
   }
}
