using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [Route("api/teste")]
    [ApiController]
    [ApiVersion(3)]
    [ApiVersion(4)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TesteV3Controller : ControllerBase
    {
        [HttpGet]
        [MapToApiVersion(3)]
        public string GetVersion3()
        {
            return "Version3 - GET - Api Versão 3.0";
        }

        [HttpGet]
        [MapToApiVersion(4)]
        public string GetVersion4()
        {
            return "Version3 - GET - Api Versão 4.0";
        }
    }
}
