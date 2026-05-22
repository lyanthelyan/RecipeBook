using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromBody]RequestRegisterUserAccountJson request)
    {
        //Register user account

        return Created();
    }


}
