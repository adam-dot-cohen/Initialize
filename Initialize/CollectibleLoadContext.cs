using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Initialize;
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
