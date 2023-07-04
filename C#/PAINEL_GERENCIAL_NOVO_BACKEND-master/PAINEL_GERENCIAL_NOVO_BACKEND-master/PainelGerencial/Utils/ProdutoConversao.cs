using Domain.Abacos;
using Domain.Vtex;
using System;
using System.Text.RegularExpressions;
using Utils;

namespace PainelGerencial.Utils
{
    public class ProdutoConversao : IProdutoConversao
    {
        public static SkuByRefIdVtexViewModel ConverterSkuVtex(ProdutoAbacosViewModel produtoAbacos, int? produtoCod, SkuByRefIdVtexViewModel? skuVtex)
        {

            var sku = new SkuByRefIdVtexViewModel
            {
                RefId = produtoAbacos.CodigoProduto,
                Height = (decimal)produtoAbacos.Altura,
                Width = (decimal)produtoAbacos.Largura,
                Length = (decimal)produtoAbacos.Comprimento,
                CubicWeight = (double)CalcularPesoCubico(produtoAbacos.Comprimento, produtoAbacos.Largura, produtoAbacos.Altura),
                WeightKg = ConverterPeso(produtoAbacos.Peso),
                PackagedWeightKg = (double)ConverterPeso(produtoAbacos.Peso),
                PackagedHeight = produtoAbacos.Altura,
                PackagedWidth = produtoAbacos.Largura,
                PackagedLength = produtoAbacos.Comprimento,
                IsKit = produtoAbacos.ProdutoKit,
                Name = produtoAbacos.NomeProduto,
                ProductId = (int)produtoCod,
                MeasurementUnit = produtoAbacos.UnidadeMedidaNome,
                ManufacturerCode = produtoAbacos.CodigoFabricante,
                CreationDate = DateTime.Now,
                ActivateIfPossible = true,
            };

            if (skuVtex == null) return sku;

            sku.Id = skuVtex.Id;
            sku.IsActive = (bool)(skuVtex?.IsActive);
            sku.CommercialConditionId = (int)(skuVtex?.CommercialConditionId);

            return sku;
        }

        public ProdutoVtexViewModel ConverterProdutoVtex(ProdutoAbacosViewModel produto, ProdutoVtexViewModel objVtex)
        {
            var produtoVtex = new ProdutoVtexViewModel
            {
                RefId = produto.CodigoProduto,
                BrandId = produto.CodigoMarca,
                CategoryId = RetornarCategoriaProduto(produto.CategoriasDoSite),
                DepartmentId = RetornarDepartamentoProduto(produto.CategoriasDoSite),
                IsActive = objVtex == null ? false : objVtex?.IsActive,
                IsVisible = objVtex?.IsVisible,
                Name = produto.NomeProduto,
                Description = objVtex?.Description,
                Title = objVtex?.Title,
                //ListStoreId = new[] { 1 }, Manter comentado para manter as politicas comerciais configuradas pelo sistema VTEX
                MetaTagDescription = objVtex?.MetaTagDescription,
                LinkId = ConverterTextLink(produto.NomeProduto) + $"-{produto.CodigoProduto}",

            };

            if (objVtex == null) return produtoVtex;

            produtoVtex.Id = objVtex.Id;
            produtoVtex.LinkId = objVtex.LinkId;

            return produtoVtex;
        }

        private static int RetornarCategoriaProduto(CategoriasDoSiteViewModel categorias)
        {
            var ultimo = categorias.Linhas.Count - 1;
            return categorias.Linhas[ultimo].CodigoCategoria;
        }

        private static int RetornarDepartamentoProduto(CategoriasDoSiteViewModel categorias)
        {
            return categorias.Linhas[0].CodigoCategoria;
        }

        private static string ConverterTextLink(string texto)
        {
            var regex = new Regex("[^a-zA-Z0-9] ");
            var textoFormatado = texto.RemoverDiacritics().RemoverQuebrasLinha();
            return regex.Replace(textoFormatado, "").SubstituirCaracteres(" ", '-').ToLower();
        }

        public static decimal CalcularPesoCubico(double comprimento, double largura, double altura)
        {
            const int fator = 6000;
            return (decimal)(comprimento * largura * altura) / fator;
        }

        private static decimal ConverterPeso(double peso)
        {
            return peso < 0
                ? (decimal)peso * -1000
                : (decimal)peso * 1000;
        }
    }
}
