using System;

namespace Intech.Lib.Util.Date
{
    /// <summary>
    /// Implementação do Pattern Strategy para isolar os algoritmos de calculo de tempo.
    /// Este algoritmo se baseia principalmente no calculo utilizando o número total de dias.
    /// </summary>
    public class CalculoAnosMesesDiasAlgoritmo1 : CalculoAnosMesesDiasStrategy
    {
        public override int Compara(Intervalo i1, Intervalo i2) => i1.TotalDeDias - i2.TotalDeDias;

        public override int ConverteParaDias(int anos, int meses, int dias) => 
            anos * 365 + meses * 30 + dias;

        public override void ConverteParaDiasMesesAnos(DateTime dataFinal, DateTime dataInicial, out int anos, out int meses, out int dias)
        {
            int totalDeDias = (int)dataFinal.Subtract(dataInicial).TotalDays;

            ConverteParaDiasMesesAnos(totalDeDias, out anos, out meses, out dias);
        }

        public override void ConverteParaDiasMesesAnos(int totalDeDias, out int anos, out int meses, out int dias)
        {
            anos = Math.DivRem(totalDeDias, 365, out int resto);
            meses = Math.DivRem(resto, 30, out resto);
            dias = resto;

            if (meses == 12)
            {
                anos += 1;
                meses = 0;
            }
        }

        public override void CalculaValoresIntervalo(DateTime dataFinal, DateTime dataInicial, out int anos, out int meses, out int dias, out int totalDeDias)
        {
            totalDeDias = dataInicial.DiasEntreDatas(dataFinal);

            ConverteParaDiasMesesAnos(totalDeDias, out anos, out meses, out dias);
        }

        public override Intervalo Adiciona(Intervalo i1, Intervalo i2)
        {
            ValidaMesmoStrategy(i1, i2);
            int totalDeDias = i1.TotalDeDias + i2.TotalDeDias;
            return new Intervalo(totalDeDias, i1.Strategy);
        }

        public override Intervalo Adiciona(Intervalo i1, int dias)
        {
            int totalDeDias = i1.TotalDeDias + dias;
            return new Intervalo(totalDeDias, i1.Strategy);
        }

        public override Intervalo Subtrai(Intervalo i1, Intervalo i2)
        {
            ValidaMesmoStrategy(i1, i2);
            int totalDeDias = i1.TotalDeDias - i2.TotalDeDias;
            return new Intervalo(totalDeDias, i1.Strategy);
        }

        public override Intervalo Subtrai(Intervalo i1, int dias)
        {
            int totalDeDias = i1.TotalDeDias - dias;
            return new Intervalo(totalDeDias, i1.Strategy);
        }
    }
}
