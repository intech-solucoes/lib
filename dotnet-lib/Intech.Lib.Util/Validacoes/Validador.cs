using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Intech.Lib.Util.Validacoes
{
    public class Validador
    {
        public static bool ValidarCNPJ(string cnpj)
        {
            int dig1 = 0;
            int dig2 = 0;
            cnpj = cnpj.LimparMascara();

            if (!string.IsNullOrEmpty(cnpj) && cnpj.Length == 14)
            {
                dig1 = ((int.Parse(cnpj[11].ToString()) * 2 +
                 int.Parse(cnpj[10].ToString()) * 3 +
                 int.Parse(cnpj[9].ToString()) * 4 +
                 int.Parse(cnpj[8].ToString()) * 5 +
                 int.Parse(cnpj[7].ToString()) * 6 +
                 int.Parse(cnpj[6].ToString()) * 7 +
                 int.Parse(cnpj[5].ToString()) * 8 +
                 int.Parse(cnpj[4].ToString()) * 9 +
                 int.Parse(cnpj[3].ToString()) * 2 +
                 int.Parse(cnpj[2].ToString()) * 3 +
                 int.Parse(cnpj[1].ToString()) * 4 +
                 int.Parse(cnpj[0].ToString()) * 5) * 10) % 11;
                if (dig1 == 10)
                {
                    dig1 = 0;
                }
                dig2 = ((int.Parse(cnpj[12].ToString()) * 2 +
                 int.Parse(cnpj[11].ToString()) * 3 +
                 int.Parse(cnpj[10].ToString()) * 4 +
                 int.Parse(cnpj[9].ToString()) * 5 +
                 int.Parse(cnpj[8].ToString()) * 6 +
                 int.Parse(cnpj[7].ToString()) * 7 +
                 int.Parse(cnpj[6].ToString()) * 8 +
                 int.Parse(cnpj[5].ToString()) * 9 +
                 int.Parse(cnpj[4].ToString()) * 2 +
                 int.Parse(cnpj[3].ToString()) * 3 +
                 int.Parse(cnpj[2].ToString()) * 4 +
                 int.Parse(cnpj[1].ToString()) * 5 +
                 int.Parse(cnpj[0].ToString()) * 6) * 10) % 11;

                if (dig2 == 10)
                {
                    dig2 = 0;
                }
                if (dig1.ToString() == cnpj[12].ToString() && dig2.ToString() == cnpj[13].ToString())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool ValidarCPF(string cpf)
        {
            int dig1 = 0;
            int dig2 = 0;
            int j = 2;
            cpf = cpf.LimparMascara();
            if (!string.IsNullOrEmpty(cpf) && cpf.Length == 11)
            {
                for (int i = cpf.Length - 3; i >= 0; i--)
                {
                    dig1 += int.Parse(cpf[i].ToString()) * j;
                    j++;
                }
                dig1 = 11 - (dig1 % 11);
                if (dig1 == 10 || dig1 == 11)
                {
                    dig1 = 0;
                }

                j = 2;

                for (int i = cpf.Length - 2; i >= 0; i--)
                {
                    dig2 += int.Parse(cpf[i].ToString()) * j;
                    j++;
                }
                dig2 = 11 - (dig2 % 11);
                if (dig2 == 10 || dig2 == 11)
                {
                    dig2 = 0;
                }
                if (Regex.IsMatch(cpf, @"(?<num>\d)(\k<num>){10}"))
                {
                    return false;
                }
                else if (cpf.Substring(0, 9) + dig1 + dig2 == cpf)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public static bool ValidarEmail(string Email)
        {
            return Regex.Match(Email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*").Success;
        }
    }
}