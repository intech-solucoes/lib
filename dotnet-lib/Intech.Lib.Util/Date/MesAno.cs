using System;
using System.Collections.Generic;
using System.Text;

namespace Intech.Lib.Util.Date
{
    public struct MesAno : IEquatable<MesAno>, IComparable<MesAno>
    {
        public int Mes { get; private set; }

        public int Ano { get; private set; }

        private int Meses => Ano * 12 + Mes;

        public MesAno(int ano, int mes) : this()
        {
            Ano = ano;
            Mes = mes;
        }

        public MesAno(DateTime data) : this(data.Year, data.Month)
        { }

        public MesAno Adiciona(int anos, int meses) => CriaMesAno(Ano + anos, Mes + meses);

        public MesAno AdicionaMeses(int meses) => CriaMesAno(Ano, Mes + meses);

        public MesAno AdiconaAnos(int anos) => CriaMesAno(Ano + anos, Mes);

        public bool Equals(MesAno obj) => obj.Mes == Mes && obj.Ano == Ano;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(MesAno)) return false;
            return Equals((MesAno)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mes * 397) ^ Ano;
            }
        }

        public static bool operator == (MesAno left, MesAno right) => left.Equals(right);

        public static bool operator != (MesAno left, MesAno right) => !left.Equals(right);

        public static bool operator > (MesAno left, MesAno right) => left.Meses > right.Meses;

        public static bool operator >= (MesAno left, MesAno right) => left.Meses >= right.Meses;

        public static bool operator < (MesAno left, MesAno right) => left.Meses < right.Meses;

        public static bool operator <= (MesAno left, MesAno right) => left.Meses <= right.Meses;

        public int CompareTo(MesAno other) => Meses - other.Meses;

        public override string ToString() => string.Format("{0}/{1}", Mes, Ano);

        public static bool ValidaMesAno(string dataString)
        {
            DateTime dataAux;
            int anoAux = 0;
            int mesAux = 0;
            int diaAux = 1;

            if (dataString.LimparMascara() != "")
            {
                try
                {
                    dataAux = Convert.ToDateTime(dataString);
                    anoAux = dataAux.Year;

                    if (anoAux < 1754)
                        return false;

                    mesAux = dataAux.Month;
                    diaAux = dataAux.Day;
                    dataAux = new DateTime(anoAux, mesAux, diaAux);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private MesAno CriaMesAno(int anos, int meses)
        {
            int anosAdicionais = 0;
            anosAdicionais = Math.DivRem(meses, 12, out meses);
            return new MesAno(anos + anosAdicionais, meses);
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            MesAno other = (MesAno)obj;
            return Ano * 12 + Mes - other.Ano * 12 - other.Mes;
        }

        #endregion
    }
}
