using System.Diagnostics;
namespace Expected;

public sealed class Expected<V, E> {
	[MemberNotNullWhen(true, nameof(_value))]
	[MemberNotNullWhen(false, nameof(_error))]
   public bool HasValue { get; internal set; }
	internal V? _value;
	internal E? _error;
   public V Value {
      get => HasValue ? _value : throw new BadExpectedAccess();
      set {
         HasValue = true;
			_value = value;
      }
   }
   public E Error {
      get => HasValue ? throw new BadExpectedAccess() : _error;
      set {
         HasValue = false;
         _error = value;
      }
   }
   public V ValueOr(V v) => HasValue ? _value : v;
   public E ErrorOr(E e) => HasValue ? e : _error;

   [MethodImpl(AggressiveInlining)]
   public Expected(V value) {
      HasValue = true;
      _value = value!;
   }
   [MethodImpl(AggressiveInlining)]
   public Expected(in Unexpected<E> u) {
      HasValue = false;
      _error = u.Error!;
   }
   public Expected<R, E> Select<R>(Func<V, R> selector) where R : notnull
       => HasValue ? new(selector(_value)) : new(Unexpected(_error));
   public Expected<V, R> SelectError<R>(Func<E, R> selector) where R : notnull
        => HasValue ? new(_value) : new(Unexpected(selector(_error)));
   public Expected<V, E> AndThen(Func<V, Expected<V, E>> selector)
       => HasValue ? selector(_value) : this;
   public Expected<R, E> AndThen<R>(Func<V, Expected<R, E>> selector) where R : notnull
       => HasValue ? selector(_value) : new(Unexpected(_error));
   public Expected<V, E> OrElse(Func<E, Expected<V, E>> selector)
       => HasValue ? this : selector(_error);
   public Expected<V, R> OrElse<R>(Func<E, Expected<V, R>> selector) where R : notnull
       => HasValue ? new(_value) : selector(_error);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<V, E>(V v) => new(v);

   [MethodImpl(AggressiveInlining)]
   public static implicit operator Expected<V, E>(in Unexpected<E> u) => new(u);

   [MethodImpl(AggressiveInlining)]
   public static bool operator true(Expected<V, E> r) => r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator false(Expected<V, E> r) => !r.HasValue;
   [MethodImpl(AggressiveInlining)]
   public static bool operator !(Expected<V, E> r) => !r.HasValue;

	public static V operator +(Expected<V, E> e) => e.Value;
	public static E operator -(Expected<V, E> e) => e.Error;
}

