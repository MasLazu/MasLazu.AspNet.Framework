using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace MasLazu.AspNet.Framework.Endpoint.EndpointGroups;

public class V1EndpointGroup : SubGroup<ApiEndpointGroup>
{
    public V1EndpointGroup()
    {
        Configure("v1", ep => ep.Description(x => x.WithTags("V1")));
    }
}