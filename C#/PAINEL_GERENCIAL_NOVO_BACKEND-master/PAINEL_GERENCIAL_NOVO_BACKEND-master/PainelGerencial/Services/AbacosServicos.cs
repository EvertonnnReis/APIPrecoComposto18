using Domain.Abacos;
using PainelGerencial.Utils.Enums;
using System.Threading.Tasks;
using System;
using PainelGerencial.Utils;
using Domain.Vtex;
using PainelGerencial.Repository;
using System.Linq;
using PainelGerencial.Domain.Vtex;
using PainelGerencial.Helpers;
using System.Collections.Generic;
using Newtonsoft.Json;
using AbacosWSPlataforma;
using Repository;
using AbacosWSERP;
using DadosPreco = AbacosWSERP.DadosPreco;

namespace PainelGerencial.Services
{
    public class AbacosServicos : IAbacosServicos
    {
        private static IVtexServicos _vtexServicosExternos;
        private static IProdutoConversao _produtoConversao;
        private static IVtexRepository _vtexRepository;
        private static IAbacosRepository _abacosRepository;
        private static AbacosWSPlataformaSoapClient _abacosWs;
        private static AbacosWSERPSoapClient _abacosWSERP;

        public AbacosServicos(
            IVtexServicos vtexServicosExternos,
            IProdutoConversao produtoConversao,
            IVtexRepository vtexRepository,
            IAbacosRepository abacosRepository,
            IWsAbacos wsAbacos)
        {
            _vtexServicosExternos = vtexServicosExternos;
            _produtoConversao = produtoConversao;
            _vtexRepository = vtexRepository;
            _abacosRepository = abacosRepository;
            _abacosWs = wsAbacos.AbacosWs();
            _abacosWSERP = new AbacosWSERPSoapClient(AbacosWSERPSoapClient.EndpointConfiguration.AbacosWSERPSoap12);
        }

        public void AtivarProduto(ProdutoVtexViewModel produto, ProdutoAbacosViewModel produtoAbacos)
        {
            try
            {
                SkuByRefIdVtexViewModel sku = ProdutoConversao.ConverterSkuVtex(produtoAbacos, produto.Id, null);
                _vtexServicosExternos.CriarSku(sku);
            }
            catch (Exception ex)
            {
                _vtexRepository.GerarLogIntegracao(produto.RefId ?? "Produto", "CodigoExterno", ex.Message, "AtivarProduto", (int)LogTipo.Produto);
            }
        }

        public async Task<bool> AtualizarProduto(ProdutoVtexViewModel produto, ProdutoAbacosViewModel abacosViewModel)
        {
            try
            {
                var existeProduto = await _vtexServicosExternos.ObterProdutoPorCodigoReferencia(produto.RefId);
                ProdutoVtexViewModel resultado;
                if (existeProduto == null)
                {
                    resultado = await _vtexServicosExternos.CriarProduto(produto);
                }
                else
                {
                    produto.Id = existeProduto.Id;
                    resultado = await _vtexServicosExternos.AtualizarProduto(produto);
                }

                if (produto.IsActive == true && resultado.IsActive == false)
                {
                    this.AtivarProduto(resultado, abacosViewModel);
                }

                if (resultado.RefId == abacosViewModel.CodigoProduto) return true;

                _vtexRepository.GerarLogIntegracao(abacosViewModel.CodigoProduto, "CodigoExterno", "Webservice retornou objeto incorreto", "AtualizarProduto",
                    (int)LogTipo.Produto);
            }
            catch (Exception ex)
            {
                _vtexRepository.GerarLogIntegracao(abacosViewModel.CodigoProduto, "CodigoExterno", ex.Message, "AtualizarProduto",
                    (int)LogTipo.Produto);
                if (ex.Message.Contains("com o mesmo LinkId"))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> IntegracaoProdutos(ProdutoAbacosViewModel abacosViewModel)
        {
            var produtoVtex = await _vtexServicosExternos.ObterProdutoPorCodigoReferencia(abacosViewModel.CodigoProduto);

            var produto = _produtoConversao.ConverterProdutoVtex(abacosViewModel, produtoVtex);

            if (produto.CategoryId <= 0)
            {
                produto.CategoryId = 1311;
            }

            return await AtualizarProduto(produto, abacosViewModel);
        }

        public async Task<int?> RecuperarProdutoIdSkuNovo(ProdutoAbacosViewModel abacosProduto)
        {
            var produto = await _vtexServicosExternos.ObterProdutoPorCodigoReferencia(abacosProduto.CodigoProdutoPai);

            return produto?.Id;
        }

        private static ProdutoAbacosViewModel ConverterProduto(AbacosWSPlataforma.DadosProdutos produto)
        {
            var json = JsonConvert.SerializeObject(produto);

            var obj = JsonConvert.DeserializeObject<ProdutoAbacosViewModel>(json);
            return obj;
        }

        public List<ProdutoAbacosViewModel> ConverterListaProduto(AbacosWSPlataforma.DadosProdutos[] produtos)
        {
            var listaProdutos = produtos.Select(ConverterProduto).ToList();
            return listaProdutos;
        }

        public async Task<bool> AtualizarSku(SkuByRefIdVtexViewModel sku, ProdutoAbacosViewModel abacosViewModel)
        {
            try
            {
                var existeSku = await _vtexServicosExternos.SkuPorRefId(sku.RefId);
                SkuByRefIdVtexViewModel resultado;
                if (existeSku == null)
                {
                    resultado = await _vtexServicosExternos.CriarSku(sku);
                }
                else
                {
                    resultado = await _vtexServicosExternos.AtualizarSku(sku);
                }

                await _vtexServicosExternos.AtualizarEan((int)sku.Id, abacosViewModel.CodigoBarras);

                if (resultado.RefId == abacosViewModel.CodigoProduto) return true;

                _vtexRepository.GerarLogIntegracao(abacosViewModel.CodigoProduto, "CodigoExterno", "Webservice retornou objeto incorreto", "AtualizarSku",
                   (int)LogTipo.Produto);
            }
            catch (Exception ex)
            {
                _vtexRepository.GerarLogIntegracao(abacosViewModel.CodigoProduto, "CodigoExterno", ex.Message, "AtualizarSku",
                   (int)LogTipo.Produto);
            }
            return false;
        }

        public async Task<StockKeepingUnitKit> GerarComponenteKit(LinhaComponentesKitViewModel componente, int? idSku)
        {
            var idComponenteSku = await _vtexServicosExternos.SkuPorRefId(componente.CodigoProdutoComponente);

            var listaComponente = await _vtexServicosExternos.SkuListaKitPorParentId(Convert.ToInt32(idSku));

            if (idComponenteSku == null)
            {
                return null;
            }

            StockKeepingUnitKit referenciaSkuComponente = null;
            if (listaComponente != null)
            {
                referenciaSkuComponente = listaComponente.FirstOrDefault(w => w.StockKeepingUnitId == idComponenteSku?.Id);
            }

            var skuKit = new StockKeepingUnitKit
            {
                Quantity = Convert.ToInt32(componente.Quantidade),
                StockKeepingUnitId = idComponenteSku.Id,
                StockKeepingUnitParent = idSku
            };

            if (referenciaSkuComponente != null)
            {
                skuKit.Id = referenciaSkuComponente.Id;
            }

            if (componente.PrecoPromocional > 0)
            {
                skuKit.UnitPrice = Convert.ToDecimal(componente.PrecoPromocional);
                return skuKit;
            }

            var produtoAbacos = await _abacosRepository.ProdutoListaPreco(1, idComponenteSku.RefId); //RecuperarProdutoFenix(idComponenteSku.RefId);
            var valor = produtoAbacos.FirstOrDefault().PROL_VAL_PREPRO;

            skuKit.UnitPrice = valor;

            return skuKit;
        }

        public async Task AtualizarKit(StockKeepingUnitKit kit)
        {
            try
            {
                var existeKit = await _vtexServicosExternos.ExisteProduto(kit);
                StockKeepingUnitKit resultado = existeKit;
                if (kit.Id == null)
                {
                    resultado = await _vtexServicosExternos.CriarSkuKit(kit);
                }

                if (resultado != null) return;

                _vtexRepository.GerarLogIntegracao(kit.ToString(), "SkuId",
                    $"Falha ao Inserir/Atualizar componentes de KIT Componente com SkuId {kit}",
                    "AtualizarKit", (int)LogTipo.Produto);
            }
            catch (Exception ex)
            {
                _vtexRepository.GerarLogIntegracao(kit.ToString(), "SkuId",
                    $"Componente com SkuId {kit} | Erro {ex.Message}",
                    "AtualizarKit", (int)LogTipo.Produto);
            }
        }


        public async Task GerarKit(ProdutoAbacosViewModel produto, int? idSku)
        {
            var componentes = produto.ComponentesKit.Linhas.Select(async s => await GerarComponenteKit(s, idSku)).ToList();

            foreach (var componente in componentes)
            {
                if (componente != null)
                {

                    var componenteRequest = await componente;
                    AtualizarKit(componenteRequest);
                }
                continue;
            }
        }

        public async Task<bool> IntegracaoSkus(ProdutoAbacosViewModel abacosViewModel)
        {
            if (!AbacosHelper.ValidarDimensoesSku(abacosViewModel)) return true;

            var skuVtex = await _vtexServicosExternos.SkuPorRefId(abacosViewModel.CodigoProduto);

            var produtoCod = skuVtex == null
                ? await RecuperarProdutoIdSkuNovo(abacosViewModel)
                : skuVtex.ProductId;

            if (produtoCod == null)
            {
                //var log = new LogViewModel
                //{
                //    Erro = "Não foi possível localizar o produto " + abacosViewModel.CodigoProduto,
                //    Metodo = "IntegracaoProduto",
                //    Origem = "IntegracaoProduto",
                //    Parametro = abacosViewModel.CodigoProduto,
                //    Tipo = 2
                //};
                //_log.RegistrarLog(log);

                return false;
            }

            var sku = ProdutoConversao.ConverterSkuVtex(abacosViewModel, produtoCod, skuVtex);

            //TODO: Melhorar o desempenho do AtualizarSKU
            var atualizado = await AtualizarSku(sku, abacosViewModel);

            if (skuVtex != null && sku.IsKit == true)
            {
                //TODO: Melhorar o desempenho do GerarKit
                GerarKit(abacosViewModel, skuVtex.Id);
            }

            return atualizado;
        }

        public async Task<bool> ProdutoOuSku(ProdutoAbacosViewModel produtoVm)
        {
            try
            {

                //_produtoConversao.GerarLogAlteracaoProduto(produtoVm);
                if (produtoVm.CodigoProduto.StartsWith("FC", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }


                if (produtoVm.CodigoProdutoPaiAbacos == 0)
                {
                    return await IntegracaoProdutos(produtoVm);
                }
                else
                {
                    return await IntegracaoSkus(produtoVm);
                }
            }
            catch (Exception ex)
            {
                _vtexRepository.GerarLogIntegracao(produtoVm.CodigoProduto, "CodigoExterno", ex.Message, "ProdutoOuSku",
                   (int)LogTipo.Produto);
            }

            return false;
        }

        public async Task ConfirmarRecebimentoProduto(string protocolo)
        {
            var resultado = await _abacosWs.ConfirmarRecebimentoProdutoAsync(protocolo);
            if (resultado.Tipo != "tdreSucesso")
            {
                _vtexRepository.GerarLogIntegracao(protocolo, "Protocolo", resultado.ExceptionMessage, "ConfirmarRecebimentoProduto", (int)LogTipo.Produto);
            }
        }

        public async Task<bool> EnviarWsdlAbacos(DadosPreco[] dados)
        {
            var resposta = await _abacosWSERP.AtualizarPrecoAsync("214137DA-076F-4E3B-934E-A4029A5C9022", dados);
            var produto = resposta.Rows.ToList().FirstOrDefault();

            if (produto?.Resultado.Tipo == "tdreSucesso")
            {
                return true;
            }
            else
            {
                _vtexRepository.GerarLogIntegracao(
                    dados[0].CodigoProduto,
                    "CodigoExterno",
                    produto.Resultado.ExceptionMessage.ToString(),
                    "EnviarWsdlAbacos",
                    (int)LogTipo.Produto
                    );

                return false;
            }
        }

        public async Task<bool> AlterarPrecoComponenteKit(DadosItemKit[] componente)
        {
            var resposta = await _abacosWSERP.AlterarPrecoComponenteKitAsync("214137DA-076F-4E3B-934E-A4029A5C9022", componente);

            if (resposta.ResultadoOperacao.Tipo == "tdreSucesso")
            {
                return true;
            }
            else
            {
                _vtexRepository.GerarLogIntegracao(
                    componente[0].CodigoProduto,
                    "CodigoExterno",
                    resposta.ResultadoOperacao.ExceptionMessage,
                    "AlterarPrecoComponenteKit",
                    (int)LogTipo.Produto
                    );

                return false;
            }
        }
    }
}
