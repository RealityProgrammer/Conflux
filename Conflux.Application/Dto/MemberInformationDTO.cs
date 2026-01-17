namespace Conflux.Application.Dto;

public readonly record struct MemberInformationDTO(
    Guid MemberId,
    RolePermissionsWithId Role,
    DateTime? UnbanAt
) {
    public bool IsBanned => UnbanAt != null && DateTime.UtcNow < UnbanAt.Value;
}