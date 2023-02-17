using System.ComponentModel.DataAnnotations;
using Shouldly;

namespace Initialize.Tests;

public static class TestExtensions
{
    // Asserts that all required properties (via the 'Required' attribute) 
// be non null using Shouldly
// Optionally include all properties if desired
    private static void AssertPropertiesAreNonNull<T>(this T obj, bool onlyRequiredProperties = true)
    {
        if (obj == null)
        {
            return;
        }

        var objType = obj.GetType();

        // Get either only required properties or ALL properties
        var properties = onlyRequiredProperties ? objType.GetProperties()
                .Where(x => x.GetCustomAttributes(false).OfType<RequiredAttribute>().Any()) : 
            objType.GetProperties();

        foreach (var property in properties)
        {
            var propValue = property.GetValue(obj, null);
            var elems = propValue as IList<object>;

            // Another layer 
            if (elems != null)
            {
                foreach (var item in elems)
                {
                    AssertPropertiesAreNonNull(item, onlyRequiredProperties);
                }
            }
            else
            {
                if (property.PropertyType.Assembly == objType.Assembly)
                {
                    AssertPropertiesAreNonNull(propValue, onlyRequiredProperties);
                }
                // Reached the end of the tree
                else
                {
                    // Use Shouldly to assert that the propValue should not be null
                    propValue.ShouldNotBeNull();
                }
            }
        }
    }
}