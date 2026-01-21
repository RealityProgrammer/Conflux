namespace Conflux.Application.Dto;

public record struct UserDisplayDTO(Guid UserId, string DisplayName, string? UserName, string? AvatarPath);