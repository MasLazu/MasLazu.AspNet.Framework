using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace MasLazu.AspNet.Framework.Endpoint.EndpointGroups;

public class ApiEndpointGroup : Group
{
    public ApiEndpointGroup()
    {
        Configure("api", ep => ep.Description(x => x.WithTags("Api")));
    }
}