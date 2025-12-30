namespace Pessoas.Integracao.Core.Domain.ValueObjects;

public class DadosBiometricos
{
    public string? CorDosOlhos { get; set; }
    public decimal? AlturaEmCm { get; set; }
    public TipoDeSangue TipoDeSangue { get; set; } = new TipoDeSangue();
}