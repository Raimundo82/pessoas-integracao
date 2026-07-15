using System.Runtime.CompilerServices;

namespace Pessoas.Integracao.Testing;

static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        DerivePathInfo((sourceFile, projectDirectory, type, method) =>
            new PathInfo(Path.Combine(projectDirectory, "__snapshots__")));
    }
}
