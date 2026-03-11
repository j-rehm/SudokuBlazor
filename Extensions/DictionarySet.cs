using System.Collections;

namespace SudokuBlazor.Extensions;

public static class DictionarySet
{
  public static TSet GetSet<TKey, TSet>(this Dictionary<TKey, TSet> dict, TKey key)
    where TKey : notnull
    where TSet : IEnumerable, new()
  {
    if (!dict.TryGetValue(key, out TSet? set))
      dict[key] = set = [];
    return set;
  }

  public static void AddItems<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dict, TKey key, params IEnumerable<TValue> values)
    where TKey : notnull
  {
    var set = dict.GetSet(key);
    foreach (TValue value in values)
      set.Add(value);
  }
  public static void AddItem<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dict, TKey key, TValue value)
    where TKey : notnull
    => dict.AddItems(key, value);

  public static void AddItems<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, params IEnumerable<TValue> values)
    where TKey : notnull
  {
    var list = dict.GetSet(key);
    list.AddRange(values);
  }
  public static void AddItem<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue value)
    where TKey : notnull
    => dict.AddItems(key, value);
}

public class ReadOnlySet<T>(Func<IEnumerable<T>> getItems) : IEnumerable<T>
{
  private readonly Func<IEnumerable<T>> _getItems = getItems;

  IEnumerator<T> IEnumerable<T>.GetEnumerator()
  {
    foreach (T item in _getItems())
      yield return item;
  }
  IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
}