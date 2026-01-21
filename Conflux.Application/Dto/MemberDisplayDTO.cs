namespace Conflux.Application.Dto;

public record struct MemberDisplayDTO(Guid MemberId, Guid UserId, string DisplayName, string? AvatarPath);