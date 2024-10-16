namespace MMLib.ServiceDiscovery.Consul;

/// <summary>
///
/// </summary>
public class ConsulNameCorrectorService : IConsulNameCorrectorService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string CorrectConsulName(string name)
    {
        return name?.Replace("MS.", string.Empty);
    }
}
