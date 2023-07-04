using PainelGerencial.Controller;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using PainelGerencial.Repository;
using System.Linq;

namespace PainelGerencial.Services
{
    public class AuditoriaProdutosServicos : IAuditoriaProdutosServicos
    {
        private readonly IAuditoriaProdutosRepository _auditoriaProdutosRepository;
        private readonly HubController _hubController;

        public AuditoriaProdutosServicos(IAuditoriaProdutosRepository auditoriaProdutosRepository, HubController hubController)
        {
            _auditoriaProdutosRepository = auditoriaProdutosRepository;
            _hubController = hubController;
        }

        public async Task<bool> Corrigido(int idProduto, string dk)
        {
            var dados = _auditoriaProdutosRepository.BuscaProdutoCorrigido(idProduto, dk).ToList();

            if (!dados.Any())
            {
                return false;
            }

            string nome = dados[0].nome;
            double largura = dados[0].largura;
            double comprimento = dados[0].comprimento;
            double espessura = dados[0].espessura;
            double peso = dados[0].peso;

            if (comprimento > 100)
            {
                (espessura, comprimento) = (comprimento, espessura);
            }

            var condIn = new List<string>();

            var sql = _auditoriaProdutosRepository.VerificaKit(dk);

            if (!sql.Any())
            {
                condIn.Add(dk);
            }
            else
            {
                foreach (var i in sql)
                {
                    condIn.Add(i.dk_kit);
                }
            }

            sql = _auditoriaProdutosRepository.VerificaFilhosKit(dk);
            sql.ToList().ForEach(item =>
            {
                condIn.Add(item.dk_kit);
            });


            bool atualizado = _auditoriaProdutosRepository.CorrigidoAtualizaProser(largura, comprimento, espessura, peso, condIn);

            atualizado = _auditoriaProdutosRepository.CorrigidoAtualizaProducts(dk, idProduto);

            var retornoVtex = await _hubController.AjusteVtex(dk, nome);

            Dictionary<string, object> data = new() {
                { "dk", dk },
                { "nome", nome },
                { "data", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                { "retorno", retornoVtex}
            };

            bool inseriu = _auditoriaProdutosRepository.InsereHistoricoVtex(data);

            return inseriu;
        }
    }
}
