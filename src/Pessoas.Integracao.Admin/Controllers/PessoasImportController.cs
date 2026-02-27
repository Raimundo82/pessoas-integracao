using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Constants;

namespace Pessoas.Integracao.Admin.Controllers;

[ApiController]
[Route("api/pessoas/import")]
public class PessoasImportController(ImportPessoas importAllPessoas) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.CanImportPessoas)]
    public async Task<IActionResult> Import(CancellationToken cancellationToken)
    {
        await importAllPessoas.ExecuteAsync(cancellationToken);
        return Accepted();
    }
}