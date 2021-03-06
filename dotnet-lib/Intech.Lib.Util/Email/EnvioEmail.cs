﻿#region Usings
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
#endregion

namespace Intech.Lib.Util.Email
{
    public static class EnvioEmail
    {
        private static Encoding EncodingEmail = Encoding.GetEncoding("ISO-8859-1");

        public static void Enviar(ConfigEmail config, string para, string assunto, string corpo) =>
            Enviar(config, para, null, null, assunto, corpo, null, null);

        public static void Enviar(ConfigEmail config, string para, string cc, string assunto, string corpo) => 
            Enviar(config, para, cc, null, assunto, corpo, null, null);

        public static void Enviar(ConfigEmail config, string para, string cc, string cco, string assunto, string corpo) =>
            Enviar(config, para, cc, cco, assunto, corpo, null, null);

        public static void Enviar(ConfigEmail config, string para, string cc, string cco, string assunto, string corpo, Stream anexo, string tituloAnexo, bool copiaParaRemetente = false)
        {
            var email = new MailMessage
            {
                From = new MailAddress(config.EmailRemetente),
                Subject = assunto,
                SubjectEncoding = EncodingEmail,
                Body = MontarCorpo(corpo),
                BodyEncoding = EncodingEmail,
                IsBodyHtml = true
            };

            email.To.Add(para);

            // Adiciona CC's
            if(!string.IsNullOrEmpty(cc))
            {
                var ccList = cc.Split(';');

                foreach (var ccItem in ccList)
                    if (string.IsNullOrEmpty(ccItem.Trim()))
                        email.CC.Add(ccItem);
            }

            if (copiaParaRemetente)
                email.CC.Add(config.EmailRemetente);

            // Adiciona CCO's
            if(!string.IsNullOrEmpty(cco))
            {
                var ccoList = cco.Split(';');

                foreach (var ccoItem in ccoList)
                    if(string.IsNullOrEmpty(ccoItem.Trim()))
                        email.Bcc.Add(ccoItem.Trim());
            }

            // Adiciona anexos
            if (anexo != null)
                email.Attachments.Add(new Attachment(anexo, tituloAnexo));

            // Configura client
            var smtp = new SmtpClient
            {
                Port = config.Porta,
                Host = config.EnderecoSMTP,
                Timeout = 50000,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if(config.RequerSSL)
            {
                smtp.EnableSsl = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            }

            if (config.RequerAutenticacao)
            {
                smtp.UseDefaultCredentials = false;
                if (config.AutenticacaoUsaDominio)
                    smtp.Credentials = new NetworkCredential(config.Usuario, config.Senha, config.EnderecoSMTP);
                else
                    smtp.Credentials = new NetworkCredential(config.Usuario, config.Senha);
            }
            else
            {
                smtp.UseDefaultCredentials = true;
            }

            if(config.DesprezarCertificado)
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            smtp.Send(email);
        }

        private static string MontarCorpo(string conteudo)
        {
            StringBuilder corpo = new StringBuilder();

            corpo.Append("<html>");
            corpo.Append("<body>");
            corpo.Append(conteudo);
            corpo.Append("</body>");
            corpo.Append("</html>");

            return corpo.ToString();
        }

        public static void EnviarMailKit(ConfigEmail config, string destinatario, string assunto, string corpo)
        {
            var listaDestinatarios = new List<string> { destinatario };
            EnviarMailKit(config, listaDestinatarios, assunto, corpo);
        }

        public static void EnviarMailKit(ConfigEmail config, List<string> listaDestinatarios, string assunto, string corpo)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(config.EmailRemetente));

            foreach(var destinatario in listaDestinatarios)
                message.To.Add(new MailboxAddress(destinatario));

            message.Subject = assunto;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = corpo
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                if (config.DesprezarCertificado)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(config.EnderecoSMTP, config.Porta, MailKit.Security.SecureSocketOptions.Auto);

                // Note: only needed if the SMTP server requires authentication
                if(config.RequerAutenticacao)
                    client.Authenticate(config.Usuario, config.Senha);

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
