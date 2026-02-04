using Conflux.Application.Dto;

namespace Conflux.Application.Abstracts;

public interface IStatisticsService {
    Task<UserStatisticsDTO> GetUserStatistics();
}