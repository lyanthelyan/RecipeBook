using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Login.WithEmailAndPassword;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;

namespace MyRecipeBook.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ResponseRegisteredUserJson), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] RequestLoginJson request,
        [FromServices] ILoginWithEmailAndPasswordUseCase useCase)
    {
        var response = await useCase.Execute(request);

        return Ok(response);
    }
}
