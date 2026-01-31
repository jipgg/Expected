namespace Expected;


public delegate R ScopedInFunc<T, out R>(scoped in T a)
where T : allows ref struct
where R : allows ref struct;

public delegate R InFunc<T, out R>(in T a)
where T : allows ref struct
where R : allows ref struct;
