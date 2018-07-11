using System;
using System.Security.Cryptography;
using System.Text;

namespace Intech.Lib.Util.Seguranca
{
    public static class Criptografia
    {
        /// <summary>
        /// Salt by bruneca
        /// </summary>
        private static readonly string Salt = "]oç/*9í$";

        #region Public Static Methods

        public static string Encriptar(string stringToEncrypt)
        {
            stringToEncrypt = stringToEncrypt + Salt;

            var sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                var enc = Encoding.UTF8;
                var result = hash.ComputeHash(enc.GetBytes(stringToEncrypt));

                foreach (var b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        public static string EncriptarMD5(string TextoClaro)
        {
            try
            {
                byte[] tmp;
                tmp = Encoding.ASCII.GetBytes(TextoClaro);
                tmp = new MD5CryptoServiceProvider().ComputeHash(tmp);
                return ByteArrayToString(tmp);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();

        }

        #endregion
    }
}
