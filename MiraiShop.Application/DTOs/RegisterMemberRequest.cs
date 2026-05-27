using System.ComponentModel.DataAnnotations;

namespace MiraiShop.Application.DTOs;

public record RegisterMemberRequest(
    [Required] string Name,
    [Required][EmailAddress] string Email,
    [Required] string Password,
    [Required][MaxLength] string MailingAddress,
    [MaxLength] string ResidentialAddress);
