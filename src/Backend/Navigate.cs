using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

using JADE.models;
using JADE.Utility;

namespace JADE.Backend;

/// <summary>
/// This class is used to navigate wapro service
/// </summary>
/// <param name="_page">playwright page</param>
/// <param name="_config">configuration class</param>
public partial class BackendNavigation(IPage _page, Config _config, ILogger _logger, Lang _lang)
{
    readonly IPage page = _page;
    readonly Config config = _config;
    readonly ILogger logger = _logger;
    readonly Lang lang = _lang;

    //set the default type of search id
    SearchBy searchIdType = SearchBy.TRADEID;
}
enum SearchBy
{
    TRADEID,
    PRODUCTID,
}