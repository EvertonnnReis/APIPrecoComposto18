

<html>

    <head><link rel="alternate" type="text/xml" href="/AbacosWSERP.asmx?disco" />

    <style type="text/css">
    
		BODY { color: #000000; background-color: white; font-family: Verdana; margin-left: 0px; margin-top: 0px; }
		#content { margin-left: 30px; font-size: .70em; padding-bottom: 2em; }
		A:link { color: #336699; font-weight: bold; text-decoration: underline; }
		A:visited { color: #6699cc; font-weight: bold; text-decoration: underline; }
		A:active { color: #336699; font-weight: bold; text-decoration: underline; }
		A:hover { color: cc3300; font-weight: bold; text-decoration: underline; }
		P { color: #000000; margin-top: 0px; margin-bottom: 12px; font-family: Verdana; }
		pre { background-color: #e5e5cc; padding: 5px; font-family: Courier New; font-size: x-small; margin-top: -5px; border: 1px #f0f0e0 solid; }
		td { color: #000000; font-family: Verdana; font-size: .7em; }
		h2 { font-size: 1.5em; font-weight: bold; margin-top: 25px; margin-bottom: 10px; border-top: 1px solid #003366; margin-left: -15px; color: #003366; }
		h3 { font-size: 1.1em; color: #000000; margin-left: -15px; margin-top: 10px; margin-bottom: 10px; }
		ul { margin-top: 10px; margin-left: 20px; }
		ol { margin-top: 10px; margin-left: 20px; }
		li { margin-top: 10px; color: #000000; }
		font.value { color: darkblue; font: bold; }
		font.key { color: darkgreen; font: bold; }
		font.error { color: darkred; font: bold; }
		.heading1 { color: #ffffff; font-family: Tahoma; font-size: 26px; font-weight: normal; background-color: #003366; margin-top: 0px; margin-bottom: 0px; margin-left: -30px; padding-top: 10px; padding-bottom: 3px; padding-left: 15px; width: 105%; }
		.button { background-color: #dcdcdc; font-family: Verdana; font-size: 1em; border-top: #cccccc 1px solid; border-bottom: #666666 1px solid; border-left: #cccccc 1px solid; border-right: #666666 1px solid; }
		.frmheader { color: #000000; background: #dcdcdc; font-family: Verdana; font-size: .7em; font-weight: normal; border-bottom: 1px solid #dcdcdc; padding-top: 2px; padding-bottom: 2px; }
		.frmtext { font-family: Verdana; font-size: .7em; margin-top: 8px; margin-bottom: 0px; margin-left: 32px; }
		.frmInput { font-family: Verdana; font-size: 1em; }
		.intro { margin-left: -15px; }
           
    </style>

    <title>
	AbacosWSERP Web Service
</title></head>

  <body>

    <div id="content">

      <p class="heading1">AbacosWSERP</p><br>

      <span>
          <p class="intro">Este WebService destina-se a integração entre sistemas ERP e o ÁBACOS</p>
      </span>

      <span>

          <p class="intro">The following operations are supported.  For a formal definition, please review the <a href="AbacosWSERP.asmx?WSDL">Service Description</a>. </p>
          
          
              <ul>
            
              <li>
                <a href="AbacosWSERP.asmx?op=AlterarPrecoComponenteKit">AlterarPrecoComponenteKit</a>
                
                <span>
                  <br>Permite a atualização dos preços dos produtos do KIT diretamente no sistema ÁBACOS.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=AtualizarPreco">AtualizarPreco</a>
                
                <span>
                  <br>Permite a atualização dos preços dos produtos diretamente no sistema ÁBACOS.
                                   Podem ser enviados vários itens para atualização dos preços de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=AtualizarProduto">AtualizarProduto</a>
                
                <span>
                  <br>Permite alterar os dados dos produtos diretamente no sistema ÁBACOS. Podem ser enviados vários produtos de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=AtualizarSaldoProduto">AtualizarSaldoProduto</a>
                
                <span>
                  <br>Permite a atualização dos saldos de estoque dos produtos diretamente no sistema ÁBACOS.
                                   Podem ser enviados vários dados de saldo de estoque do produto de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarCodigosProduto">CadastrarCodigosProduto</a>
                
                <span>
                  <br>Permite o cadastramento dos códigos de origem do produto diretamente no sistema ÁBACOS.
                                   Podem ser enviados vários códigos para o produto de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarComponenteProdutoKIT">CadastrarComponenteProdutoKIT</a>
                
                <span>
                  <br>Permite a inclusão dos componentes dos produtos KIT diretamente no sistema ÁBACOS. Podem ser enviados vários componentes de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarComposicaoProduto">CadastrarComposicaoProduto</a>
                
                <span>
                  <br>Permite o cadastramento de composição de preço diretamente no sistema ÁBACOS.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarFormaPagamento">CadastrarFormaPagamento</a>
                
                <span>
                  <br>Permite o cadastramento de formas de pagamento diretamente no sistema ÁBACOS. Podem ser enviados várias formas de pagamento de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarFornecedor">CadastrarFornecedor</a>
                
                <span>
                  <br>Permite a inclusão dos fornecedores diretamente no sistema ÁBACOS. Podem ser enviados vários fornecedores de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarGrupoClassificacaoComplementar">CadastrarGrupoClassificacaoComplementar</a>
                
                <span>
                  <br>Permite o cadastramento de grupos de classificação complementar.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarMarca">CadastrarMarca</a>
                
                <span>
                  <br>Permite o cadastramento das marcas do produto. Podem ser enviados várias descrições de marca de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarNCM">CadastrarNCM</a>
                
                <span>
                  <br>Permite o cadastramento do NCM.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarOpcaoClassificacaoComplementar">CadastrarOpcaoClassificacaoComplementar</a>
                
                <span>
                  <br>Permite o cadastramento de opções de classificação complementar.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarProduto">CadastrarProduto</a>
                
                <span>
                  <br>Permite o cadastramento dos produtos diretamente no sistema ÁBACOS. Podem ser enviados vários produtos de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarProdutoCategoriasSite">CadastrarProdutoCategoriasSite</a>
                
                <span>
                  <br>Permite a atualização das Categorias Site associadas ao produtos diretamente no sistema ÁBACOS.
                                   Podem ser enviados vários produtos de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=CadastrarRepresentante">CadastrarRepresentante</a>
                
                <span>
                  <br>Permite o cadastramento de representantes diretamente no sistema ÁBACOS. Podem ser enviados vários representantes de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ClientesFornecedoresDisponiveis">ClientesFornecedoresDisponiveis</a>
                
                <span>
                  <br>Deve ser utilizado para obter as informações de Clientes e fornecedores do  que estão disponíveis para a integração.
                                   A informação se torna disponível para integração quando ocorrer a conclusão de um Cliente no módulo de loja do ÁBACOS.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConfirmarRecebimentoClientesFornecedores">ConfirmarRecebimentoClientesFornecedores</a>
                
                <span>
                  <br>Quando um Cliente/Fornecedor do  se torna disponível para integração ele se torna um item de uma lista até que seja retirado da mesma.
                                   Para retirá-lo da lista é necessário informar que a sua integração foi bem sucedida. Este método deve ser usado para isso. Caso a confirmação
                                   não ocorra o mesmo irá permanecer na lista indefinidamente.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConfirmarRecebimentoMovimentoEstoque">ConfirmarRecebimentoMovimentoEstoque</a>
                
                <span>
                  <br>Quando um movimento de estoque se torna disponível para integração ele se torna um item de uma lista até que seja retirado da mesma.
                                   Para retirá-lo da lista é necessário informar que a sua integração foi bem sucedida. Este método deve ser usado para isso. Caso a confirmação
                                   não ocorra o mesmo irá permanecer na lista indefinidamente.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConfirmarRecebimentoNotaFiscal">ConfirmarRecebimentoNotaFiscal</a>
                
                <span>
                  <br>Este método deve ser usado para Marcar como lida uma respectiva Nota Fiscal
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConfirmarRecebimentoPedido">ConfirmarRecebimentoPedido</a>
                
                <span>
                  <br>Quando um pedido se torna disponível para integração ele se torna um item de uma lista até que seja retirado da mesma.
                                   Para retirá-lo da lista é necessário informar que a sua integração foi bem sucedida. Este método deve ser usado para isso.
                                   Caso a confirmação não ocorra a mesma irá permanecer na lista indefinidamente.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConfirmarRecebimentoPedidoCompra">ConfirmarRecebimentoPedidoCompra</a>
                
                <span>
                  <br>Quando um pedido se torna disponível para integração ele se torna um item de uma lista até que seja retirado da mesma.
                                   Para retirá-lo da lista é necessário informar que a sua integração foi bem sucedida. Este método deve ser usado para isso. Caso a confirmação
                                   não ocorra a mesma irá permanecer na lista indefinidamente.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConfirmarRecebimentoStatusPedido">ConfirmarRecebimentoStatusPedido</a>
                
                <span>
                  <br>Quando um status de pedido se torna disponível para integração ele se torna um item de uma lista até que seja retirada da mesma.
                                   Para retirá-lo da lista é necessário informar que a sua integração foi bem sucedida. Este método deve ser usado para isso. Caso a confirmação não ocorra o mesmo irá permanecer na lista indefinidamente.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConfirmarRecebimentoStatusTraking">ConfirmarRecebimentoStatusTraking</a>
                
                <span>
                  <br>Confirmar a integração do status do pedido durante o traking
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConfirmarRecebimentoTrocaDevolucao">ConfirmarRecebimentoTrocaDevolucao</a>
                
                <span>
                  <br>Quando um processo de Troca / Devolução fica disponível para integração ele se torna um item de uma lista até que seja retirado da mesma.
                                   Para retirá-lo da lista é necessário informar que a sua integração foi bem sucedida. Este método deve ser usado para isso. Caso a confirmação
                                   não ocorra o mesmo irá permanecer na lista indefinidamente.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ConsultaComposicaoProduto">ConsultaComposicaoProduto</a>
                
                <span>
                  <br>Permite a consulta da composição de preço diretamente no sistema ÁBACOS.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=GetVersion">GetVersion</a>
                
                <span>
                  <br>Consultar a versão do webservice e do Ábacos
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ImportarNotaFiscalSimplesRemessa">ImportarNotaFiscalSimplesRemessa</a>
                
                <span>
                  <br>Importar Nota Fiscal de Simples Remessa de Entrada ou Saída
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=InserirPedidoCompra">InserirPedidoCompra</a>
                
                <span>
                  <br>Permite a inclusão dos pedidos de compra diretamente no sistema ÁBACOS. Podem ser enviados vários pedidos de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ManutencaoProduto">ManutencaoProduto</a>
                
                <span>
                  <br>Cadastrar ou Alterar produto principal no ábacos. Métodos com campos mais direto e simplificado
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=MovimentarEstoque">MovimentarEstoque</a>
                
                <span>
                  <br>Permite a movimentação de estoque dos produtos diretamente no sistema ÁBACOS,
                                   ou seja, os lançamentos das entrada e saídas de estoques dos produtos.
                                   Podem ser enviados vários dados de movimentação de estoque do produto de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=MovimentoEstoqueDisponiveis">MovimentoEstoqueDisponiveis</a>
                
                <span>
                  <br>Deve ser utilizado para obter as informações de movimentos de estoque que estão disponíveis para a integração.
                                   A informação se torna disponível para integração quando ocorre uma inclusão de algum lançamento.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=NotasFiscaisDisponiveis">NotasFiscaisDisponiveis</a>
                
                <span>
                  <br>Este método deve ser usado para Listar Todas as Notas Ficais Disponiveis para Integração.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=PedidoComprasDisponiveis">PedidoComprasDisponiveis</a>
                
                <span>
                  <br>Deve ser utilizado para obter as informações de pedidos que estão disponíveis para a integração.
                                   A informação se torna disponível para integração quando ocorre uma inclusão, alteração de alguma informação ou exclusão.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=PedidosDisponiveis">PedidosDisponiveis</a>
                
                <span>
                  <br>Deve ser utilizado para obter as informações de pedidos que estão disponíveis para a integração.
                                   A informação se torna disponível para integração quando ocorre uma inclusão, alteração de alguma informação ou exclusão.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ProdutoAtivaDesativa">ProdutoAtivaDesativa</a>
                
                <span>
                  <br>Permite a ativação / desativação dos produtos diretamente no sistema ÁBACOS.
                                   Podem ser enviados vários produtos para ativação / desativação de uma única vez se assim for desejado.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ProdutoExiste">ProdutoExiste</a>
                
                <span>
                  <br>Este método deve ser usado para identificar se um produto existe na base de dados do ÁBACOS.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=RegistrarErroFaturamentoPedido">RegistrarErroFaturamentoPedido</a>
                
                <span>
                  <br>Confirmar o erro do faturamento do pedido
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=RegistrarSucessoFaturamentoPedido">RegistrarSucessoFaturamentoPedido</a>
                
                <span>
                  <br>Confirmar o sucesso do faturamento do pedido
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=ReservarEstoque">ReservarEstoque</a>
                
                <span>
                  <br>Permite reservar o estoque da lista de pedidos
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=StatusPedidoDisponiveis">StatusPedidoDisponiveis</a>
                
                <span>
                  <br>Deve ser utilizado para obter as informações de status de pedidos que estão disponíveis para a integração.
                                   A informação se torna disponível para integração quando ocorre alguma alteração no status do pedido.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=StatusPedidoTraking">StatusPedidoTraking</a>
                
                <span>
                  <br>Permite consultar o(s) status do(s) pedido(s) durante o traking.
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=TrocaDevolucoes">TrocaDevolucoes</a>
                
                <span>
                  <br>Este método deve ser usado para trazer dados de troca e devolucoes
                </span>
              </li>
              <p>
            
              <li>
                <a href="AbacosWSERP.asmx?op=TrocaDevolucoesNovo">TrocaDevolucoesNovo</a>
                
                <span>
                  <br>Este método deve ser usado para trazer dados de troca e devolucoes - Novo Método
                </span>
              </li>
              <p>
            
              </ul>
            
      </span>

      
      

    <span>
        
    </span>
    
      

      

    
  </body>
</html>
