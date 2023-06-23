using APIPrecoComposto18.Services;
using Microsoft.AspNetCore.Mvc;

namespace APIPrecoComposto18.Controller
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
            var registro = await _services.ObterRegistros("F70D24D1-A024-403E-9CD7-C345D3B37F6A");

            // Retorna os registros como resposta JSON
            return new { registro };
        }
    }
}

