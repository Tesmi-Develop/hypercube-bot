namespace HypercubeBot.Utils;

public static class ReflectionHelper
{
    public static IEnumerable<KeyValuePair<Type, T>> GetAllTypes<T>() where T : Attribute
    {
        return from type in GetAllTypes() 
            from customAttribute in type.GetCustomAttributes(typeof(T), false)
            select new KeyValuePair<Type, T>(type, (T) customAttribute);
    }
    
    public static IEnumerable<Type> GetAllTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
    }
}