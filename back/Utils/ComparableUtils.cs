
namespace back.Utils
{
  public class ComparableUtils : IComparableUtils
  {
    public List<T> SortByComparable<T>(List<T> items) where T : class, IComparable<T>
    {
      items.Sort();
      return items;
    }

    public T? FindMinimum<T>(List<T> items) where T : class, IComparable<T>
    {
      if (items.Count == 0) return null;

      T min = items[0];
      for (int i = 1; i < items.Count; i++)
      {
        if (items[i].CompareTo(min) < 0)
        {
          min = items[i];
        }
      }
      return min;
    }
  }
}
