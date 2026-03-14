using ApiInator.Infrastructure;

namespace ApiInator.Application.HowLongToBeatApi;

public class HowLongToBeatApi(HltbPythonIntegrator integrator)
{
  public async Task<HltbResponse> GetByNameAsync(string name)
  {
    return await integrator.FetchByNameAsync(name);
  }

  public async Task<HltbResponse> GetByIdAsync(int id)
  {
    return await integrator.FetchByIdAsync(id);
  }
}