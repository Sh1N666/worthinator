using ApiInator.Infrastructure;

namespace ApiInator.Application.HowLongToBeatApi;

public class HowLongToBeatApi(HltbPythonIntegrator integrator)
{
  public async Task<HltbResponse> GetByName(string name)
  {
    return await integrator.FetchByNameAsync(name);
  }

  public async Task<HltbResponse> GetById(int id)
  {
    return await integrator.FetchByIdAsync(id);
  }
}