using PainelGerencial.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public interface IPainelGerencialRepository
    {
        public IEnumerable<dynamic> PreAnalise();
        public IEnumerable<dynamic> PedidoMaisAntigoPreAnalise();
        public IEnumerable<dynamic> PedidoMaisAntigoFaturamentoAFaturar();
        public IEnumerable<dynamic> AFaturar();
        public IEnumerable<dynamic> Faturados();
        public IEnumerable<dynamic> PedidoMaisAntigoFaturamentoPendentesVendas();
        public IEnumerable<dynamic> PendentesVendas();
        public IEnumerable<dynamic> FaturamentoHoje();
        public IEnumerable<dynamic> FaturamentoOntem();
        public IEnumerable<dynamic> FaturamentoUltimos7Dias();
        public IEnumerable<dynamic> FaturamentoMes();
        public IEnumerable<dynamic> PedidosAProcessarPelaAPI();
        public IEnumerable<dynamic> PedidosACorrigirEstoque();
        public IEnumerable<dynamic> PedidoMaisAntigoACorrigirEstoque();
        // Não usados
        /*
        public IEnumerable<dynamic> FaturamentoPorHora();
        public IEnumerable<dynamic> PedidosAguardandoSeparacao();
        public IEnumerable<dynamic> PerfilDosPedidosFaturados();
        public IEnumerable<dynamic> PerfilDosPedidosAFaturar();
        */
        public IEnumerable<dynamic> PendentesOutrasSaidas();
        public IEnumerable<dynamic> PendentesDevolucoes();
        public IEnumerable<dynamic> UsuariosLogin(string email, string senha);
        public bool UsuariosLogout(int id);
        public bool UsuariosVerificaSessao(int id, string ip);
        public IEnumerable<dynamic> UsuariosListar();
        public IEnumerable<dynamic> UsuariosDados(int id);
        public IEnumerable<dynamic> UsuariosGrupos();
        public IEnumerable<dynamic> UsuariosSetores();
        public IEnumerable<Usuario> UsuarioPorEmail(string email);
        public int UsuariosCadastrar(Usuario usuario);
        public bool UsuariosExcluir(int id);

        public IEnumerable<Relatorio_Model> FaturamentoPorHora();

        public IEnumerable<dynamic> FaturamentoDespachados();
        public IEnumerable<dynamic> FaturamentoNaoDespachados();
        public IEnumerable<dynamic> PedidosAguardandoSeparacao();
        public IEnumerable<dynamic> PerfilDosPedidosFaturados();
        public IEnumerable<dynamic> PerfilDosPedidosAFaturar();
        // Captacao
        public IEnumerable<dynamic> CaptacaoLojaVirtualIntegracaoVtex();
        public IEnumerable<dynamic> CaptacaoMarketPlaceIntegracaoVtex();
        public IEnumerable<dynamic> CaptacaoMarketPlaceConnectIntegracaoVtex();
        public IEnumerable<dynamic> CaptacaoMercadoLivreIntegracaoVtex();
        public IEnumerable<dynamic> CaptacaoPedidosDigitadosIntegracaoVtex();
        public IEnumerable<dynamic> FaturamentoCreditoAprovado();
        public Task<dynamic> AbacosInsereCotacao(Cotacao_Model cotacao);
        public Task<IEnumerable<dynamic>> PedidosAPIConsulting();
        public Task<IEnumerable<dynamic>> Cubos();
        public Task<IEnumerable<dynamic>> ReplicacaoDataWarehouse();
        public Task<IEnumerable<dynamic>> EnvioIntelipost();
        public Task<IEnumerable<dynamic>> Integracao00k();
        public Task<IEnumerable<dynamic>> IntegracaoVtex();
        public Task<IEnumerable<dynamic>> PedidosIntegradosPorHoraVia00k();
        public Task<IEnumerable<dynamic>> PedidosIntegradosPorHoraViaVtex();
        public Task<IEnumerable<dynamic>> RelatorioPedidosPorPeriodo(DateTime dataInicial, DateTime dataFinal, int deslocamento, int limite);
        public Task<IEnumerable<dynamic>> RelatorioSaldoPorCodigo(String dk);
        public Task<IEnumerable<dynamic>> RelatorioSaldoPorSaldo1();
        public Task<IEnumerable<dynamic>> RelatorioPrecoPorMarketplace(string codigo, int deslocamento, int limite);
        public Task<IEnumerable<dynamic>> RelatorioPrecoPorCodigo(string dk);
        public Task<IEnumerable<dynamic>> CodigoListaDePreco();
        public Task<IEnumerable<dynamic>> RelatorioPendentesVendasPorPeriodo(DateTime dataInicial, DateTime dataFinal, int deslocamento, int limite);
        public Task<IEnumerable<dynamic>> RelatorioPendentesVendasData();
        public Task<IEnumerable<dynamic>> RelatorioPendentesVendasPorCodigo(string codigo);
        public Task<IEnumerable<dynamic>> RelatorioPendentesVendasPorPeriodoGrafico(DateTime dataInicial, DateTime dataFinal);
        public Task<IEnumerable<dynamic>> RelatorioPrimeiroEnvioNucci();
        public Task<IEnumerable<dynamic>> RelatorioPrimeiroEnvioComErroNucci();
        public Task<IEnumerable<dynamic>> RelatorioAguardandoDespachoNucci();
    }
}
