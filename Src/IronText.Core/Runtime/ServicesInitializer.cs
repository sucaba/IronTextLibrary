using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;
using IronText.Misc;

namespace IronText.Framework
{
    static class ServicesInitializer
    {
        public static void SetServiceProperties(
            Type    contextType,
            object  context,
            Type    serviceType,
            object  service)
        {
            foreach (var prop in EnumeratePublicInstanceProperties(contextType))
            {
                if (Attributes.Exists<LanguageServiceAttribute>(prop))
                {
                    if (prop.PropertyType == serviceType)
                    {
                        prop.SetValue(context, service, null);
                    }
                }
                else if (Attributes.Exists<SubContextAttribute>(prop))
                {
                    var value = prop.GetValue(context, null);
                    SetServiceProperties(
                        prop.PropertyType,
                        value,
                        serviceType,
                        service);
                }
            }
        }

        private static IEnumerable<PropertyInfo> EnumeratePublicInstanceProperties(Type type)
        {
            var result = new List<PropertyInfo>();
            result.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            if (type.IsInterface)
            {
                result.AddRange(type.GetInterfaces().SelectMany(EnumeratePublicInstanceProperties));
            }

            return result;
        }
    }
}
