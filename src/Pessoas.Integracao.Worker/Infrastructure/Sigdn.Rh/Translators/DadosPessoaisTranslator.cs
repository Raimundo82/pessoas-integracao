using System.Globalization;

using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

public class DadosPessoaisTranslator : IDadosPessoaisTranslator
{
    public DadosPessoais Translate(ZhrSPessoaisOutput? output)
    {
        if (output?.Pessoais is not { Length: > 0 })
            return new DadosPessoais();

        var pessoais = output.Pessoais[^1];

        return new DadosPessoais
        {
            NomeCompleto = pessoais.Nome,
            Sobrenome = pessoais.Apelido,
            Apelidos = pessoais.Rufnm,
            DataNascimento =
                DateOnly.TryParse(pessoais.DtNasci, new CultureInfo("pt-PT"), out var dataNasc)
                ? dataNasc
                : null
        };
    }
}
