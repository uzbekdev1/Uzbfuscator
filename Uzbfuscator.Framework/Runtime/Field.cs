using Mono.Cecil;
using System;

namespace Uzbfuscator.Framework
{
    public sealed partial class Obfuscator : ObfuscatorBase
    {

        protected override void DoObfuscateField(TypeDefinition type, FieldDefinition field)
        {
            if (!IsFieldObfuscatable(field))
                return;

            string initialName = field.Name;
            string obfuscatedName = DoObfuscateItem(ObfuscationItem.Field, field.Name);

            if (field.DeclaringType.Name == type.Name && field.DeclaringType.Namespace == type.Namespace)
                if (!Object.ReferenceEquals(field, field) && (field.Name == initialName))
                {
                    try
                    {
                        field.Name = obfuscatedName;
                    }
                    catch (InvalidOperationException) { }
                }

            field.Name = obfuscatedName;
        }

        public static bool IsFieldObfuscatable(FieldDefinition field)
        {
            bool flag = true;

            if (field.IsRuntimeSpecialName)
                flag = false;

            if (field.IsSpecialName)
                flag = false;

            if (field.Name.StartsWith("<"))
                flag = false;

            return flag;
        }

    }
}
