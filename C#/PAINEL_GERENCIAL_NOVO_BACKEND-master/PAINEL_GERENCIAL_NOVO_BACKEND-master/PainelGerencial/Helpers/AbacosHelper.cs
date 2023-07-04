using Domain.Abacos;
using Newtonsoft.Json;

namespace PainelGerencial.Helpers
{
    public class AbacosHelper
    {
        public static bool ValidarDimensoesSku(ProdutoAbacosViewModel produto)
        {
            return produto.Peso > 0 && produto.Altura > 0 && produto.Largura > 0 && produto.Comprimento > 0;
        }

        
    }
}
