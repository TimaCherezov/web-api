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
    [HttpHead("{userId}")]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {

        var user = _userRepository.FindById(userId);
        if (user is null)
        {
            return NotFound();
        }

        var userDto = _mapper.Map<UserDto>(user);
        if (HttpMethods.IsHead(Request.Method))
        {
            Response.ContentType = "application/json; charset=utf-8";
            return Ok();
        }
        return Ok(userDto);
    }

    [HttpPost(Name = nameof(CreateUser))]
    [Produces("application/json", "application/xml")]
    public ActionResult<Guid> CreateUser([FromBody] UserCreateRequest? user)
    {
        if (user is null)
            return BadRequest();

        if (string.IsNullOrEmpty(user.Login) || !user.Login.All(char.IsLetterOrDigit))
        {
            ModelState.AddModelError(nameof(user.Login), "Login is invalid");
        }

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        
        var userEntity = _mapper.Map<UserEntity>(user);
        var entity = _userRepository.Insert(userEntity);

        return CreatedAtRoute(
            nameof(CreateUser),
            new { userId = entity.Id },
            entity.Id);
    }
    
    [HttpPut("{userId}")]
    public IActionResult UpdateUser([FromBody] UserUpdateRequest? user, Guid userId)
    {
        if (user is null || userId == Guid.Empty)
            return BadRequest();
        if (string.IsNullOrEmpty(user.Login))
        {
            ModelState.AddModelError(nameof(user.Login), "Login is invalid");
        }

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        
        var userEntity = _mapper.Map<UserEntity>(user, opt =>
        {
            opt.Items["Id"] = userId;
        });
        
        _userRepository.UpdateOrInsert(userEntity, out var isInsterted);
        if (!isInsterted)
            return NoContent();
        
        var userDto = _mapper.Map<UserDto>(userEntity);
        
        return CreatedAtRoute(
            nameof(CreateUser),
            userDto);
    }

    [HttpDelete("{userId}")]
    public IActionResult DeleteUser(Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user is null)
            return NotFound();
        
        _userRepository.Delete(userId);
        return NoContent();
    }
}