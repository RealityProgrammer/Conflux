namespace Conflux.Application.Dto;

public readonly record struct DirectConversationDisplayDTO(Guid ConversationId, UserDisplayDTO OtherUserDisplay);