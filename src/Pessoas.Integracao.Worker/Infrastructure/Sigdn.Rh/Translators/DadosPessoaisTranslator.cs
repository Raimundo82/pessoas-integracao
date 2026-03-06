using System.Globalization;

using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

public class DadosPessoaisTranslator : IDadosPessoaisTranslator
{
    public DadosPessoais? Translate(ZhrSPessoaisOutput output)
    {
        if (output.Pessoais is not { Length: > 0 })
            return null;

        var pessoais = output.Pessoais[0];

        return new DadosPessoais
        {
            NomeCompleto = pessoais.Nome,
            Sobrenome = pessoais.Apelido,
            Apelidos = pessoais.Rufnm,
            DataNascimento = string.IsNullOrEmpty(pessoais.DtNasci)
                ? null
                : DateOnly.Parse(pessoais.DtNasci, new CultureInfo("pt-PT"))
        };
    }
}