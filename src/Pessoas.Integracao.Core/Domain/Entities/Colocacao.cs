using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Core.Domain.Entities;

public class Colocacao
{
    public int Id { get; set; }
    public int PessoaId { get; set; }
    public virtual Pessoa Pessoa { get; set; } = null!;
    public required UnidadeExternaRef ExternalReference { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime? Fim { get; set; }
}
