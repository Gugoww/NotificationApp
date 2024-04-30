using Grpc.Core;
using Newtonsoft.Json;

namespace FCGuiaProducaoNotificacao.Model
{
    public class NotificationResponse
    {

        public int modCodigo { get; set; }

        public string status  { get; set; }

        public string mensagem { get; set; }
    }
}
