using System.ComponentModel.DataAnnotations;

namespace MiraiShop.Application.DTOs;

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password);
