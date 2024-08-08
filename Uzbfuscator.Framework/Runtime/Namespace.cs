using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace Uzbfuscator.Framework
{
    public sealed partial class Obfuscator : ObfuscatorBase
    {

        protected override void DoObfuscateNamespace(TypeDefinition type)
        {
            if (!IsNamespaceObfuscatable(type))
                return;

            if (m_excludedTypes.Contains(type.FullName))
                return;

            string initialFullName = type.FullName;
            string initialNamespace = type.Namespace;

            type.Namespace = GetObfuscatedNamespace(type.Namespace);

            foreach (AssemblyDefinition assembly in m_assemblyDefinitions)
                foreach (ModuleDefinition module in assembly.Modules)
                    foreach (TypeReference typeReference in module.GetTypeReferences())
                        if (typeReference.Namespace == initialNamespace)
                            typeReference.Namespace = type.Namespace;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in m_mapResources.Keys)
            {
                string resValue = m_mapResources[key];
                if (resValue.Contains("."))
                    if (resValue.Substring(0, resValue.LastIndexOf('.')) == initialNamespace)
                        resValue = type.Namespace + resValue.Substring(resValue.LastIndexOf('.'));

                dic.Add(key, resValue);
            }

            m_mapResources = dic;
        }

        public static bool IsNamespaceObfuscatable(TypeDefinition type)
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

            if (type.Namespace.Length < 1)
                flag = false;

            return flag;
        }

        private string GetObfuscatedNamespace(string initialNamespace)
        {
            if (m_mapObfuscatedNamespaces.ContainsKey(initialNamespace))
                return m_mapObfuscatedNamespaces[initialNamespace];

            string[] namespaceSet = initialNamespace.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            string currentNs = "";
            string currentNsObfuscated = "";
            foreach (string ns in namespaceSet)
            {
                if (currentNs.Length > 0)
                {
                    currentNs += ".";
                    currentNsObfuscated += ".";
                }

                currentNs += ns;

                if (!m_mapObfuscatedNamespaces.ContainsKey(currentNs))
                {
                    m_mapObfuscatedNamespaces.Add(currentNs, currentNsObfuscated + DoObfuscateItem(ObfuscationItem.Namespace, ns));
                }

                currentNsObfuscated = m_mapObfuscatedNamespaces[currentNs];
            }

            return m_mapObfuscatedNamespaces[initialNamespace];
        }
    
    }
}
