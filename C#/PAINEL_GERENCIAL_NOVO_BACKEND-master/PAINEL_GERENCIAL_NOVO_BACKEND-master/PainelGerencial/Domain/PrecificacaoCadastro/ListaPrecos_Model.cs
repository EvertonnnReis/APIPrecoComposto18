namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class ListaPrecos_Model
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public float PercentualAjuste { get; set; }
        public bool Ativa { get; set; }
        public int Id_Lista_Base { get; set; }
        public ListaPrecos_Model? ListaPrecosBase { get; set; }
    }
}
