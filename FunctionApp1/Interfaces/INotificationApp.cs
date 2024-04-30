using FCGuiaProducaoNotificacao.Model;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FCGuiaProducaoNotificacao.Interfaces
{
    public interface INotificationApp
    {
        [Post("/app-guia")]
        Task<NotificationResponse> PostNotifications([Header("Authorization")] string token, NotificationConfig config);
    }
}
