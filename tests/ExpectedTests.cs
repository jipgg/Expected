using Expected;
namespace Tests;

using Expected = Expected<Foo, Bar>;
using RefExpected = RefExpected<RefFoo, RefBar>;

public class Expected_Tests {
   [Fact]
   public void HasValue_when_constructed_with_value() {
      var e = new Expected(new Foo(42));
      Assert.True(e.HasValue);
      Assert.False(e.HasError);
      Assert.Equal(42, e.Value.X);
   }

   [Fact]
   public void HasError_when_constructed_with_unexpected() {
      var e = new Expected(new Unexpected<Bar>(new("err")));
      Assert.False(e.HasValue);
      Assert.True(e.HasError);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void Default_is_error_with_default_payload() {
      var e = default(Expected);
      Assert.False(e.HasValue);
      Assert.True(e.HasError);
   }
   [Fact]
   public void Explicit_cast_throws_when_empty() {
      var o = default(Expected);
      Action act = () => { var _ = (Foo)o; };
      Assert.ThrowsAny<InvalidOperationException>(act);
   }
}
public class Expected_MapTests {
   [Fact]
   public void Map_applies_only_on_value() {
      var e = new Expected(new Foo(10))
          .Transform(f => new Foo(f.X * 2));

      Assert.True(e.HasValue);
      Assert.Equal(20, e.Value.X);
   }

   [Fact]
   public void Map_does_not_apply_on_error() {
      var e = new Expected(
          new Unexpected<Bar>(new("err"))
      ).Transform(f => new Foo(f.X * 2));

      Assert.True(e.HasError);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void MapError_applies_only_on_error() {
      var e = new Expected(
          new Unexpected<Bar>(new Bar("abc"))
      ).TransformError(err => new Bar("xyz"));

      Assert.True(e.HasError);
      Assert.Equal("xyz", e.Error.Msg);
   }
}
public class Expected_BindTests {
   static Expected Increment(Foo v)
       => new Expected(new Foo(v.X + 1));

   [Fact]
   public void AndThen_left_identity() {
      var e = new Expected(new Foo(5))
          .AndThen(Increment);

      Assert.Equal(6, e.Value.X);
   }

   [Fact]
   public void AndThen_right_identity() {
      var e = new Expected(new Foo(7));
      var bound = e.AndThen(x => new Expected(x));

      Assert.True(bound.HasValue);
      Assert.Equal(7, bound.Value.X);
   }

   [Fact]
   public void OrElse_applies_on_error() {
      var e = new Expected(
          new Unexpected<Bar>(new("err"))
      ).OrElse(_ => new Expected(new Foo(99)));

      Assert.True(e.HasValue);
      Assert.Equal(99, e.Value.X);
   }

   [Fact]
   public void OrElse_skips_on_value() {
      var e = new Expected(new Foo(1))
          .OrElse(_ => new Expected(new Foo(2)));

      Assert.Equal(1, e.Value.X);
   }
}
//@RefExpected
public class RefExpected_Tests {
   [Fact]
   public void HasValue_when_constructed_with_value() {
      var e = new RefExpected(new RefFoo(42));
      Assert.True(e.HasValue);
      Assert.False(e.HasError);
      Assert.Equal(42, e.Value.X);
   }

   [Fact]
   public void HasError_when_constructed_with_unexpected() {
      var e = new RefExpected(
          new Unexpected<RefBar>(new("err"))
      );

      Assert.False(e.HasValue);
      Assert.True(e.HasError);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void Default_is_error_with_default_payload() {
      var e = default(RefExpected);
      Assert.False(e.HasValue);
      Assert.True(e.HasError);
   }
   [Fact]
   public void Explicit_cast_throws_when_empty() {
      Action act = () => {
         var e = default(RefExpected);
         var _ = (RefFoo)e;
      };
      Assert.ThrowsAny<InvalidOperationException>(act);
   }
}
public class Expected_TransformerTests {
   struct Tr : ITransformer<Foo, Foo> {
      public Foo Transform(Foo f) => new Foo(f.X * 2);
   }
   struct TrExp : ITransformer<Foo, Expected> {
      public Expected Transform(Foo f) => new(new Foo(f.X * 2));
   }

   [Fact]
   public void Map_with_ITransformer_works() {
      var e = new Expected(new Foo(4))
          .Transform<Tr, Foo>(default);

      Assert.Equal(8, e.Value.X);
   }

   [Fact]
   public void AndThen_with_ITransformer_works() {
      var e = new Expected(new Foo(4))
          .AndThen<TrExp, Foo>(default);
      Assert.Equal(8, e.Value.X);
   }
}
public class RefExpected_MapTests {
   [Fact]
   public void Map_applies_only_on_value() {
      var e = new RefExpected(new RefFoo(10))
          .Transform(f => new RefFoo(f.X * 2));

      Assert.True(e.HasValue);
      Assert.Equal(20, e.Value.X);
   }

   [Fact]
   public void Map_does_not_apply_on_error() {
      var e = new RefExpected(
          new Unexpected<RefBar>(new("err"))
      ).Transform(f => new RefFoo(f.X * 2));

      Assert.True(e.HasError);
      Assert.Equal("err", e.Error.Msg);
   }

   [Fact]
   public void MapError_applies_only_on_error() {
      var e = new RefExpected(
          new Unexpected<RefBar>(new("abc"))
      ).TransformError(err => new RefBar("xyz"));

      Assert.True(e.HasError);
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
public class RefExpected_TransformerTests {
   struct Tr : ITransformer<RefFoo, RefFoo> {
      public RefFoo Transform(RefFoo f) => new RefFoo(f.X * 2);
   }
   struct TrExp : ITransformer<RefFoo, RefExpected> {
      public RefExpected Transform(RefFoo f) => new(new RefFoo(f.X * 2));
   }

   [Fact]
   public void Map_with_ITransformer_works() {
      var e = new RefExpected(new RefFoo(4))
          .Transform<Tr, RefFoo>(default);

      Assert.Equal(8, e.Value.X);
   }

   [Fact]
   public void AndThen_with_ITransformer_works() {
      var e = new RefExpected(new RefFoo(4))
          .AndThen<TrExp, RefFoo>(default);
      Assert.Equal(8, e.Value.X);
   }
}
