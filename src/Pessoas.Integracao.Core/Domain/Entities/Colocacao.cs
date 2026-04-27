using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Core.Domain.Entities;

public class Colocacao
{
    public int Id { get; set; }
    public required int PessoaId { get; set; }
    public required virtual Pessoa Pessoa { get; set; }
    public required UnidadeExternaRef ExternalReference { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime? Fim { get; set; }
}
