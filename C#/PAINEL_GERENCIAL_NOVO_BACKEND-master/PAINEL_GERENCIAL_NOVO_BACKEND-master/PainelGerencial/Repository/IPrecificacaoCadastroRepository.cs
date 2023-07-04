using PainelGerencial.Domain.PrecificacaoCadastro;
using PainelGerencial.Domain.ShoppingDePrecos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public interface IPrecificacaoCadastroRepository
    {
        Task<IEnumerable<SolicitacoesKit_Model>> SolicitacaoKitPendente(string produtoCodigoExterno);
        Task<IEnumerable<Solicitacao_Model>> ListarSolicitacoes(
            string ordenarPor,
            int pagina,
            int deslocamento,
            string? produtoCodigoExternoPai,
            int solicitacaoTipoCodigo,
            int solicitacaoStatusCodigo,
            int? produtoListaPrecoCodigo,
            int? produtoGrupoCodigo,
            string produtoGrupoCodigos,
            bool somentePrecosReduzidos,
            string solicitanteResponsavelEmail,
            string aprovadorResponsavelEmail,
            DateTime dataInicial,
            DateTime dataFinal,
            bool somentePais
            );

        Task<IEnumerable<SolicitacoesKit_Model>> ListarSolicitacoesKit(
            string? produtoCodigoExternoPai,
            int? produtoListaPrecoCodigo,
            int? produtoGrupoCodigo,
            string? solicitanteResponsavelEmail,
            string? aprovadorResponsavelEmail, 
            int pagina,
            int deslocamento
            );

        int ConfirmarSolicitacaoKit(int codigoSolicitacaoKit);
        int FinalizaSolicitacaoKit(int codigo);
        int GravaSolicitacaoTemporaria(SolicitacaoTemporaria_Model soliticacao);
        int GerarSolicitacoesTemporarias(string dkProdutoPai, string emailSolicitante, int tipoKit, int listaPreco = 2);
        Task<IEnumerable<SolicitacaoTemporaria_Model>> ListaSolicitacoesTemporarias(string email);
        Task<int> CancelarERemoverPorUsuario(string email);
        Task<SolicitacoesKit_Model> ObterRecente(int listaPreco, string produtoCodigoExterno);
        Task<int> AtualizarSolicitacaoTemporaria(SolicitacaoTemporaria_Model soliticacao);
        Task<int> GerarSolicitacoes(Solicitacao_Model solicitacao);
        Task<int> GerarSolicitacoesKits(Solicitacao_Model solicitacao);
        Task<IEnumerable<ListaPrecos_Model>> ListarListaPrecos();
        Task<IEnumerable<ListaPrecos_Model>> ListarLojas();
        Task<int> AtualizaListaPreco(ListaPrecos_Model lista);
        Task<IEnumerable<ListaPrecos_Model>> PercentualAjuste(int listaBase);
        Task<IEnumerable<dynamic>> Precificacoes(string Email, string CodigoExternoPai, DateTime? DataInicio, DateTime? DataFim, int? Plataforma, int pagina);
        Task<int> AvaliarPreco(int codigo, bool aprovado, string email);
        Task<IEnumerable<Precificacao_Model>> PrecificacaoPorId(int id);
        Task<IEnumerable<dynamic>> RelatorioPrecificacoes(
            string Email,
            string CodigoExternoPai,
            string? CodigoExterno,
            DateTime? DataInicio,
            DateTime? DataFim,
            int? Plataforma,
            bool? aprovado,
            bool? pendente,
            int pagina);
    }
}
