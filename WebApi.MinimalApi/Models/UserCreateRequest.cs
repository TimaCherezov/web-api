using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.MinimalApi.Models;

public record UserCreateRequest(
    [Required] string Login,
    [DefaultValue("John")] string FirstName,
    [DefaultValue("Doe")] string LastName);