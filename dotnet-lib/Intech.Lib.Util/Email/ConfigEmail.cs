namespace Intech.Lib.Util.Email
{
    public class ConfigEmail
    {
        public string EmailRemetente { get; set; }
        public string EmailRelacionamento { get; set; }
        public string NomeExibicao { get; set; }
        public string EnderecoSMTP { get; set; }
        public int Porta { get; set; }
        public bool RequerAutenticacao { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public bool RequerSSL { get; set; }
        public bool AutenticacaoUsaDominio { get; set; }
        public bool DesprezarCertificado { get; set; }
    }
}
