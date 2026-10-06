using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user is null)
        {
            return NotFound();
        }
        
        var userDto = _mapper.Map<UserDto>(user);

        return Ok(userDto);
    }

    [HttpPost(Name = nameof(CreateUser))]
    public IActionResult CreateUser([FromBody] UserCreateRequest? user)
    {
        if (user is null)
            return BadRequest();
        if (string.IsNullOrEmpty(user.Login))
            return UnprocessableEntity();
        var userEntity = _mapper.Map<UserEntity>(user);
        var entity = _userRepository.Insert(userEntity);

        return CreatedAtRoute(
            nameof(CreateUser),
            new { userId = entity.Id },
            entity);
    }
}