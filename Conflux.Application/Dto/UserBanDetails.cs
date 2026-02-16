namespace Conflux.Application.Dto;

public readonly record struct UserBanDetails(string? Reason, TimeSpan BanDuration, DateTime BanExpiresAt);