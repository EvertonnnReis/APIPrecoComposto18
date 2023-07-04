using Domain.Vtex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using PainelGerencial.Domain;
using PainelGerencial.Domain.Abacos;
using PainelGerencial.Domain.Vtex;
using PainelGerencial.Repository;
using PainelGerencial.Services;
using RecursosCompartilhados.ViewModels.Vtex;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class VtexSellerController : ControllerBase
    {
        private IVtexRepository _vtexRepository;
        private IAbacosRepository _abacosRepository;
        private IVtexServicos _vtexServicos;
        private const string _urlVtex = "https://connectparts.vtexcommercestable.com.br/";

        public VtexSellerController(IVtexRepository wrepository, IAbacosRepository abacosRepository, IVtexServicos vtexServicos)
        {
            _vtexRepository = wrepository;
            _abacosRepository = abacosRepository;
            _vtexServicos = vtexServicos;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<dynamic> ListaSellersAtivos()
        {
            List<SellerParamModel> parametros = new List<SellerParamModel>();
            ListSellersModel listaSellers = await _vtexServicos.Get_Lista_Sellers_Ativos(parametros);
            return listaSellers;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<dynamic> AtualizaSellerList()
        {
            List<SellerParamModel> parametros = new List<SellerParamModel>();
            List<ListSellersModel> listSellers = new List<ListSellersModel>();
            ListSellersModel getSeller = await _vtexServicos.Get_Lista_Sellers_Ativos(parametros);
            listSellers.Add(getSeller);
            if (getSeller.Paging.Total > getSeller.Paging.To)
            {
                while (getSeller.Paging.Total > getSeller.Paging.To)
                {
                    int Ate = 0;
                    if (getSeller.Paging.Total - getSeller.Paging.To > 100)
                    {
                        Ate = getSeller.Paging.To + 100;
                    }
                    else
                    {
                        Ate = getSeller.Paging.Total;
                    }
                    parametros.Clear();

                    parametros.Add(new SellerParamModel()
                    {
                        Key = "From",
                        Value = getSeller.Paging.To.ToString(),

                    });
                    parametros.Add(new SellerParamModel()
                    {
                        Key = "To",
                        Value = Ate.ToString(),
                    });
                    getSeller = await _vtexServicos.Get_Lista_Sellers_Ativos(parametros);
                    listSellers.Add(getSeller);
                }
            }

            List<TbListSellersModel> SellersModel = new List<TbListSellersModel>();
            List<TbListSellersModel> SellersFiltro = new List<TbListSellersModel>();

            SellersModel = await _vtexRepository.BuscaSellersSalvos(true);
            foreach (var Salvo in SellersModel)
            {

            }
            return listSellers;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<dynamic> NovosPedidos()
        {
            return await _vtexServicos.GravaPedidos(await _vtexServicos.Get_PedidosSeller());
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<dynamic> UpdatePedidos()
        {
            return await _vtexServicos.checaStatusPedidos();
        }
    }
}