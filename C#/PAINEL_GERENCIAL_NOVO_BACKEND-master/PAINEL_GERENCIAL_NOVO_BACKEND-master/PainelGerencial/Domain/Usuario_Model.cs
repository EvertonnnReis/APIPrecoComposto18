using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Sessao
    {

        public bool erros { get; set; }
        public bool sessao { get; set; }
        public dynamic dados { get; set; }
        public string mensagem { get; set; }

    }

    public class Usuario
    {
        public string id { get; set; }
        public string nome { get; set; }
        public string email { get; set; }
        public string senha { get; set; }
        public string ativo { get; set; }
        public string setor { get; set; }
        public string id_setor { get; set; }
        public string grupo { get; set; }
        public string id_grupo { get; set; }
    }

    public class Usuario_Model
    {
        public long quantidade_registros { get; set; }
        public List<dynamic> relatorio { get; set; }
    }

    public class Grupo
    {
        public dynamic grupos { get; set; }
    }

    public class Setor
    {
        public dynamic setores { get; set; }
        public int id { get; set; }
        public string nome { get; set; }
    }

    public class Retorno
    {
        public bool erros { get; set; }
        public string mensagem { get; set; }
    }
    
}
