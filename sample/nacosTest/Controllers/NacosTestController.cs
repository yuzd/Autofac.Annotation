using Autofac.Annotation;
using Microsoft.AspNetCore.Mvc;

namespace nacosTest.Controllers;

[Component]
[ApiController]
[Route("[controller]")]
public class NacosTestController : ControllerBase
{
    [Autowired] private NacosTest NacosTest;

    [HttpGet(Name = "GetNacosTest")]
    public string Get()
    {
        return NacosTest.a.Value;
    }
}