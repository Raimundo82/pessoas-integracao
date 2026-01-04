using Microsoft.AspNetCore.Mvc;

using Pessoas.Integracao.Core.Application.UseCases;

namespace Pessoas.Integracao.Admin.Controllers;

[ApiController]
[Route("api/pessoas/import")]
public class PessoasImportController(ImportAllPessoas importAllPessoas) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Import(CancellationToken cancellationToken)
    {
        await importAllPessoas.ExecuteAsync(cancellationToken);
        return Accepted();
    }
}