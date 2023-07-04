using Newtonsoft.Json;

namespace PainelGerencial.Domain.Fenix
{
    public class ProdutoListaPrecoFenixViewModel
    {
        public int Codigo { get; set; }
        public string Nome { get; set; }
    }

    [JsonConverter(typeof(GroupingConverter))]
    public class ProdutoListaPrecoComValorFenixViewModel : ProdutoListaPrecoFenixViewModel
    {
        public double? ValorDe { get; set; }
        public double? ValorPor { get; set; }
    }
}
