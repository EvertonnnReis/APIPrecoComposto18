using AbacosWSPlataforma;
using System;
using static AbacosWSPlataforma.AbacosWSPlataformaSoapClient;

namespace Repository
{
    public class WsAbacos : IWsAbacos
    {
        public AbacosWSPlataformaSoapClient AbacosWs()
        {
            var wsAbacos = new AbacosWSPlataformaSoapClient(EndpointConfiguration.AbacosWSPlataformaSoap12);
            wsAbacos.InnerChannel.OperationTimeout = new TimeSpan(0, 5, 0);
            return wsAbacos;
        }
    }
}
