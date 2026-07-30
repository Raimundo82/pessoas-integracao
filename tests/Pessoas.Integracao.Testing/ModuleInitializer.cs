using System.Runtime.CompilerServices;

using Pessoas.Integracao.Testing.Converters;

namespace Pessoas.Integracao.Testing;

static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        DerivePathInfo((sourceFile, projectDirectory, type, method) =>
            new PathInfo(Path.Combine(projectDirectory, "__snapshots__")));
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.AddExtraSettings(s => s.Converters.Add(new DateTimeOffsetFormatConverter()));

    }
}
