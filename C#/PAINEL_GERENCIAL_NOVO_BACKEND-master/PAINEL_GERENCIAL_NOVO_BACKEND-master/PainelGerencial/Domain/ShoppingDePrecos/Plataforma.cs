using System.Collections;
using System.Collections.Generic;

namespace PainelGerencial.Domain.ShoppingDePrecos
{
    public class Plataforma
    {
        public bool status { get; set; }
        public int quantidade { get; set; }
        public IList<ItemPlataforma> registros { get; set; }
    }

    public class ItemPlataforma
    {
        public int id { get; set; }
        public string titulo { get; set; }
        public string tipo { get; set; }
        public bool ativo { get; set; }
        public string icone { get; set; }
    }
}
