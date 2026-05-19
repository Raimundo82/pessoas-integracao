using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Benchmarks;

public static class PessoaDataFactory
{
    public static List<Pessoa> CreatePessoas(int count)
    {
        var pessoas = new List<Pessoa>(count);
        for (int i = 1; i <= count; i++)
        {
            pessoas.Add(new Pessoa
            {
                NII = $"NII-{i:D8}",
                ExternalId = $"EXT-NII-{i:D8}",
                DadosPessoais = new DadosPessoais
                {
                    NomeCompleto = $"Nome Completo {i}",
                    Sobrenome = $"Sobrenome {i}",
                    Apelidos = $"Apelido {i}",
                    DataNascimento = new DateOnly(1970 + (i % 50), (i % 12) + 1, (i % 28) + 1),
                },
                DadosBiometricos = new DadosBiometricos
                {
                    CorDosOlhos = "Castanho",
                    AlturaEmCm = 150 + (i % 50),
                },
            });
        }
        return pessoas;
    }
}
