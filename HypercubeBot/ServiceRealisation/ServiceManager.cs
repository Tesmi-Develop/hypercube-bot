using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using Hypercube.Shared.Utilities.Extensions;

namespace HypercubeBot.ServiceRealisation;

public static class ServiceManager
{
    private static bool _isCreated;
    private static readonly List<object> Services = [];

    public static void InitAll()
    {
        foreach (var service in Services)
        {
            var type = service.GetType();
            if (!type.IsAssignableTo(typeof(IInitializable))) 
                continue;
            
            ((IInitializable) service).Init();
        }
    }

    public static void StartAll()
    {
        foreach (var service in Services)
        {
            var type = service.GetType();
            if (!type.IsAssignableTo(typeof(IStartable))) 
                continue;

            ((IStartable) service).Start();
        }
    }

    private static void InjectLogger(object @object)
    {
        var type = @object.GetType();

        foreach (var field in  type.GetAllFields())
        {
            var fieldType = field.FieldType;
            if (fieldType.IsAssignableTo(typeof(ILogger)))
            {
                field.SetValue(@object, LoggingManager.GetLogger(type.Name));
            }
        }
    }
    
    public static void CreateAll()
    {
        if (_isCreated) 
            return;
        
        _isCreated = true;
        
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass) 
                    continue;
                
                if (!Attribute.IsDefined(type, typeof(Service))) 
                    continue;

                DependencyManager.Register(type, type);
                var service = DependencyManager.Instantiate(type);
                Services.Add(service);

                InjectLogger(service);
                
                Console.WriteLine($"Instantiated {type.Name}");
            }
        }
    }
}