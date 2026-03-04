using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Application.Security;
using Pessoas.Integracao.Core.Application.UseCases;

namespace Pessoas.Integracao.Admin.Controllers;

[ApiController]
[Route("api/pessoas/import")]
public class PessoasImportController(ImportPessoas importAllPessoas) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.CanImportPessoas)]
    public async Task<IActionResult> Import(CancellationToken cancellationToken)
    {
        var result = await importAllPessoas.ExecuteAsync(cancellationToken);

        var pessoasResult = new ImportPessoasResultDto(
            TotalProcessed: result.TotalProcessed,
            TotalAdded: result.TotalAdded,
            TotalUpdated: result.TotalUpdated
        );

        return Accepted(pessoasResult);
    }
}