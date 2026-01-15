namespace Conflux.Application.Dto;

public struct ReportedMessageDTO {
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public DateTime CreatedAt { get; set; }
}