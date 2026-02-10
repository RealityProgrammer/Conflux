namespace Conflux.Application.Dto;

public readonly record struct ConversationStatisticsDTO(
    int DirectConversationCount,
    int CommunityConversationCount,
    int GlobalMessageCount
);