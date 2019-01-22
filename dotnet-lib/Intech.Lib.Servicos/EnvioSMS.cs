using HumanAPIClient.Model;
using HumanAPIClient.Service;
using Intech.PrevSystem.Entidades;
using Intech.PrevSystem.Negocio.Proxy;
using System;
using System.Linq;

namespace Intech.Lib.Servicos
{
    public class EnvioSMS
    {
        private string ddi = "55";

        public string EnviarHumanAPI(string numTelefone, string conta, string senha, string Remetente, string Mensagem, string matricula, string inscricao, bool gravarLog = false)
        {
            string retorno = string.Empty;
            var sms = new SimpleSending(conta, senha);
            var message = new SimpleMessage();

            if (numTelefone.IndexOf('0') == 0)
                numTelefone = numTelefone.Substring(1);

            if (numTelefone.IndexOf("55") <= -1)
                numTelefone = "55" + numTelefone;

            message.To = numTelefone;
            message.From = Remetente;
            message.Message = Mensagem;

            retorno = sms.send(message).LastOrDefault();

            if (gravarLog)
                gravarLogSMS(retorno, numTelefone, matricula, inscricao);

            return retorno;
        }

        private static void gravarLogSMS(string retorno, string numTelefone, string matricula, string inscricao)
        {
            try
            {
                var logSMSProxy = new LogSMSProxy();
                logSMSProxy.Inserir(new LogSMSEntidade {
                    RESPOSTA_ENVIO = retorno,
                    NUM_TELEFONE = numTelefone,
                    NUM_MATRICULA = matricula,
                    NUM_INSCRICAO = inscricao,
                    DTA_ENVIO = DateTime.Today
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu erro ao gravar log de sms: Message: {ex.Message}, e StackTrace: {ex.StackTrace}");
            }
        }
    }
}