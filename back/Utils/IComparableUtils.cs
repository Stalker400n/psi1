
namespace back.Utils
{
  public interface IComparableUtils
  {
    List<T> SortByComparable<T>(List<T> items) where T : class, IComparable<T>;
    T? FindMinimum<T>(List<T> items) where T : class, IComparable<T>;
  }
}
