using Mono.Cecil;

namespace Uzbfuscator.Framework
{
    public sealed partial class Obfuscator : ObfuscatorBase
    {

        protected override void DoObfuscateType(TypeDefinition type)
        {
            if (!IsTypeObfuscatable(type))
                return;

            if (m_excludedTypes.Contains(type.FullName))
                return;

            string initialTypeName = type.FullName;
            type.Name = DoObfuscateItem(ObfuscationItem.Type, type.Name);

            if (!initialTypeName.Contains("/"))
            {
                m_mapResources.Add(initialTypeName, type.FullName);
            }

            foreach (AssemblyDefinition assembly in m_assemblyDefinitions)
                foreach (ModuleDefinition module in assembly.Modules)
                    foreach (TypeReference typeReference in module.GetTypeReferences())
                        if (typeReference.FullName == initialTypeName)
                            typeReference.Name = type.Name;

            foreach (MethodDefinition method in type.Methods)
                DoObfuscateMethod(type, initialTypeName, method);

            foreach (PropertyDefinition property in type.Properties)
                DoObfuscateProperty(type, property);

            foreach (FieldDefinition field in type.Fields)
                DoObfuscateField(type, field);
        }

        public static bool IsTypeObfuscatable(TypeDefinition type)
        {
            bool flag = true;

            if (string.IsNullOrEmpty(type.Name))
                flag = false;

            if (type.Name == "<Module>")
                flag = false;

            if (type.IsRuntimeSpecialName)
                flag = false;

            if (type.IsSpecialName)
                flag = false;

            if (type.Name.Contains("Resources"))
                flag = false;

            if (type.Name.StartsWith("<"))
                flag = false;

            if (type.Name.Contains("__"))
                flag = false;

            return flag;
        }
         
    }
}
