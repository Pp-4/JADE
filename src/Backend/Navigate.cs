using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

using JADE.models;

namespace JADE.Backend;

/// <summary>
/// This class is used to navigate wapro service
/// </summary>
/// <param name="_page">playwright page</param>
/// <param name="_config">configuration class</param>
public partial class BackendNavigation(IPage _page, Config _config, ILogger _logger)
{
    readonly IPage page = _page;
    readonly Config config = _config;
    readonly ILogger logger = _logger;

    //set the default type of search id
    SearchBy searchIdType = SearchBy.TRADEID;
}
enum SearchBy
{
    TRADEID,
    PRODUCTID,
}