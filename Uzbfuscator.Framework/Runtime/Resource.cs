using Mono.Cecil;

namespace Uzbfuscator.Framework
{
    public sealed partial class Obfuscator : ObfuscatorBase
    {

        protected override void DoObfuscateResource(Resource resource)
        {
            if (!IsResourceObfuscatable(resource.Name))
                return;

            string resourceName = resource.Name.Substring(0, resource.Name.Length - 10);

            if (!m_mapResources.ContainsKey(resourceName))
                return;

            string obfucatedName = m_mapResources[resourceName];
            resource.Name = obfucatedName + ".resources";
        }

        public static bool IsResourceObfuscatable(string name)
        {
            bool flag = true;

            if (name == "<Module>")
                flag = false;

            return flag;
        }

    }
}
