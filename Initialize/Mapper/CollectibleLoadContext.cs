using System.Reflection;
using System.Runtime.Loader;

namespace Initialize.Mapper;
public class CollectibleLoadContext : AssemblyLoadContext
{
    public CollectibleLoadContext() : base(isCollectible: true)
    {

    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }
}
