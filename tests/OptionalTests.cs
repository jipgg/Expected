using Expected;

namespace Tests;

using Optional = Optional<Foo>;
using RefOptional = RefOptional<RefFoo>;
using InvalidAccess = InvalidOperationException;

public class Optional_Tests {
   [Fact]
   public void HasValue_when_constructed_with_value() {
      var o = new Optional(new Foo(42));
      Assert.True(o.HasValue);
      Assert.Equal(42, o.Value.X);
   }

   [Fact]
   public void Default_has_no_value() {
      var o = default(Optional);
      Assert.False(o.HasValue);
   }

   [Fact]
   public void Explicit_cast_throws_when_empty() {
      var o = default(Optional);
      Action act = () => { var _ = (Foo)o; };
      Assert.ThrowsAny<InvalidAccess>(act);
   }
}

public class Optional_MapTests {
   [Fact]
   public void Map_applies_only_when_has_value() {
      var o = new Optional(new Foo(10))
          .Transform(f => new Foo(f.X * 2));

      Assert.True(o.HasValue);
      Assert.Equal(20, o.Value.X);
   }

   [Fact]
   public void Map_skips_when_empty() {
      var o = default(Optional)
          .Transform(f => new Foo(f.X * 2));

      Assert.False(o.HasValue);
   }
}

public class Optional_BindTests {
   static Optional Inc(Foo f)
       => new Optional(new Foo(f.X + 1));

   [Fact]
   public void AndThen_left_identity() {
      var o = new Optional(new Foo(5))
          .AndThen(Inc);

      Assert.True(o.HasValue);
      Assert.Equal(6, o.Value.X);
   }

   [Fact]
   public void AndThen_right_identity() {
      var o = new Optional(new Foo(7));
      var bound = o.AndThen(x => new Optional(x));

      Assert.True(bound.HasValue);
      Assert.Equal(7, bound.Value.X);
   }

   [Fact]
   public void AndThen_skips_when_empty() {
      var o = default(Optional)
          .AndThen(Inc);

      Assert.False(o.HasValue);
   }
}

// @RefOptional
public class RefOptional_Tests {
   [Fact]
   public void HasValue_when_constructed_with_value() {
      var o = new RefOptional(new RefFoo(42));
      Assert.True(o.HasValue);
      Assert.Equal(42, o.Value.X);
   }

   [Fact]
   public void Default_has_no_value() {
      var o = default(RefOptional);
      Assert.False(o.HasValue);
   }

   [Fact]
   public void Explicit_cast_throws_when_empty() {
      Action act = () => {
         var o = default(RefOptional);
         var _ = (RefFoo)o;
      };
      Assert.ThrowsAny<InvalidAccess>(act);
   }
}

public class RefOptional_MapTests {
   [Fact]
   public void Map_applies_only_when_has_value() {
      var o = new RefOptional(new RefFoo(10))
          .Transform(f => new RefFoo(f.X * 2));

      Assert.True(o.HasValue);
      Assert.Equal(20, o.Value.X);
   }

   [Fact]
   public void Map_skips_when_empty() {
      var o = default(RefOptional)
          .Transform(f => new RefFoo(f.X * 2));

      Assert.False(o.HasValue);
   }
}

public class RefOptional_BindTests {
   static RefOptional Inc(RefFoo f)
       => new(new RefFoo(f.X + 1));

   [Fact]
   public void AndThen_left_identity() {
      var o = new RefOptional(new RefFoo(5))
          .AndThen(Inc);

      Assert.Equal(6, o.Value.X);
   }

   [Fact]
   public void AndThen_right_identity() {
      var o = new RefOptional(new RefFoo(7));
      var bound = o.AndThen(x => new RefOptional(x));

      Assert.True(bound.HasValue);
      Assert.Equal(7, bound.Value.X);
   }

   [Fact]
   public void AndThen_skips_when_empty() {
      var o = default(RefOptional)
          .AndThen(Inc);

      Assert.False(o.HasValue);
   }
}
