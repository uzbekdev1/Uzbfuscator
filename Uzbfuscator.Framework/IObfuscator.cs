namespace Uzbfuscator.Framework
{
    public interface IObfuscator
    {

        void AddAssembly(string path, bool obfuscate);

        void StartObfuscation();

    }
}
