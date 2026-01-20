using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Abstracts;

public readonly record struct CommunityCreatedEventArgs(Community Community);

public interface ICommunityService {
    event Action<CommunityCreatedEventArgs>? OnUserCreatedCommunity;
    
    Task<bool> CreateCommunityAsync(string name, Stream? avatarStream, string creatorId);

    Task CreateChannelCategoryAsync(string name, Guid communityId);
    Task CreateChannelAsync(string name, CommunityChannelType type, Guid channelCategoryId);
    
    Task<bool> JoinCommunityAsync(string userId, Guid communityId, Guid invitationId);
    
     Task<RolePermissionsWithId?> GetUserRolePermissionsAsync(string userId, Guid communityId);

    Task<bool> SetMembersRoleAsync(Guid communityId, IReadOnlyCollection<Guid> memberIds, Guid? roleId);
    
    Task<Guid> GetMemberId(Guid communityId, string userId);
    
    Task<MemberDisplayDTO?> GetMemberDisplayAsync(Guid memberId);
    Task<MemberDisplayDTO?> GetMemberDisplayAsync(Guid communityId, string userId);

    Task<bool> BanMemberAsync(Guid communityId, Guid memberId, TimeSpan banDuration);
    Task<bool> BanMemberAsync(ApplicationDbContext dbContext, Guid communityId, Guid memberId, TimeSpan banDuration);

    Task<MemberInformationDTO?> GetMemberInformationAsync(Guid memberId);
    Task<MemberInformationDTO?> GetMemberInformationAsync(Guid communityId, string userId);
}