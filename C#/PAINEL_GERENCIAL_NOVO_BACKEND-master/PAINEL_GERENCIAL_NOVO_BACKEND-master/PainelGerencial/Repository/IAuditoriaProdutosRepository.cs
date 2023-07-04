using Microsoft.AspNetCore.Mvc;
using PainelGerencial.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public interface IAuditoriaProdutosRepository
    {
        public IEnumerable<dynamic> DadosProduto();
        public IEnumerable<dynamic> DadosProduto(string codigoExterno);
        public dynamic DadosAbacos(string codigoExterno);
        public IEnumerable<dynamic> KitsProduto(string codigoExterno);
        public Task<int> LimparProdutosPaiAsync();
        public Task<int> LimparProdutosFilhosAsync();
        public int InsereProdutoPai(ProdutoPai_Model produto);
        public int InsereProdutoPaiEmLote();
        public IEnumerable<dynamic> DksPai();
        public IEnumerable<dynamic> DadosProdutoPai(int prosCod);
        public int InsereProdutoFilho(ProdutoFilho_Model produto);
        public int InsereProdutoFilhoEmLote();
        public IEnumerable<ProdutoPai_Model> DadosProdutosPai();
        public IEnumerable<ProdutoFilho_Model> DadosProdutoFilho(int paiId);
        public int LimpaProdutosDivergentes();
        public int InsereDivergencia(Divergencia_Produto_Model divergencia, int idProduto);
        public int InsereProdutosDivergentes(ProdutoDivergente_Model produto);
        public IEnumerable<ProdutoDivergente_Model> ListaProdutosDivergentesAsync();
        public bool ProdutoExistente(string codigoExterno);
        public IEnumerable<dynamic> Fornecedores(string dk);
        public IEnumerable<dynamic> FornecedoresProduto(string codigoExterno);
        public Task<IList<ProdutoDivergente_Model>> RelatorioMedidasDivergentes(string dk, string nome);
        public IEnumerable<dynamic> RelatorioAuditoria();
        public IEnumerable<dynamic> Historico(string dk);
        public bool ProdutoKit(string codigoExterno);
        public IEnumerable<dynamic> RelatorioGeral(DateTime? dataInicial, DateTime? dataFinal, int pagina = 1, int offset = 100);
        public bool VerificaPeso(string dk, float peso);
        public IEnumerable<dynamic> BuscaProdutoCorrigido(int idProduto, string dk);
        public IEnumerable<dynamic> VerificaKit(string dk);
        public IEnumerable<dynamic> VerificaFilhosKit(string dk);
        public bool CorrigidoAtualizaProser(double largura, double comprimento, double espessura, double peso, List<string> condIn);
        public bool CorrigidoAtualizaProducts(string dk, int idProduto);
        public bool InsereHistoricoVtex(Dictionary<string, object> data);
    }
}
