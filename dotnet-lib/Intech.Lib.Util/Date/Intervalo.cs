using System;

namespace Intech.Lib.Util.Date
{
    public class Intervalo
    {
        #region Propriedades

        private int _totalDeDias;

        public int TotalDeDias
        {
            get { return _totalDeDias; }
            set
            {
                _totalDeDias = value;
                Strategy.ConverteParaDiasMesesAnos(_totalDeDias, out _anos, out _meses, out _dias);
                //calculaAnosMesesDias();
            }
        }

        private int _dias;
        public int Dias
        {
            get { return _dias; }
            private set { _dias = value; }
        }

        private int _meses;
        public int Meses
        {
            get { return _meses; }
            private set { _meses = value; }
        }

        private int _anos;
        public int Anos
        {
            get { return _anos; }
            private set { _anos = value; }
        }

        public int TotalMeses => Anos * 12 + Meses;

        public CalculoAnosMesesDiasStrategy Strategy { get; private set; }

        #endregion

        #region Construtores

        public Intervalo() : this(new CalculoAnosMesesDiasAlgoritmo1())
        {
        }

        public Intervalo(CalculoAnosMesesDiasStrategy strategy)
        {
            _totalDeDias = 0;
            _anos = 0;
            _meses = 0;
            _dias = 0;
            Strategy = strategy;
        }

        public Intervalo(int dias) : this(dias, new CalculoAnosMesesDiasAlgoritmo1())
        {
        }

        public Intervalo(int dias, CalculoAnosMesesDiasStrategy strategy)
        {
            //TotalDeDias = dias;
            Strategy = strategy;
            _totalDeDias = dias;
            Strategy.ConverteParaDiasMesesAnos(dias, out _anos, out _meses, out _dias);
        }

        public Intervalo(int anos, int meses, int dias) : this(anos, meses, dias, new CalculoAnosMesesDiasAlgoritmo1())
        {
        }

        public Intervalo(int anos, int meses, int dias, CalculoAnosMesesDiasStrategy strategy)
        {
            Strategy = strategy;
            Anos = anos;
            Meses = meses;
            Dias = dias;
            _totalDeDias = Strategy.ConverteParaDias(anos, meses, dias);
        }

        public Intervalo(DateTime dataFinal, DateTime dataInicial) : this(dataFinal, dataInicial, new CalculoAnosMesesDiasAlgoritmo1())
        {
        }

        public Intervalo(DateTime dataFinal, DateTime dataInicial, CalculoAnosMesesDiasStrategy strategy)
        {
            Strategy = strategy;
            Strategy.CalculaValoresIntervalo(dataFinal, dataInicial, out _anos, out _meses, out _dias, out _totalDeDias);
        }

        public Intervalo(IPeriodo periodo) : this(periodo, new CalculoAnosMesesDiasAlgoritmo1())
        {
        }

        public Intervalo(IPeriodo periodo, CalculoAnosMesesDiasStrategy strategy) : this(periodo.DataTermino, periodo.DataInicio, strategy)
        {
        }

        #endregion

        #region Métodos públicos

        public static int ConverteParaDias(int anos, int meses, int dias) => 
            anos * 365 + meses * 30 + dias;

        public void Adiciona(Intervalo intervalo)
        {
            Intervalo novoIntervalo = Strategy.Adiciona(this, intervalo);
            CopiaValores(novoIntervalo);
        }

        public void Adiciona(int dias)
        {
            Intervalo novoIntervalo = Strategy.Adiciona(this, dias);
            CopiaValores(novoIntervalo);
        }

        public void Subtrai(Intervalo intervalo)
        {
            Intervalo novoIntervalo = Strategy.Subtrai(this, intervalo);
            CopiaValores(novoIntervalo);
        }

        public void Subtrai(int dias)
        {
            Intervalo novoIntervalo = Strategy.Subtrai(this, dias);
            CopiaValores(novoIntervalo);
        }

        public int Compara(Intervalo intervalo) => Strategy.Compara(this, intervalo);

        #endregion

        #region Métodos privados

        private void CopiaValores(Intervalo intervalo)
        {
            _totalDeDias = intervalo.TotalDeDias;
            _anos = intervalo.Anos;
            _meses = intervalo.Meses;
            _dias = intervalo.Dias;
        }

        private void calculaAnosMesesDias()
        {
            int resto;
            Anos = Math.DivRem(_totalDeDias, 365, out resto);
            Meses = Math.DivRem(resto, 30, out resto);
            Dias = resto;

            if (Meses == 12)
            {
                Anos += 1;
                Meses = 0;
            }
        }

        private Intervalo Subtract(Intervalo intervalo) => Strategy.Subtrai(this, intervalo);

        private Intervalo Add(Intervalo intervalo) => Strategy.Adiciona(this, intervalo);

        #endregion

        #region Overload operadores

        /// <summary>
        /// Intervalo + Intervalo
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static Intervalo operator +(Intervalo i1, Intervalo i2) => i1.Add(i2);

        /// <summary>
        /// Intervalo + Qtd_Dias
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="dias"></param>
        /// <returns></returns>
        public static Intervalo operator +(Intervalo i1, int dias) => new Intervalo(i1.TotalDeDias + dias);

        /// <summary>
        /// Intervalo - Intervalo
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static Intervalo operator -(Intervalo i1, Intervalo i2) => i1.Subtract(i2);

        /// <summary>
        /// Intervalo - Qtd_Dias
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="dias"></param>
        /// <returns></returns>
        public static Intervalo operator -(Intervalo i1, int dias) => new Intervalo(i1.TotalDeDias - dias);

        /// <summary>
        /// Intervalo &lt; Intervalo
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool operator <(Intervalo i1, Intervalo i2) => i1.Compara(i2) < 0;

        /// <summary>
        /// Intervalo &lt;= Intervalo
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool operator <=(Intervalo i1, Intervalo i2) => i1.Compara(i2) <= 0;

        /// <summary>
        /// Intervalo &gt; Intervalo
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool operator >(Intervalo i1, Intervalo i2) => i1.Compara(i2) > 0;

        /// <summary>
        /// Intervalo &gt;= Intervalo
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool operator >=(Intervalo i1, Intervalo i2) => i1.Compara(i2) >= 0;

        public override string ToString() => $"{Anos.ToString("00")}a {Meses.ToString("00")}m {Dias.ToString("00")}d - Total {TotalDeDias.ToString("0000")} dias";

        public string ToShortString() => $"{Anos.ToString("00")}a {Meses.ToString("00")}m {Dias.ToString("00")}d";

        //Quantidade Completa
        //public string PorExtenso()
        //{
        //    string ano = string.Format("{0}", Anos.Equals(0) ? string.Empty : Anos > 1 ? string.Format("{0} ({1}) anos", Anos.ToString("00"), NumeroPorExtenso.Extenso_Valor(Anos).Replace("Reais", "").ToLower().Trim()) : string.Format("{0} ({1}) ano", Anos.ToString("00"), NumeroPorExtenso.Extenso_Valor(Anos).Replace("Real", "").ToLower().Trim()));
        //    string mes = string.Format("{0}", Meses.Equals(0) ? string.Empty : Meses > 1 ? string.Format("{0}{1} ({2}) meses", string.Format("{0}", Anos.Equals(0) ? string.Empty : !Anos.Equals(0) && Dias.Equals(0) ? " e " : ", "), Meses.ToString("00"), NumeroPorExtenso.Extenso_Valor(Meses).Replace("Reais", "").ToLower().Trim()) : string.Format("{0}{1} ({2}) mês", string.Format("{0}", Anos.Equals(0) ? string.Empty : !Anos.Equals(0) && Dias.Equals(0) ? " e " : ", "), Meses.ToString("00"), NumeroPorExtenso.Extenso_Valor(Meses).Replace("Real", "").ToLower().Trim()));
        //    string dia = string.Format("{0}", Dias.Equals(0) ? string.Empty : Dias > 1 ? string.Format("{0}{1} ({2}) dias", string.Format("{0}", Anos.Equals(0) && Meses.Equals(0) ? string.Empty : " e "), Dias.ToString("00"), NumeroPorExtenso.Extenso_Valor(Dias).Replace("Reais", "").ToLower().Trim()) : string.Format("{0}{1} ({2}) dia", string.Format("{0}", Anos.Equals(0) && Meses.Equals(0) ? string.Empty : " e "), Dias.ToString("00"), NumeroPorExtenso.Extenso_Valor(Dias).Replace("Real", "").ToLower().Trim()));

        //    return string.Format("{0}{1}{2}", Anos.Equals(0) ? string.Empty : ano, Meses.Equals(0) ? string.Empty : mes, Dias.Equals(0) ? string.Empty : dia);
        //}

        #endregion
    }
}
