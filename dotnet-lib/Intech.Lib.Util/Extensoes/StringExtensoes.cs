using System;
using System.Linq;
using System.Text;

namespace System
{
    public static class StringExtensoes
    {
        public static string AplicarMascara(this string str, string mascara)
        {
            var cont = 0;

            foreach (char item in mascara)
            {
                if (item == '9')
                    cont++;
            }

            if (str.Length == cont)
            {
                var valorFormatado = new StringBuilder(mascara);
                var indiceValor = 0;

                for (int i = 0; i < mascara.Length; i++)
                {
                    if (valorFormatado[i] == '9')
                        valorFormatado[i] = str[indiceValor++];
                }

                return valorFormatado.ToString();
            }
            else
            {
                throw new Exception("A quantidade de caracteres do VALOR é invalido para a máscara, ou a máscara não é formada por '9' e seus caracteres especiais.");
            }
        }

        /// <summary>
        /// Retorna valor sem máscara. Se o parâmetro possuir apenas os caracteres especiais de máscara,
        /// o método retorna uma string vazia.
        /// </summary>
        /// <param name="valor">Valor mascarado.</param>
        /// <returns>Valor sem máscara.</returns>
        public static string LimparMascara(this string valor)
        {
            if (String.IsNullOrEmpty(valor))
                return valor;

            var sb = new StringBuilder(valor.Length);

            foreach (char c in valor.Where(Char.IsDigit))
            {
                sb.Append(c);
            }

            return sb.ToString();
        }
    }

    public static class Mascaras
    {
        /// <summary>
        /// Mascara do CPF
        /// </summary>
        public static string CPF = "999.999.999-99";

        /// <summary>
        /// Mascara do CNPJ
        /// </summary>
        public static string CNPJ = "99.999.999/9999-99";
        /// <summary>
        /// Mascara do CEP
        /// </summary>
        public static string CEP = "99999-999";

        /// <summary>
        /// Mascara do Telefone
        /// </summary>
        public static string TELEFONE = "9999-9999";

        /// <summary>
        /// Mascara do Telefone
        /// </summary>
        public static string DDD_TELEFONE = "(99) 9999-9999";
    }
}
