using AbacosWSERP;
using Google.Protobuf.WellKnownTypes;
using PainelGerencial.Controller;
using PainelGerencial.Domain;
using PainelGerencial.Domain.Fenix;
using PainelGerencial.Domain.PrecificacaoCadastro;
using PainelGerencial.Repository;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Services
{
    public class SolicitacaoTemporariaServicos : ISolicitacaoTemporariaServicos
    {
        private readonly ControleAcessoController _controleAcesso;
        private readonly IFenixServicos _fenixServicos;
        private readonly IPrecificacaoCadastroRepository _precificacaoRepository;

        public SolicitacaoTemporariaServicos(
            ControleAcessoController controleAcesso, 
            IFenixServicos fenixServicos,
            IPrecificacaoCadastroRepository precificacao)
        {
            _controleAcesso = controleAcesso;
            _fenixServicos = fenixServicos;
            _precificacaoRepository = precificacao;
        }
        
        double ContarComponentes(string ProdutoCodigoExterno)
        {
            double quantidade = 0;
            var componentes = (ProdutoKitComComponentesViewModel)
                _fenixServicos.BuscarProdutoKit(2, ProdutoCodigoExterno);

            foreach (ProdutoKitComponenteViewModel componente in componentes.Componentes.GroupBy(x => x.CodigoExterno).Select(y => y.First()).ToList())
            {
                quantidade = quantidade + componente.Quantidade;
            };
            return quantidade;
        }

        public async void SalvarAlteracaoTemporaria(SolicitacaoTemporaria_Model solicitacao)
        {
            await _precificacaoRepository.AtualizarSolicitacaoTemporaria(solicitacao);
        }

        public async Task<bool> VerificaPrecoZerado(string email)
        {
            var solicitacoesTemporarias = await _precificacaoRepository.ListaSolicitacoesTemporarias(email);

            return solicitacoesTemporarias.Any(item => item.ValorDefinido == 0);
        }

        public async void ConfirmarSolicitacoesTemporarias(string email, int aplicarPrecoVendavel)
        {
            var solicitacoesTemporarias = await _precificacaoRepository.ListaSolicitacoesTemporarias(email);
            
            var agrupamentoPorPai = solicitacoesTemporarias.GroupBy(x => new { x.ProdutoCodigoExternoPai, x.TabelaPreco });

            //filhos kits e não kits
            foreach (var kitPaiGrupo in agrupamentoPorPai)
            {
                var kitPai = kitPaiGrupo.FirstOrDefault(x => x.ProdutoPai) ?? kitPaiGrupo.FirstOrDefault();
                if (kitPai.TipoKit == 1)
                {

                    foreach (var kit in kitPaiGrupo.Where(x => x.Kit))
                    {
                        double quantidade = 0;
                        if (kit.Kit)
                        {
                            quantidade = ContarComponentes(kit.ProdutoCodigoExterno);
                        }
                        if (kitPai.ValorDefinido > 0)
                        {
                            kit.ValorDefinido = kitPai.ValorDefinido * quantidade;
                            var margem = kit.ValorDefinido > 0 ? (kit.ValorDefinido - kit.ProdutoCusto) / kit.ValorDefinido * 100 : 0;

                            kit.PercentualAjuste = Math.Round(margem, 2);

                            await _precificacaoRepository.AtualizarSolicitacaoTemporaria(kit);
                        }
                    }
                }
                //filhos somente kits
                else if (kitPai.TipoKit == 2)
                {
                    foreach (var kit in kitPaiGrupo.Where(x => x.Kit))
                    {
                        {
                            if (kitPai.ValorDefinido > 0)
                            {
                                kit.ValorDefinido = kitPai.ValorDefinido;
                                var margem = kit.ValorDefinido > 0 ? (kit.ValorDefinido - kit.ProdutoCusto) / kit.ValorDefinido * 100 : 0;
                                kit.PercentualAjuste = Math.Round(margem, 2);

                            }
                            await _precificacaoRepository.AtualizarSolicitacaoTemporaria(kit);
                        }
                    }
                }
                //filho
                else if (kitPai.TipoKit == 4)
                {
                    //tratar pai com kits
                    foreach (var kit in kitPaiGrupo.Where(x => x.TipoKit == 1))
                    {
                        double quantidade = 0;

                        if (kitPai.ValorDefinido > 0)
                        {
                            quantidade = ContarComponentes(kit.ProdutoCodigoExterno);
                            kit.ValorDefinido = kitPai.ValorDefinido * quantidade;

                            var margem = kit.ValorDefinido > 0 ? (kit.ValorDefinido - kit.ProdutoCusto) / kit.ValorDefinido * 100 : 0;

                            kit.PercentualAjuste = Math.Round(margem, 2);

                            await _precificacaoRepository.AtualizarSolicitacaoTemporaria(kit);
                        }
                    }
                }

            }

            var pais = solicitacoesTemporarias.Where(x => x.ProdutoPai && x.ValorDefinido > 0);

            foreach (var pai in pais)
            {
                // Configuração
                var reajustes = await _precificacaoRepository.PercentualAjuste(pai.TabelaPreco);

                foreach (var reajuste in reajustes)
                {
                    double valorDefinido = 0;
                    valorDefinido = pai.ValorDefinido;

                    if (aplicarPrecoVendavel == 1)
                    {
                        if (reajuste.Id != 2)
                        {
                            valorDefinido = pai.ValorDefinido * (1 + (double)reajuste.PercentualAjuste / 100);
                            valorDefinido = this.PrecoVenda(valorDefinido);
                        }
                    }

                    var solicitacaoPai = new Solicitacao_Model
                    {
                        SolicitacaoTipoCodigo = 3,
                        ProdutoCodigoExternoPai = pai.ProdutoCodigoExternoPai,
                        ProdutoCodigoExterno = pai.ProdutoCodigoExterno,
                        ProdutoNome = pai.ProdutoNome,
                        ProdutoPrecoTabelaNovo = pai.ValorDefinido,
                        ProdutoPrecoPromocionalNovo = valorDefinido,
                        ProdutoPrecoPromocional = pai.ValorPor,
                        ProdutoPrecoTabela = pai.ValorDe,
                        ProdutoListaPrecoCodigo = reajuste.Id,
                        SolicitanteResponsavelEmail = pai.Email,
                        SolicitacaoStatusCodigo = 1,
                        ProdutoGrupoCodigo = pai.Grupo,
                        Observacao = pai.Observacao != null ? ObterMensagemObservacao((int)pai.Observacao) : ""
                    };
                    int atualizado = await _precificacaoRepository.GerarSolicitacoes(solicitacaoPai);
                }
                

            }
            
            this.GerarSolicitacoesKits(solicitacoesTemporarias.Where(x => x.Kit && x.ValorDefinido > 0).Distinct()?.ToList());

            await _precificacaoRepository.CancelarERemoverPorUsuario(email);
        }

        public static string ObterMensagemObservacao(int obs)
        {
            switch (obs)
            {
                case 1: return "Custo esta zerado por se tratar de uma nova compra.";
                case 2: return "Custo esta zerado no Abacos, pode se tratar de uma bonificação.";
                case 3: return "Exorbitante";
                case 4: return "Foi precificado recentemente";
                default:
                    return "";
            }
        }
        public double PrecoVenda(double valor)
        {
            if ((valor % 5) <= 2.5)
            {
                return valor - (valor % 5) - 0.10;
            } 
            else
            {
                return valor - (valor % 5) - 0.10 + 5;
            }
        }

        public async void GerarSolicitacoesKits(List<SolicitacaoTemporaria_Model> solicitacoes)
        {
            foreach (var objSolicitacao in solicitacoes)
            {
                // Configuração
                var reajustes = await _precificacaoRepository.PercentualAjuste(objSolicitacao.TabelaPreco);

                foreach (var reajuste in reajustes)
                {
                    var obs = "";
                    if (objSolicitacao.PercentualAjuste < 10)
                    {
                        obs = $"Margem é de {objSolicitacao.PercentualAjuste}%! ";
                    }
                    if (objSolicitacao.Observacao != null)
                    {
                        obs = obs != "" ? obs + " | " + ObterMensagemObservacao((int)objSolicitacao.Observacao) : ObterMensagemObservacao((int)objSolicitacao.Observacao);
                    }

                    double valorDefinido = 0;
                    valorDefinido = objSolicitacao.ValorDefinido * (1 + (double)reajuste.PercentualAjuste / 100);
                    valorDefinido = this.PrecoVenda(valorDefinido);

                    var solicitacaoRecebida = new Solicitacao_Model
                    {
                        SolicitacaoTipoCodigo = 5,
                        SolicitanteResponsavelEmail = objSolicitacao.Email,
                        ProdutoCodigoExterno = objSolicitacao.ProdutoCodigoExterno,
                        ProdutoGrupoCodigo = Convert.ToInt32(objSolicitacao.Grupo),
                        ProdutoNome = objSolicitacao.ProdutoNome,
                        ProdutoPrecoTabela = objSolicitacao.ValorDe,
                        ProdutoListaPrecoCodigo = reajuste.Id,
                        ProdutoPrecoPromocional = objSolicitacao.ValorPor,
                        ProdutoPrecoPromocionalNovo = valorDefinido,
                        ProdutoPrecoTabelaNovo = Convert.ToDouble(objSolicitacao.ValorDefinido),
                        Observacao = obs,
                        SolicitacaoStatusCodigo = 1,
                        ProdutoCodigoExternoPai = objSolicitacao.ProdutoCodigoExternoPai,
                        ProdutoNomePai = objSolicitacao.ProdutoNomePai
                    };

                    int atualizado = await _precificacaoRepository.GerarSolicitacoes(solicitacaoRecebida);
                    //await _precificacaoRepository.GerarSolicitacoesKits(solicitacaoRecebida);
                }   
            }
        }
    }
}
