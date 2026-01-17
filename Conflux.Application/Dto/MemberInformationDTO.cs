namespace Conflux.Application.Dto;

public record struct MemberInformationDTO(
    Guid MemberId,
    RolePermissionsWithId Role,
    DateTime? UnbanAt
);