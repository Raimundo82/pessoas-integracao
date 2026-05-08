using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Fragments;

public sealed record PessoaCoreDataFragment(
    DadosPessoais DadosPessoais,
    DadosBiometricos DadosBiometricos,
    List<Colocacao> Colocacoes
);
