using Expected;
namespace Tests;
using RefExpected = RefExpected<RefFoo, RefBar>;

public class RefExpected_Tests {
   [Fact]
   public void HasValue_when_constructed_with_value() {
      var e = new RefExpected(new RefFoo(42));
      Assert.True(e.HasValue);
      Assert.Equal(42, e.Value.X);
   }

   [Fact]
   public void HasError_when_constructed_with_unexpected() {
      var e = new RefExpected(
          new Unexpected<RefBar>(new("err"))
      );

      Assert.False(e.HasValue);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void Default_is_error_with_default_payload() {
      var e = default(RefExpected);
      Assert.False(e.HasValue);
   }
   [Fact]
   public void Explicit_cast_throws_when_empty() {
      Action act = () => {
         var e = default(RefExpected);
         var _ = +e;
      };
      Assert.ThrowsAny<InvalidOperationException>(act);
   }
}
public class RefExpected_MapTests {
   [Fact]
   public void Map_applies_only_on_value() {
      var e = new RefExpected(new RefFoo(10))
          .Select(f => new RefFoo(f.X * 2));

      Assert.True(e.HasValue);
      Assert.Equal(20, e.Value.X);
   }

   [Fact]
   public void Map_does_not_apply_on_error() {
      var e = new RefExpected(
          new Unexpected<RefBar>(new("err"))
      ).Select(f => new RefFoo(f.X * 2));

      Assert.False(e.HasValue);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void MapError_applies_only_on_error() {
      var e = new RefExpected(
          new Unexpected<RefBar>(new("abc"))
      ).SelectError(err => new RefBar("xyz"));

      Assert.False(e.HasValue);
      Assert.Equal("xyz", e.Error.Msg);
   }
}

public class RefExpected_BindTests {
   static RefExpected Increment(RefFoo f) => new(new RefFoo(f.X + 1));

   [Fact]
   public void AndThen_left_identity() {
      var e = new RefExpected(new RefFoo(5))
          .AndThen(Increment);

      Assert.Equal(6, e.Value.X);
   }

   [Fact]
   public void AndThen_right_identity() {
      var e = new RefExpected(new RefFoo(7));
      var bound = e.AndThen(x => new RefExpected(x));

      Assert.True(e.HasValue);
      Assert.Equal(7, e.Value.X);
   }

   [Fact]
   public void OrElse_applies_on_error() {
      var e = new RefExpected(
          new Unexpected<RefBar>(new("err"))
      ).OrElse(_ => new RefExpected(new RefFoo(99)));

      Assert.True(e.HasValue);
      Assert.Equal(99, e.Value.X);
   }

   [Fact]
   public void OrElse_skips_on_value() {
      var e = new RefExpected(new RefFoo(1))
          .OrElse(_ => new RefExpected(new RefFoo(2)));

      Assert.Equal(1, e.Value.X);
   }
}
