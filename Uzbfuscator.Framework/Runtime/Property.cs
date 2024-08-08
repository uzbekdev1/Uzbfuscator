using Mono.Cecil;
using System;

namespace Uzbfuscator.Framework
{
    public sealed partial class Obfuscator : ObfuscatorBase
    {

        protected override void DoObfuscateProperty(TypeDefinition type, PropertyDefinition property)
        {
            if (!IsPropertyObfuscatable(property))
                return;

            string initialName = property.Name;
            string obfuscatedName = DoObfuscateItem(ObfuscationItem.Property, property.Name);

            if (property.DeclaringType.Name == type.Name && property.DeclaringType.Namespace == type.Namespace)
            {
                if (!Object.ReferenceEquals(property, property) && (property.Name == property.Name) && (property.Parameters.Count == property.Parameters.Count))
                {
                    bool paramsEquals = true;
                    for (int paramIndex = 0; paramIndex < property.Parameters.Count; paramIndex++)
                        if (property.Parameters[paramIndex].ParameterType.FullName != property.Parameters[paramIndex].ParameterType.FullName)
                        {
                            paramsEquals = false;
                            break;
                        }
                    try
                    {
                        if (paramsEquals)
                            property.Name = obfuscatedName;
                    }
                    catch (InvalidOperationException) { }
                }
            }

            property.Name = obfuscatedName;
        }

        public static bool IsPropertyObfuscatable(PropertyDefinition property)
        {
            bool flag = true;

            if (property.IsSpecialName)
                flag = false;

            if (property.IsRuntimeSpecialName)
                flag = false;

            return flag;
        }

    }
}
