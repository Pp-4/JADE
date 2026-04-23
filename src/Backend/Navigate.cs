using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Data.Analysis;
using Microsoft.Playwright;

using JADE.models;


namespace JADE.Backend;

/// <summary>
/// This class is used to navigate wapro service
/// </summary>
/// <param name="_page">playwright page</param>
/// <param name="_config">configuration class</param>
public partial class BackendNavigation(IPage _page, IConfiguration _config)
{
    readonly IPage page = _page;
    readonly IConfiguration config = _config;

    //set the default type of search id
    SearchBy searchIdType = SearchBy.TRADEID;
}
enum SearchBy
{
    TRADEID,
    PRODUCTID,
}