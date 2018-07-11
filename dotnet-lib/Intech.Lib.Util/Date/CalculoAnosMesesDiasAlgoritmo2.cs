using System;

namespace Intech.Lib.Util.Date
{
    /// <summary>
    /// Implementação do Pattern Strategy para isolar os algoritmos de calculo de tempo.
    /// Este algoritmo se baseia principalmente no calculo utilizando dias, meses e anos.
    /// </summary>
    public class CalculoAnosMesesDiasAlgoritmo2 : CalculoAnosMesesDiasStrategy
    {
        public override int Compara(Intervalo i1, Intervalo i2)
        {
            int anos = i1.Anos - i2.Anos;

            if (anos != 0) return anos;

            int meses = i1.Meses - i2.Meses;

            if (meses != 0) return meses;

            int dias = i1.Dias - i2.Dias;

            return dias;
        }

        public override int ConverteParaDias(int anos, int meses, int dias)
        {
            return anos * 365 + meses * 30 + dias;
        }

        public override void ConverteParaDiasMesesAnos(DateTime dataFinal, DateTime dataInicial, out int anos, out int meses, out int dias)
        {
            dias = 0;
            meses = 0;
            anos = 0;

            DateTime data = dataFinal;

            dias = data.Day - dataInicial.Day;
            meses += data.Month - dataInicial.Month;
            anos += data.Year - dataInicial.Year;

            if (dias < 0)
            {
                dias += 30;
                meses -= 1;
            }
            if (dias == 30)
            {
                dias = 0;
                meses += 1;
            }

            if (meses < 0)
            {
                meses += 12;
                anos -= 1;
            }
            if (meses == 12)
            {
                meses = 0;
                anos += 1;
            }
        }

        public override void ConverteParaDiasMesesAnos(int totalDias, out int anos, out int meses, out int dias)
        {
            throw new Exception("Este algoritmo não suporta conversão de Dias em Anos-Meses-Dias");
        }

        public override void CalculaValoresIntervalo(DateTime dataFinal, DateTime dataInicial, out int anos, out int meses, out int dias, out int totalDeDias)
        {
            ConverteParaDiasMesesAnos(dataFinal, dataInicial, out anos, out meses, out dias);
            totalDeDias = ConverteParaDias(anos, meses, dias);
        }

        public override Intervalo Adiciona(Intervalo i1, Intervalo i2)
        {
            ValidaMesmoStrategy(i1, i2);

            int dias = i1.Dias + i2.Dias;
            int meses = i1.Meses + i2.Meses;
            int anos = i1.Anos + i2.Anos;
            return CriaIntervalo(anos, meses, dias);
        }

        public override Intervalo Adiciona(Intervalo i1, int dias)
        {
            dias = i1.Dias + dias;
            int meses = i1.Meses;
            int anos = i1.Anos;
            return CriaIntervalo(anos, meses, dias);
        }

        public override Intervalo Subtrai(Intervalo i1, Intervalo i2)
        {
            ValidaMesmoStrategy(i1, i2);

            int dias = i1.Dias - i2.Dias;
            int meses = i1.Meses - i2.Meses;
            int anos = i1.Anos - i2.Anos;

            if (dias < 0)
            {
                dias += 30;
                meses -= 1;
            }
            if (dias == 30)
            {
                dias = 0;
                meses += 1;
            }
            if (meses < 0)
            {
                meses += 12;
                anos -= 1;
            }
            if (meses == 12)
            {
                meses = 0;
                anos += 1;
            }
            if (anos < 0)
                throw new Exception($"A subração dos períodos retornou um valor negativo. Período1:{i1.ToShortString()}, Período2:{i2.ToShortString()}");

            return new Intervalo(anos, meses, dias, new CalculoAnosMesesDiasAlgoritmo2());
        }

        public override Intervalo Subtrai(Intervalo i1, int dias)
        {
            dias = i1.Dias - dias;
            int meses = i1.Meses;
            int anos = i1.Anos;

            while (dias < 0)
            {
                dias += 30;
                meses -= 1;
            }
            if (dias == 30)
            {
                dias = 0;
                meses += 1;
            }
            if (meses < 0)
            {
                meses += 12;
                anos -= 1;
            }
            if (meses == 12)
            {
                meses = 0;
                anos += 1;
            }
            if (anos < 0)
                throw new Exception($"A subração dos períodos retornou um valor negativo. Período1:{i1.ToShortString()}, Dias:{dias}");

            return new Intervalo(anos, meses, dias, new CalculoAnosMesesDiasAlgoritmo2());
        }

        protected Intervalo CriaIntervalo(int anos, int meses, int dias)
        {
            while (dias >= 30)
            {
                meses += 1;
                dias -= 30;
            }

            while (meses >= 12)
            {
                anos += 1;
                meses -= 12;
            }
            return new Intervalo(anos, meses, dias, new CalculoAnosMesesDiasAlgoritmo2());
        }
    }
}
