using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    [Serializable]
    public class PlanoAcao_Model
    {
        public long? id { get; set; }
        public string? fornecedor { get; set; }
        public int? marca { get; set; }
        public string? marca_nome { get; set; }
        public DateTime? data_reuniao { get; set; }
        public string descricao { get; set; }
        public DateTime? data_cadastro { get; set; }
        public DateTime? data_alteracao { get; set; }
        public int? usuario { get; set; }
        public string usuario_email { get; set; }
        public string usuario_nome { get; set; }
        public string? hora_inicial { get; set; }
        public string? hora_final { get; set; }
        public string participantes { get; set; }
        public string pauta { get; set; }
        public string? local { get; set; }
        public int? qtde_interacoes { get; set; }
    }
}
