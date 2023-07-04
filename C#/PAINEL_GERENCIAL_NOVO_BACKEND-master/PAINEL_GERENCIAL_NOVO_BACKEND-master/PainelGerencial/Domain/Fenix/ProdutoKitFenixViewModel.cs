using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PainelGerencial.Domain.Fenix
{
    public class ProdutoKitFenixViewModel
    {
        public string CodigoExterno { get; set; }

        public string CodigoExternoPai { get; set; }

        public double? PrecoDe { get; set; }

        public double? PrecoPor { get; set; }
    }

    public class ProdutoKitComponenteViewModel
    {
        public string CodigoExterno { get; set; }

        public int ListaPrecoCodigo { get; set; }

        public double Quantidade { get; set; }

        public double? ValorDe { get; set; }

        public double? ValorPor { get; set; }
    }

    public class ProdutoKitComComponentesViewModel : ProdutoKitFenixViewModel
    {
        public IEnumerable<ProdutoKitComponenteViewModel> Componentes { get; set; }

        public static explicit operator ProdutoKitComComponentesViewModel(Task<ProdutoKitComComponentesViewModel> v)
        {
            throw new NotImplementedException();
        }
    }
}
