using Mono.Cecil;

namespace Uzbfuscator.Framework
{
    public abstract class ObfuscatorBase
    {

        protected abstract void AsyncStartObfuscation();

        protected abstract void UpdateProgress(string message, int percent);

        protected abstract void LogProgress(string message);

        protected abstract void DoObfuscateField(TypeDefinition type, FieldDefinition field);

        protected abstract void DoObfuscateMethod(TypeDefinition type, string initialTypeName, MethodDefinition method);

        protected abstract void DoObfuscateNamespace(TypeDefinition type);

        protected abstract void DoObfuscateProperty(TypeDefinition type, PropertyDefinition property);

        protected abstract void DoObfuscateResource(Resource resource);

        protected abstract void DoObfuscateType(TypeDefinition type);

    }
}
