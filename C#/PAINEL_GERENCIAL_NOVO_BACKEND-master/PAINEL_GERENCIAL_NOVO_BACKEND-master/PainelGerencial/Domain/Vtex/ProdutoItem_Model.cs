using System.Collections.Generic;

namespace PainelGerencial.Domain.Vtex
{
    public class ProdutoItem_Model
    {
        public string itemId { get; set; }
        public string name { get; set; }
        public List<Referencia_Model> referenceId { get; set; }
        public List<Imagem_Model> images { get; set; }
    }
}
