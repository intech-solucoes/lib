#region Usings
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endregion

namespace Intech.Lib.Data.Erros
{
    public class ErrosBanco
    {
        #region Códigos

        public static int ChaveDuplicada = 2601;

        #endregion

        public static string NomeArquivo = "errosBanco.json";

        public static ErrosBanco Current { get; private set; }

        public ErrosBancoItens UK { get; set; }

        public static void Popular()
        {
            var file = File.ReadAllText(NomeArquivo);
            Current = JsonConvert.DeserializeObject<ErrosBanco>(file);
        }

        public static string Traduzir(string erro, int numeroErro)
        {
            var splitErro = erro.Split(' ');

            if (numeroErro == ChaveDuplicada)
            {
                var nomeUK = splitErro.First(x => x.Contains("_UK")).Replace("'", "").Replace(".", "");
                var valor = Current.UK.Itens.Single(x => x.Key == nomeUK).Value;

                return string.Format(Current.UK.MensagemBase, valor);
            }

            return null;
        }
    }

    public class ErrosBancoItens
    {
        public string MensagemBase { get; set; }
        public List<KeyValuePair<string, string>> Itens { get; set; }
    }
}