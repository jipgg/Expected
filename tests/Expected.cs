using Expected;
using System.Runtime.InteropServices;
namespace Tests.Expected;
using Substitute = long;

// invariant rules
public abstract class TestStateInvariant<Expected, V, E>
where Expected : IExpected<Expected, V, E>
where V : new()
where E : new() {
   public static void AssertValue(V value, Expected subject) {
      Assert.True(subject.HasValue);
      Assert.True(subject ? true : false);
#pragma warning disable xUnit2022
      Assert.False(!subject);
#pragma warning restore xUnit2022
      Assert.Equal(value, subject.Value);
      Assert.Equal(value, +subject);
      Assert.Throws<BadExpectedAccess>(() => subject.Error);
      Assert.Throws<BadExpectedAccess>(() => -subject);
   }
   public static void AssertError(E error, Expected subject) {
      Assert.False(subject.HasValue);
      Assert.False(subject ? true : false);
#pragma warning disable xUnit2022
      Assert.True(!subject);
#pragma warning restore xUnit2022
      Assert.Equal(error, subject.Error);
      Assert.Equal(error, -subject);
      Assert.Throws<BadExpectedAccess>(() => subject.Value);
      Assert.Throws<BadExpectedAccess>(() => +subject);
   }
   [Fact]
   public void FromValue() {
      var value = new V();
      AssertValue(value, new Expected<V, E>(value));
   }
   [Fact]
   public void FromError() {
      var error = new E();
      AssertError(error, new Unexpected<E>(error));
   }
   [Fact]
   public void Select() {
      var value = new V();
      Expected expected = new Expected<V, E>(value);
      AssertValue(value, expected
         .Select(static value => value));
      AssertValue(value, expected
         .Select(static (scoped in value) => value));
   }
   [Fact]
   public void SelectError() {
      var error = new E();
      Expected expected = new Unexpected<E>(error);
      AssertError(error, expected
         .SelectError(static error => error));
      AssertError(error, expected
         .SelectError(static (scoped in error) => error));
   }
   [Fact]
   public void AndThen() {
      var value = new V();
      Expected expected = new Expected<V, E>(value);
      AssertValue(value, expected
         .AndThen(v => expected));
      AssertValue(value, expected
         .AndThen((scoped in v) => expected));
      AssertValue(value, expected
         .AndThen<V>(v => expected));
      AssertValue(value, expected
         .AndThen<V>((scoped in v) => expected));
   }
   [Fact]
   public void OrElse() {
      var error = new E();
      Expected expected = new Unexpected<E>(error);
      AssertError(error, expected
         .OrElse(e => expected));
      AssertError(error, expected
         .OrElse((scoped in e) => expected));
      AssertError(error, expected
         .OrElse<E>(e => expected));
      AssertError(error, expected
         .OrElse<E>((scoped in e) => expected));
   }
}
public abstract class TestStateInvariantMutable<Expected, V, E> : TestStateInvariant<Expected, V, E>
where Expected : IExpected<Expected, V, E>, IMutableExpected<Expected, V, E>
where V : new() where E : new() {
   [Fact]
   public void ValueAssignment() {
      var value = new V();
      Expected expected = new Unexpected<E>(new());
      expected.Value = value;
      AssertValue(value, expected);
   }
   [Fact]
   public void ErrorAssignment() {
      var error = new E();
      Expected expected = new Expected<V, E>(new());
      expected.Error = error;
      AssertError(error, expected);
   }
}

public record ValueObject(int X) { public ValueObject() : this(1) { } }
public class ErrorObject { public int X; }

// source generated non-generic
[Expected<ValueObject, ErrorObject>]
public partial class ClassWithObjects;
public class TestClassWithObjects : TestStateInvariantMutable<ClassWithObjects, ValueObject, ErrorObject>;

[Expected<ValueObject, ErrorObject>]
public sealed partial class SealedClassWithObjects;
public class TestSealedClassWithObjects : TestStateInvariantMutable<SealedClassWithObjects, ValueObject, ErrorObject>;

[Expected<ValueObject, ErrorObject>]
public partial record RecordWithObjects;
public class TestRecordWithObjects : TestStateInvariant<RecordWithObjects, ValueObject, ErrorObject>;

[Expected<ValueObject, ErrorObject>]
public sealed partial record SealedRecordWithObjects;
public class TestSealedRecordWithObjects : TestStateInvariant<SealedRecordWithObjects, ValueObject, ErrorObject>;

[Expected<ValueObject, ErrorObject>]
public readonly partial record struct RecordStructWithObjects;
public class TestRecordStructWithObjects : TestStateInvariant<RecordStructWithObjects, ValueObject, ErrorObject>;

[Expected<ValueObject, ErrorObject>]
public partial struct StructWithObjects;
public class TestStructWithObjects : TestStateInvariantMutable<StructWithObjects, ValueObject, ErrorObject>;

public struct Value(int x = 1) { public int X = x; }
public readonly record struct Error(object? X = null);

[Expected<Value, Error>]
public partial class ClassWithValues;
public class TestClassWithValues : TestStateInvariantMutable<ClassWithValues, Value, Error>;

[Expected<Value, Error>]
public sealed partial class SealedClassWithValues;
public class TestSealedClassWithValues : TestStateInvariantMutable<SealedClassWithValues, Value, Error>;

[Expected<Value, Error>]
public partial record RecordWithValues;
public class TestRecordWithValues : TestStateInvariant<RecordWithValues, Value, Error>;

[Expected<Value, Error>]
public sealed partial record SealedRecordWithValues;
public class TestSealedRecordWithValues : TestStateInvariant<SealedRecordWithValues, Value, Error>;

[Expected<Value, Error>]
public readonly partial record struct RecordStructWithValues;
public class TestRecordStructWithValues : TestStateInvariant<RecordStructWithValues, Value, Error>;

[Expected<Value, Error>]
public partial struct StructWithValues;
public class TestStructWithValues : TestStateInvariantMutable<StructWithValues, Value, Error>;

[Expected<Value, ErrorObject>]
public partial class ClassMixed;
public class TestClassMixed : TestStateInvariantMutable<ClassMixed, Value, ErrorObject>;

[Expected<ValueObject, Error>]
public sealed partial class SealedClassMixed;
public class TestSealedClassMixed : TestStateInvariantMutable<SealedClassMixed, ValueObject, Error>;

[Expected<Value, ErrorObject>]
public partial record RecordMixed;
public class TestRecordMixed : TestStateInvariant<RecordMixed, Value, ErrorObject>;

[Expected<ValueObject, Error>]
public sealed partial record SealedRecordMixed;
public class TestSealedRecordMixed : TestStateInvariant<SealedRecordMixed, ValueObject, Error>;

[Expected<Value, ErrorObject>]
public readonly partial record struct RecordStructMixed;
public class TestRecordStructMixed : TestStateInvariant<RecordStructMixed, Value, ErrorObject>;

[Expected<ValueObject, Error>]
public partial struct StructMixed;
public class TestStructMixed : TestStateInvariantMutable<StructMixed, ValueObject, Error>;


[StructLayout(LayoutKind.Explicit, Size = 256)]
public struct Unmanaged;

[Expected<int, double>]
public partial class ClassUnmanaged;
public class TestClassUnmanaged : TestStateInvariantMutable<ClassUnmanaged, int, double> {
   // ensuring memory layout overlaps if both are unmanaged types
   [Fact]
   public unsafe void Sizeof() {
      Assert.Equal(sizeof(ClassUnmanaged.Storage), Math.Max(sizeof(int), sizeof(double)));
   }
}

[Expected<byte, Int128>]
public sealed partial class SealedClassUnmanaged;
public class TestSealedClassUnmanaged : TestStateInvariantMutable<SealedClassUnmanaged, byte, Int128> {
   [Fact]
   public unsafe void Sizeof() {
      Assert.Equal(sizeof(SealedClassUnmanaged.Storage), Math.Max(sizeof(byte), sizeof(Int128)));
   }
}

[Expected<double, float>]
public partial record RecordUnmanaged;
public class TestRecordUnmanaged : TestStateInvariant<RecordUnmanaged, double, float> {
   [Fact]
   public unsafe void Sizeof() {
      Assert.Equal(sizeof(RecordUnmanaged.Storage), Math.Max(sizeof(double), sizeof(float)));
   }
}

[Expected<Unmanaged, nint>]
public sealed partial record SealedRecordUnmanaged;
public class TestSealedRecordUnmanaged : TestStateInvariant<SealedRecordMixed, ValueObject, Error> {
   [Fact]
   public unsafe void Sizeof() {
      Assert.Equal(sizeof(SealedRecordUnmanaged.Storage), Math.Max(sizeof(nint), sizeof(Unmanaged)));
   }
}

[Expected<bool, Unmanaged>]
public readonly partial record struct RecordStructUnmanaged;
public class TestRecordStructUnmanaged : TestStateInvariant<RecordStructUnmanaged, bool, Unmanaged> {
   [Fact]
   public unsafe void Sizeof() {
      Assert.Equal(sizeof(RecordStructUnmanaged.Storage), Math.Max(sizeof(bool), sizeof(Unmanaged)));
   }

}

[Expected<bool, byte>]
public partial struct StructUnmanaged;
public class TestStructUnmanaged : TestStateInvariantMutable<StructUnmanaged, bool, byte> {
   [Fact]
   public unsafe void Sizeof() {
      Assert.Equal(sizeof(StructUnmanaged.Storage), Math.Max(sizeof(byte), sizeof(bool)));
   }
}

//fully generic
[Expected]
public partial class ClassGeneric<V, E> where V : new() where E : new();
public abstract class TestClassGeneric<V, E> : TestStateInvariantMutable<ClassGeneric<V, E>, V, E>
where V : new() where E : new();

// tbh these would likely always succeed if one succeeds
public class ClassGenericWithObjects : TestClassGeneric<ValueObject, ErrorObject>;
public class ClassGenericWithValues : TestClassGeneric<Value, Error>;
public class ClassGenericMixed : TestClassGeneric<Value, ErrorObject>;
public class ClassGenericUnmanaged : TestClassGeneric<int, float>;

[Expected]
public sealed partial class SealedClassGeneric<V, E> where V : new() where E : new();
public abstract class TestSealedClassGeneric<V, E> : TestStateInvariantMutable<SealedClassGeneric<V, E>, V, E>
where V : new() where E : new();

public class SealedClassGenericWithObjects : TestSealedClassGeneric<ValueObject, ErrorObject>;
public class SealedClassGenericWithValues : TestSealedClassGeneric<Value, Error>;
public class SealedClassGenericMixed : TestSealedClassGeneric<Value, ErrorObject>;
public class SealedClassGenericUnmanaged : TestSealedClassGeneric<int, float>;

[Expected]
public partial record RecordGeneric<V, E> where V : new() where E : new();
public abstract class TestRecordGeneric<V, E> : TestStateInvariant<RecordGeneric<V, E>, V, E>
where V : new() where E : new();

public class RecordGenericWithObjects : TestRecordGeneric<ValueObject, ErrorObject>;
public class RecordGenericWithValues : TestRecordGeneric<Value, Error>;
public class RecordGenericMixed : TestRecordGeneric<Value, ErrorObject>;
public class RecordGenericUnmananged : TestRecordGeneric<int, float>;

[Expected]
public sealed partial record SealedRecordGeneric<V, E> where V : new() where E : new();
public abstract class TestSealedRecordGeneric<V, E> : TestStateInvariant<SealedRecordGeneric<V, E>, V, E>
where V : new() where E : new();

public class SealedRecordGenericWithObjects : TestSealedRecordGeneric<ValueObject, ErrorObject>;
public class SealedRecordGenericWithValues : TestSealedRecordGeneric<Value, Error>;
public class SealedRecordGenericMixed : TestSealedRecordGeneric<Value, ErrorObject>;
public class SealedRecordGenericUnmananged : TestSealedRecordGeneric<int, float>;

[Expected]
public readonly partial record struct RecordStructGeneric<V, E> where V : new() where E : new();
public abstract class TestRecordStructGeneric<V, E> : TestStateInvariant<RecordStructGeneric<V, E>, V, E>
where V : new() where E : new();

public class RecordStructGenericWithObjects : TestRecordStructGeneric<ValueObject, ErrorObject>;
public class RecordStructGenericWithValues : TestRecordStructGeneric<Value, Error>;
public class RecordStructGenericMixed : TestRecordStructGeneric<Value, ErrorObject>;
public class RecordStructGenericUnmananged : TestRecordStructGeneric<int, float>;

[Expected]
public partial struct StructGeneric<V, E> where V : new() where E : new();
public abstract class TestStructGeneric<V, E> : TestStateInvariantMutable<StructGeneric<V, E>, V, E>
where V : new() where E : new();

public class StructGenericWithObjects : TestStructGeneric<ValueObject, ErrorObject>;
public class StructGenericWithValues : TestStructGeneric<Value, Error>;
public class StructGenericMixed : TestStructGeneric<Value, ErrorObject>;
public class StructGenericUnmananged : TestStructGeneric<int, float>;


//partial generic
[Expected<Substitute>]
public partial class ClassGenericE<T> where T : new();
public abstract class TestClassGenericE<T> : TestStateInvariantMutable<ClassGenericE<T>, Substitute, T> where T : new();

public class ClassGenericWithObjectE : TestClassGenericE<ErrorObject>;
public class ClassGenericWithValueE : TestClassGenericE<Error>;
public class ClassGenericUnmanagedE : TestClassGenericE<float>;

[Expected<Substitute>]
public sealed partial class SealedClassGenericE<T> where T : new();
public abstract class TestSealedClassGenericE<T> : TestStateInvariantMutable<SealedClassGenericE<T>, Substitute, T> where T : new();
public class SealedClassGenericWithObjectE : TestSealedClassGenericE<ErrorObject>;
public class SealedClassGenericWithValueE : TestSealedClassGenericE<Error>;
public class SealedClassGenericUnmanagedE : TestSealedClassGenericE<float>;

[Expected<Substitute>]
public partial record RecordGenericE<T> where T : new();
public abstract class TestRecordGenericE<T> : TestStateInvariant<RecordGenericE<T>, Substitute, T> where T : new();

public class RecordGenericWithObjectE : TestRecordGenericE<ErrorObject>;
public class RecordGenericWithValueE : TestRecordGenericE<Error>;
public class RecordGenericUnmanangedE : TestRecordGenericE<float>;

[Expected<Substitute>]
public sealed partial record SealedRecordGenericE<T> where T : new();
public abstract class TestSealedRecordGenericE<T> : TestStateInvariant<SealedRecordGenericE<T>, Substitute, T> where T : new();

public class SealedRecordGenericWithObjectE : TestSealedRecordGenericE<ErrorObject>;
public class SealedRecordGenericWithValueE : TestSealedRecordGenericE<Error>;
public class SealedRecordGenericUnmanangedE : TestSealedRecordGenericE<float>;

[Expected<Substitute>]
public readonly partial record struct RecordStructGenericE<T> where T : new();
public abstract class TestRecordStructGenericE<T> : TestStateInvariant<RecordStructGenericE<T>, Substitute, T> where T : new();

public class RecordStructGenericWithObjectE : TestRecordStructGenericE<ErrorObject>;
public class RecordStructGenericWithValueE : TestRecordStructGenericE<Error>;
public class RecordStructGenericUnmanangedE : TestRecordStructGenericE<float>;

[Expected<Substitute>]
public partial struct StructGenericE<T> where T : new();
public abstract class TestStructGenericE<T> : TestStateInvariantMutable<StructGenericE<T>, Substitute, T> where T : new();

public class StructGenericWithObjectE : TestStructGenericE<ErrorObject>;
public class StructGenericWithValueE : TestStructGenericE<Error>;
public class StructGenericUnmanangedE : TestStructGenericE<float>;

[Unexpected<Substitute>]
public partial class ClassGenericV<T> where T : new();
public abstract class TestClassGenericV<T> : TestStateInvariantMutable<ClassGenericV<T>, T, Substitute> where T : new();

public class ClassGenericWithObjectV : TestClassGenericV<ErrorObject>;
public class ClassGenericWithValueV : TestClassGenericV<Error>;
public class ClassGenericUnmanagedV : TestClassGenericV<float>;

[Unexpected<Substitute>]
public sealed partial class SealedClassGenericV<T> where T : new();
public abstract class TestSealedClassGenericV<T> : TestStateInvariantMutable<SealedClassGenericV<T>, T, Substitute> where T : new();
public class SealedClassGenericWithObjectV : TestSealedClassGenericV<ErrorObject>;
public class SealedClassGenericWithValueV : TestSealedClassGenericV<Error>;
public class SealedClassGenericUnmanagedV : TestSealedClassGenericV<float>;

[Unexpected<Substitute>]
public partial record RecordGenericV<T> where T : new();
public abstract class TestRecordGenericV<T> : TestStateInvariant<RecordGenericV<T>, T, Substitute> where T : new();

public class RecordGenericWithObjectV : TestRecordGenericV<ErrorObject>;
public class RecordGenericWithValueV : TestRecordGenericV<Error>;
public class RecordGenericUnmanangedV : TestRecordGenericV<float>;

[Unexpected<Substitute>]
public sealed partial record SealedRecordGenericV<T> where T : new();
public abstract class TestSealedRecordGenericV<T> : TestStateInvariant<SealedRecordGenericV<T>, T, Substitute> where T : new();

public class SealedRecordGenericWithObjectV : TestSealedRecordGenericV<ErrorObject>;
public class SealedRecordGenericWithValueV : TestSealedRecordGenericV<Error>;
public class SealedRecordGenericUnmanangedV : TestSealedRecordGenericV<float>;

[Unexpected<Substitute>]
public readonly partial record struct RecordStructGenericV<T> where T : new();
public abstract class TestRecordStructGenericV<T> : TestStateInvariant<RecordStructGenericV<T>, T, Substitute> where T : new();

public class RecordStructGenericWithObjectV : TestRecordStructGenericE<ErrorObject>;
public class RecordStructGenericWithValueV : TestRecordStructGenericE<Error>;
public class RecordStructGenericUnmanangedV : TestRecordStructGenericE<float>;

[Unexpected<Substitute>]
public partial struct StructGenericV<T> where T : new();
public abstract class TestStructGenericV<T> : TestStateInvariantMutable<StructGenericV<T>, T, Substitute> where T : new();

public class StructGenericWithObjectV : TestStructGenericE<ErrorObject>;
public class StructGenericWithValueV : TestStructGenericE<Error>;
public class StructGenericUnmanangedV : TestStructGenericE<float>;
