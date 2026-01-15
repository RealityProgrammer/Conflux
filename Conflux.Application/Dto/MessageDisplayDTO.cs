using Conflux.Domain.Entities;

namespace Conflux.Application.Dto;

public record struct MessageDisplayDTO(string SenderDisplayName, string? SenderAvatar, string? Body, List<MessageAttachment> Attachments);