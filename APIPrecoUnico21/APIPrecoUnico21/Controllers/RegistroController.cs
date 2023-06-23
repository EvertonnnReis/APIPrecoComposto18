using APIPrecoUnico21.Services;
using Microsoft.AspNetCore.Mvc;

namespace APIPrecoUnico21.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class RegistroController : ControllerBase
    {
        private readonly IServices _services;

        public RegistroController(IConfiguration configuration,
            IServices services)
        {
            _services = services;
        }



        [HttpGet]
        public async Task<dynamic> ObterRegistros()
        {
            var registro = await _services.ObterRegistros("655BE0B5-0BC4-4028-A0DD-FB243CAF2A84");

            // Retorna os registros como resposta JSON
            return new { registro };
        }
    }
}
