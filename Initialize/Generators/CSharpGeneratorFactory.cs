using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Initialize.Generators;
public static class CSharpGeneratorFactory
{
    public static string GenerateDto<T>(string @namespace, string nameSuffix = "Dto")
    {
        var generator = new CSharpDtoGenerator(typeof(T), nameSuffix,@namespace, false);
        return generator.Generate().NormalizeWhitespace().ToFullString();
    }
    public static string GenerateDtoWithNotifyPropertyChanged<T>(string @namespace, string nameSuffix = "Dto")
    {
        var generator = new CSharpDtoGenerator(typeof(T), nameSuffix, @namespace);
        return generator.Generate().NormalizeWhitespace().ToFullString();
    }
}
