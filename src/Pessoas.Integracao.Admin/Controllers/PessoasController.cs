using Microsoft.AspNetCore.Mvc;

using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Application.UseCases;

namespace Pessoas.Integracao.Admin.Controllers;

[ApiController]
[Route("api/pessoas")]
public class PessoasController(GetAllPessoas getAllPessoas) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PessoaDto>>> GetAll(CancellationToken cancellationToken)
    {
        var pessoas = await getAllPessoas.ExecuteAsync(cancellationToken);
        return Ok(pessoas);
    }
}