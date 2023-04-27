namespace WildfrostModMiya;

public static class IL2CPPHelpers
{
    public static T Find<T>(this Il2CppSystem.Collections.Generic.List<T> list, System.Predicate<T> p)
    {
        foreach (var element in list)
        {
            if (p(element))
            {
                return element;
            }
        }

        return default;
    }
}