using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;
using System.Text;
using FCGuiaProducaoNotificacao.Model;
using FCGuiaProducaoNotificacao.Interfaces;

namespace FCGuiaProducaoNotificacao
{
    public class PostNotification
    {
        private readonly ILogger _logger;
        private Itoken _token;
        private INotificationApp _NotificationApp;

        public PostNotification(ILoggerFactory loggerFactory, IConfiguration configuration)
        {

            _logger = loggerFactory.CreateLogger<PostNotification>();
            _token = RestService.For<Itoken>("https://portoapicloud-hml.portoseguro.com.br");
            _NotificationApp = RestService.For<INotificationApp>("https://portoapicloud-hml.portoseguro.com.br/producao/comercial/rastreios/v1");
        }

        [Function("PostNotification")]
        public async Task<IActionResult> FCPostNotification([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Está sendo processada a function PostNotification");

            try
            {
                _logger.LogInformation("Gerando parametros para gerar o token");
                var bodyToken = new TokenBody();
                var bodyNotification = new NotificationConfig();
                bodyToken.grant_type = "client_credentials";
                var auth = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("95762764-2de6-4ece-b596-994799a943f8:0b22815e-ed47-4ef6-b5ee-ed15f75d6e18"));

                _logger.LogInformation("Iniciando chamada para gerar o token");
                var resultToken = await _token.getToken(bodyToken, auth).ConfigureAwait(false);
                var count = 0;
                var authBearer = "Bearer " + resultToken.access_token;
                _logger.LogInformation("Token Gerado");
                _logger.LogInformation("Iniciando Leitura dos arquivos");
                var conectionStringBlob = @"DefaultEndpointsProtocol=https;AccountName=sttratativasdev01;AccountKey=qP3Bs7zEWiGKjiZaoQufHrn8ONmR2L3+DiVIUM0737UcY0pmtIfzphxYCplu+gEU1u7mDDIr76NRfRmGoxvFYw==;EndpointSuffix=core.windows.net";
                var containerName = "notificacao-guiaproducao";
                BlobContainerClient blobClientConteiner = new BlobContainerClient(conectionStringBlob, containerName);
                Pageable<BlobItem> blobs = blobClientConteiner.GetBlobs(BlobTraits.None, BlobStates.None, default, default);
                foreach (BlobItem blob in blobs)
                {
                    if (blob.Name.Contains(".txt"))
                    {
                        BlobClient blobClientTxt = blobClientConteiner.GetBlobClient(blob.Name);
                        MemoryStream blobStreamTXT = new MemoryStream();
                        blobClientTxt.DownloadTo(blobStreamTXT);
                        blobStreamTXT.Position = 0;
                        string STRTD = new StreamReader(blobStreamTXT).ReadToEnd();
                        _logger.LogInformation("Arquivo " + blob.Name + " (Leitura e Extração Concluida)");
                        bodyNotification.tituloDescricao = STRTD;
                        bodyNotification.modCodigo = 0;
                        NotificationResponse resultNotification = await _NotificationApp.PostNotifications(authBearer, bodyNotification).ConfigureAwait(false);
                        //rest pra gerar o mod e retorno id do mod
                        int responsemodcod = resultNotification.modCodigo;
                        var ArquivoCsv = blob.Name.Replace(".txt", ".csv");
                        BlobClient blobClientCsv = blobClientConteiner.GetBlobClient(ArquivoCsv);
                        MemoryStream blobStreamTCsv = new MemoryStream();
                        blobClientCsv.DownloadTo(blobStreamTCsv);
                        blobStreamTCsv.Position = 0;
                        _logger.LogInformation("Arquivo " + ArquivoCsv + " (Leitura e Extração Concluida)");
                        using (StreamReader sr = new StreamReader(blobStreamTCsv))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {

                                if (count == 0)
                                {
                                    bodyNotification.atributos = line;
                                    _logger.LogInformation("Preenchendo atributos da chamada");
                                    count++;
                                }
                                else
                                {
                                    _logger.LogInformation("Preenchendo atributos da chamada linha = " + count);
                                    bodyNotification.valores = line;
                                    count++;
                                    _logger.LogInformation("Inserindo notificação linha = " + count);
                                    //bodyNotification.modCodigo = responsemodcod;
                                    resultNotification = await _NotificationApp.PostNotifications(authBearer, bodyNotification).ConfigureAwait(false);
                                }

                            }
                        }
                        _logger.LogInformation("Notificação concluida para o arquivo " + blob.Name);
                        _logger.LogInformation("Apagando arquivos " + blob.Name + " e " + ArquivoCsv + " do Storage");
                        await blobClientTxt.DeleteAsync().ConfigureAwait(false);
                        await blobClientCsv.DeleteAsync().ConfigureAwait(false);
                        _logger.LogInformation("Arquivos Apagados");
                    }




                }
                return new OkObjectResult(bodyNotification);







            }
            catch (ApiException ex)
            {
                _logger.LogInformation("Deu erro a function PostNotification", ex);
                throw;
            }
        }
    }
}
