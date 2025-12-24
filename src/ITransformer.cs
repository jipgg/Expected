namespace Expected;

public interface ITransformer<in From, out To>
where From : allows ref struct
where To : allows ref struct {
   To Transform(From from);
}
