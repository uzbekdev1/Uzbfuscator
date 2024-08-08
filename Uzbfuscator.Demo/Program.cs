using Uzbfuscator.Framework;

namespace Uzbfuscator.Demo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var target = Path.Combine(Environment.CurrentDirectory, "ClassLibrary1.dll");
            var outputPath = Path.GetDirectoryName(target);
            var info = new ObfuscationInfo(outputPath, true, true, true, true, true, false);
            var obfuscator = new Obfuscator(info)
            {
                PrefixObfuscatedFiles = Guid.NewGuid().ToString("N")
            };

            obfuscator.AddAssembly(target, true);

            obfuscator.OnOutputEvent += new DelOutputEvent(RaiseOnOutputEvent);
            obfuscator.OnNameObfuscated += new DelNameObfuscated(RaiseOnNameObfuscated);
            obfuscator.OnProgress += new DelProgress(RaiseOnProgress);

            obfuscator.StartObfuscation();
        }

        private static void RaiseOnOutputEvent(string message)
        {
            Console.WriteLine();
            Console.WriteLine(message);
        }

        private static void RaiseOnNameObfuscated(ObfuscationItem item, string initialName, string obfuscatedName)
        {
            Console.WriteLine();
            Console.WriteLine($"ItemObfuscated: [{item}]");
            Console.WriteLine($"Old: [{initialName}]");
            Console.WriteLine($"New: [{obfuscatedName}]");
        }

        private static void RaiseOnProgress(string phaseName, int percents)
        {
            Console.WriteLine();
            Console.WriteLine($"{phaseName} : [%{percents}]");
        }

    }
}
