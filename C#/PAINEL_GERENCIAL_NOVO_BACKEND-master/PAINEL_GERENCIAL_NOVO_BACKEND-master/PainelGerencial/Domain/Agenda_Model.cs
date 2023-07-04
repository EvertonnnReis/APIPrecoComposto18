using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Agenda_Model
    {
        public long? id { get; set; }
        public string para { get; set; }
        public string avisar { get; set; }
        public int? fornecedor { get; set; }
        public string fornecedor_descricao { get; set; }
        public int fornecedor_novo { get; set; }
        public string assunto { get; set; }
        public string local { get; set; }
        public DateTime data_reuniao { get; set; }
        public DateTime hora_inicial { get; set; }
        public DateTime hora_final { get; set; }
        public string observacao { get; set; }
        public int invite_enviado { get; set; }
        public DateTime data_cadastro { get; set; }
        public DateTime data_alteracao { get; set; }
        public int usuario { get; set; }
        public string usuario_nome { get; set; }
        public string usuario_email { get; set; }
        public int compras { get; set; }
        public int vendas { get; set; }
        public int exorbitante { get; set; }
        public int recebimento { get; set; }
        public int juridico { get; set; }
        public int financeiro { get; set; }
        public int trocas { get; set; }
        public int garantia { get; set; }
        public int marketing { get; set; }
        public int ti { get; set; }
        public int atendimento { get; set; }
        public int rh { get; set; }
        public int ga { get; set; }
        public int gca { get; set; }
        public int frete { get; set; }
        public int facilities { get; set; }
        public int desenvolvimento { get; set; }
    }
}
