using System;

namespace Intech.Lib.Data.Util.Tradutor
{
    public class Token
    {
        public TipoToken Tipo { get; private set; }
        public string Valor { get; private set; }

        public Token(TipoToken tipo, string valor)
        {
            if (valor == null)
            {
                throw new ArgumentNullException("valor do token");
            }

            Tipo = tipo;
            Valor = valor.ToUpper();
        }

        public bool Equals(TipoToken tipo, string valor)
        {
            return Tipo == tipo && Valor == valor;
        }

        public override string ToString()
        {
            return string.Format("Tipo: {0} Valor: {1}", Tipo, Valor);
        }
    }
}
