namespace Uzbfuscator.Framework
{
    public struct ObfuscationInfo
    {

        public string OutputDirectory { get; set; }

        public bool ObfuscateTypes { get; set; }

        public bool ObfuscateMethods { get; set; }

        public bool ObfuscateNamespaces { get; set; }

        public bool ObfuscateProperties { get; set; }

        public bool ObfuscateFields { get; set; }

        public bool ObfuscateResources { get; set; }

        public ObfuscationInfo(string outputDirectory, bool obfuscateTypes, bool obfuscateMethods, bool obfuscateNamespaces, bool obfuscateProperties, bool obfuscateFields, bool obfuscateResources) : this()
        {
            OutputDirectory = outputDirectory;
            ObfuscateTypes = obfuscateTypes;
            ObfuscateMethods = obfuscateMethods;
            ObfuscateNamespaces = obfuscateNamespaces;
            ObfuscateProperties = obfuscateProperties;
            ObfuscateFields = obfuscateFields;
            ObfuscateResources = obfuscateResources;
        }

    }
}
