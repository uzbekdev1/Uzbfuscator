using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Mono.Cecil;

namespace Uzbfuscator.Framework
{

    public delegate void DelOutputEvent(string message);

    public delegate void DelNameObfuscated(ObfuscationItem item, string initialName, string obfuscatedName);

    public delegate void DelProgress(string phaseName, int percents);

    public sealed partial class Obfuscator : ObfuscatorBase, IObfuscator
    {

        private Dictionary<string, string> m_mapResources = new Dictionary<string, string>();

        private List<AssemblyDefinition> m_assemblyDefinitions = new List<AssemblyDefinition>();

        private Dictionary<string, string> m_mapObfuscatedNamespaces = new Dictionary<string, string>();

        private Dictionary<string, bool> m_assemblies = new Dictionary<string, bool>();

        private List<string> m_excludedTypes = new List<string>();

        private ObfuscationInfo m_obfuscationInfo;

        private ObfuscationProgress m_obfuscationProgress;

        private XmlDocument m_xmlDocument;

        private XmlElement m_xmlElement;

        public event DelOutputEvent OnOutputEvent;

        public event DelNameObfuscated OnNameObfuscated;

        public event DelProgress OnProgress;

        public Obfuscator(ObfuscationInfo obfuscationInfo)
        {
            m_obfuscationInfo = obfuscationInfo;
            m_obfuscationProgress = new ObfuscationProgress();
        }

        public void AddAssembly(string path, bool obfuscate)
        {
            m_assemblies.Add(path, obfuscate);
        }

        public void ExcludeType(string typeName)
        {
            m_excludedTypes.Add(typeName);
        }

        protected override void UpdateProgress(string message, int percent)
        {
            OnProgress?.Invoke(message, percent);
        }

        protected override void LogProgress(string message)
        {
            OnOutputEvent?.Invoke(message);
        }

        public void StartObfuscation()
        {
            Thread thread = new Thread(new ThreadStart(AsyncStartObfuscation));
            thread.Start();
        }

        protected override void AsyncStartObfuscation()
        {
            List<string> assembliesPaths = new List<string>();
            List<bool> assembliesToObfuscate = new List<bool>();

            LogProgress("[0]: Starting...");

            m_xmlDocument = new XmlDocument();
            m_xmlElement = m_xmlDocument.CreateElement("mappings");
            m_xmlDocument.AppendChild(m_xmlElement);

            UpdateProgress("[1]: Loading assemblies...", 10);
            foreach (string assemblyPath in m_assemblies.Keys)
            {
                try
                {
                    AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
                    foreach (ModuleDefinition module in assembly.Modules)
                        LogProgress($"[OK]: Module loaded: {module.Name}");

                    m_assemblyDefinitions.Add(assembly);
                    assembliesPaths.Add(Path.GetFileName(assemblyPath));
                    assembliesToObfuscate.Add(m_assemblies[assemblyPath]);
                }
                catch (Exception ex)
                {
                    LogProgress($"[ERR]: Module load failed: {ex.Message}");
                    continue;
                }
            }

            UpdateProgress("[2]: Starting obfuscate...", 20);

            int progressCurrent = 20;
            int assemblyIndex = -1;

            if (m_assemblyDefinitions.Any())
            {
                int progressIncrement = 60 / m_assemblyDefinitions.Count;

                foreach (AssemblyDefinition assembly in m_assemblyDefinitions)
                {
                    assemblyIndex++;

                    if (!assembliesToObfuscate[assemblyIndex])
                        continue;

                    LogProgress("Obfuscating assembly: " + assembly.Name.Name);

                    LogProgress("Obfuscating Types");
                    foreach (TypeDefinition type in assembly.MainModule.Types)
                        DoObfuscateType(type);

                    if (m_obfuscationInfo.ObfuscateNamespaces)
                        LogProgress("Obfuscating Namespaces");
                    foreach (TypeDefinition type in assembly.MainModule.Types)
                        DoObfuscateNamespace(type);

                    if (m_obfuscationInfo.ObfuscateResources)
                        LogProgress("Obfuscating Resources");
                    foreach (Resource resource in assembly.MainModule.Resources)
                        DoObfuscateResource(resource);

                    progressCurrent += progressIncrement;
                }
            }

            UpdateProgress("[3]: Saving assembly...", 80);

            assemblyIndex = -1;
            foreach (AssemblyDefinition assembly in m_assemblyDefinitions)
            {
                assemblyIndex++;

                if (Directory.Exists(m_obfuscationInfo.OutputDirectory) == false)
                    Directory.CreateDirectory(m_obfuscationInfo.OutputDirectory);

                string outputFileName = Path.Combine(m_obfuscationInfo.OutputDirectory, "Obfuscated_" + assembliesPaths[assemblyIndex]);

                if (File.Exists(outputFileName))
                    File.Delete(outputFileName);

                assembly.Write(outputFileName);
            }

            m_xmlDocument.Save(Path.Combine(m_obfuscationInfo.OutputDirectory, "Mapping.xml"));

            UpdateProgress("[4]: Testing assembly...", 90);

            foreach (string assemblyPath in m_assemblies.Keys)
            {
                if (!File.Exists(assemblyPath))
                {
                    LogProgress($"[FAIL]: File not exists: {assemblyPath}");
                    continue;
                }

                try
                {
                    AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
                    foreach (ModuleDefinition module in assembly.Modules)
                        LogProgress($"[OK]: {module.Name}");

                }
                catch (Exception ex)
                {
                    LogProgress($"[FAIL]: {assemblyPath} - Exception: {ex.Message}");
                }

            }

            UpdateProgress("[5]: Complete.", 100);
        }

        internal string DoObfuscateItem(ObfuscationItem item, string initialName)
        {
            string obfuscated = string.Empty;

            switch (item)
            {
                case ObfuscationItem.Method:
                    if (!m_obfuscationInfo.ObfuscateMethods)
                        return initialName;
                    m_obfuscationProgress.CurrentObfuscatedMethodID++;
                    obfuscated = GetObfuscatedFormat(item, initialName, m_obfuscationProgress.CurrentObfuscatedMethodID);
                    break;

                case ObfuscationItem.Type:
                    if (!m_obfuscationInfo.ObfuscateTypes)
                        return initialName;
                    m_obfuscationProgress.CurrentObfuscatedTypeID++;
                    obfuscated = GetObfuscatedFormat(item, initialName, m_obfuscationProgress.CurrentObfuscatedTypeID);
                    break;

                case ObfuscationItem.Namespace:
                    m_obfuscationProgress.CurrentObfuscatedNamespaceID++;
                    obfuscated = GetObfuscatedFormat(item, initialName, m_obfuscationProgress.CurrentObfuscatedNamespaceID);
                    break;

                case ObfuscationItem.Property:
                    if (!m_obfuscationInfo.ObfuscateProperties)
                        return initialName;
                    m_obfuscationProgress.CurrentObfuscatedPropertyID++;
                    obfuscated = GetObfuscatedFormat(item, initialName, m_obfuscationProgress.CurrentObfuscatedPropertyID);
                    break;

                case ObfuscationItem.Field:
                    if (!m_obfuscationInfo.ObfuscateFields)
                        return initialName;
                    m_obfuscationProgress.CurrentObfuscatedFieldID++;
                    obfuscated = GetObfuscatedFormat(item, initialName, m_obfuscationProgress.CurrentObfuscatedFieldID);
                    break;
            }

            OnNameObfuscated?.Invoke(item, initialName, obfuscated);

            AddToXMLMap(item, initialName, obfuscated);

            return obfuscated;
        }

        public string PrefixObfuscatedFiles { get; set; } = "[Obfuscator]";

        private string GetObfuscatedFormat(ObfuscationItem item, string initialName, ulong index)
        {
            return string.Format($"{PrefixObfuscatedFiles}-{0}-{1}", EncryptAsCaesar(initialName, 1), index);
        }

        private string EncryptAsCaesar(string value, int shift)
        {
            char[] buffer = value.ToCharArray();
            char letter;
            for (int i = 0; i < buffer.Length; i++)
            {
                letter = buffer[i];
                letter = (char)(letter + shift);
                if (letter > 'z')
                {
                    letter = (char)(letter - 26);
                }
                else if (letter < 'a')
                {
                    letter = (char)(letter + 26);
                }
                buffer[i] = letter;
            }
            return new string(buffer);
        }

        private void AddToXMLMap(ObfuscationItem item, string initialName, String obfuscated)
        {
            XmlElement element = m_xmlDocument.CreateElement("mapping");
            m_xmlElement.AppendChild(element);
            element.SetAttribute("Type", item.ToString());
            element.SetAttribute("InitialValue", initialName);
            element.SetAttribute("ObfuscatedValue", obfuscated);
        }

    }
}
