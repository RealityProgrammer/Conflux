namespace Conflux.Application.Dto;

public record struct MemberDisplayDTO(Guid MemberId, string UserId, string DisplayName, string? AvatarPath);