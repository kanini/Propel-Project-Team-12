namespace Backend.Api.Services;

public interface IService
{
    // Marker interface for services
}

public abstract class BaseService : IService
{
    protected readonly ILogger _logger;

    protected BaseService(ILogger logger)
    {
        _logger = logger;
    }
}
