using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class PlanoAcaoItem_Model
    {
        public int? id { get; set; }
        public int? plano_acao { get; set; }
        public int? plano_acao_item_pai { get; set; }
        public string descricao { get; set; }
        //public string responsavel { get; set; }
        public List<Responsavel_Model> responsavel { get; set; }
        public int? setor { get; set; }
        public DateTime? data_prazo { get; set; }
        public DateTime? data_conclusao { get; set; }
        public int? status { get; set; }
        public string status_nome { get; set; }
        public int efetividade { get; set; }
        public string efetividade_nome { get; set; }
        public DateTime? data_cadastro { get; set; }
        public DateTime? data_alteracao { get; set; }
        public int usuario { get; set; }
        public string usuario_email { get; set; }
        public string usuario_nome { get; set; }
        public int? ficha_cadastro { get; set; }
        public int? nova_regra { get; set; }
        public int posicao { get; set; }
        // Campos calculados
        public int dias_para_finalizar { get; set; }
        public string? codigo { get; set; }
        public long? qtde_observacoes { get; set; }
        public string? item_filho_check { get; set; }
        public string? qtde_itens_filhos { get; set; }
        public string responsavel_nomes { get; set; }
        public List<PlanoAcaoItem_Model> itens_filhos { get; set; }
    }
}
