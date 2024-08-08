namespace Uzbfuscator.Framework
{
    public struct ObfuscationProgress
    {

        public ulong CurrentObfuscatedMethodID { get; set; }

        public ulong CurrentObfuscatedTypeID { get; set; }

        public ulong CurrentObfuscatedNamespaceID { get; set; }

        public ulong CurrentObfuscatedPropertyID { get; set; }

        public ulong CurrentObfuscatedFieldID { get; set; }

    }
}
