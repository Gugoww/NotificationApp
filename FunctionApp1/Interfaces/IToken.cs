using FCGuiaProducaoNotificacao.Model;
using Refit;

namespace FCGuiaProducaoNotificacao.Interfaces
{
    public interface Itoken
    {
        [Post("/oauth/v2/access-token")]
        [Headers("Content-Type application/json")]

        Task<Token> getToken(TokenBody body, [Header("Authorization")] string auth);

    }
}
