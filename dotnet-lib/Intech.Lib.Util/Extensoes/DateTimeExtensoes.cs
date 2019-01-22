using Intech.Lib.Util.Date;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class DateTimeExtensoes
    {
        /// <summary>
        /// Retorna o primeiro dia do mes da data informada
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime PrimeiroDiaDoMes(this DateTime data) => new DateTime(data.Year, data.Month, 1);

        /// <summary>
        /// Retorna o primeiro dia do ano do mesmo ano da data informada
        /// </summary>
        public static DateTime PrimeiroDiaDoAno(this DateTime data) => new DateTime(data.Year, 1, 1);

        /// <summary>
        /// Calcula o período entre duas datas
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataReferencia"></param>
        /// <returns></returns>
        public static Intervalo IdadeEm(this DateTime data, DateTime dataReferencia) =>
            new Intervalo(dataReferencia, data, new CalculoAnosMesesDiasAlgoritmo2());

        /// <summary>
        /// Método que retorna o mês por extenso
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string MesPorExtenso(this DateTime data) => MesPorExtenso(data.Month.ToString());

        public static string MesPorExtenso(string mes)
        {
            string[] meses = {"Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro",
                "Outubro", "Novembro", "Dezembro"};

            return meses[Convert.ToInt32(mes) - 1];
        }

        /// <summary>
        /// Retona o último dia do ano do mesmo ano da data informada
        /// </summary>
        public static DateTime UltimoDiaDoAno(this DateTime data) => new DateTime(data.Year, 12, 31);

        /// <summary>
        /// Retorna em qual decêndio a data se encontra dentro do mês.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int NumeroDecendio(this DateTime data)
        {
            int dia = data.Day;
            if (dia < 11)
                return 1;
            else if (dia < 21)
                return 2;
            else
                return 3;
        }

        /// <summary>
        /// Retorna em qual quinzena a data se encontra dentro do mês.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int NumeroQuinzena(this DateTime data) => data.Day < 16 ? 1 : 2;

        /// <summary>
        /// Retorna em qual semana a data se encontra dentro do mês.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int NumeroSemana(this DateTime data)
        {
            DateTime dataPrimeiroDia = data.DefinirDia(1);

            int diaSemana = 0;
            int numSemana = 1;

            while (dataPrimeiroDia < data)
            {
                diaSemana = (int)data.DayOfWeek;

                if (diaSemana == 0)
                    numSemana++;

                data = data.AddDays(1);

            }
            return numSemana;
        }

        /// <summary>
        /// Substitui o dia da data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dia"></param>
        /// <returns>o mesmo mês e ano com o novo dia</returns>
        public static DateTime DefinirDia(this DateTime data, int dia)
        {
            try
            {
                return new DateTime(data.Year, data.Month, dia);
            }
            catch (Exception)
            {
                return new DateTime(data.Year, data.Month, data.QuantidadeDiasMes());
            }
        }

        /// <summary>
        /// Retorna a quantidade de dias do mês
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int QuantidadeDiasMes(this DateTime data) => DateTime.DaysInMonth(data.Year, data.Month);

        /// <summary>
        /// Retorna o ultimo dia do mês
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime UltimoDiaDoMes(this DateTime data)
        {
            var proximoMes = data.AddMonths(1);
            var ultimoDia = proximoMes.AddDays(-proximoMes.Day);

            ultimoDia = new DateTime(ultimoDia.Year, ultimoDia.Month, ultimoDia.Day);

            return ultimoDia;
        }

        /// <summary>
        /// Verifica se é o mesmo mês e ano
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataComparacao"></param>
        /// <returns></returns>
        public static bool IgualMesAno(this DateTime data, DateTime dataComparacao) => data.Month == dataComparacao.Month && data.Year == dataComparacao.Year;

        /// <summary>
        /// Verifica se a data é menor que a data (Mês e Ano) informada por parâmetro
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataComparacao"></param>
        /// <returns></returns>
        public static bool MenorOuIgualQueMesAno(this DateTime data, DateTime dataComparacao)
        {
            var dataTemp = new DateTime(data.Year, data.Month, 1);
            var dataComparacaoTemp = new DateTime(dataComparacao.Year, dataComparacao.Month, 1);

            return dataTemp <= dataComparacaoTemp;
        }

        /// <summary>
        /// Verifica se o dia é um dia util ou não
        /// </summary>
        /// <param name="data"></param>
        /// <param name="feriados"></param>
        /// <returns></returns>
        public static bool EhDiaUtil(this DateTime data, IEnumerable<DateTime> feriados) =>
            !(data.DayOfWeek == DayOfWeek.Sunday
            || data.DayOfWeek == DayOfWeek.Saturday
            || feriados.Contains(data));

        /// <summary>
        /// Adiciona uma quantidade de dias uteis na data
        /// </summary>
        /// <param name="data">data alvo</param>
        /// <param name="qtdDias">quantidade de dias a adicionar</param>
        /// <param name="feriados">lista de feriados</param>
        /// <returns></returns>
        public static DateTime AddDiaUtil(this DateTime data, int qtdDias, IEnumerable<DateTime> feriados)
        {
            int qtdDiasUteis = 1;

            while (qtdDias >= qtdDiasUteis)
            {
                data = data.AddDays(1);

                if (data.EhDiaUtil(feriados) == true)
                    qtdDiasUteis = qtdDiasUteis + 1;
            }

            return data;
        }

        /// <summary>
        /// Retorna o número total de dias entre duas datas
        /// </summary>
        /// <param name="dataInicial"></param>
        /// <param name="dataFinal"></param>
        /// <returns></returns>
        public static int DiasEntreDatas(this DateTime dataInicial, DateTime dataFinal)
        {
            TimeSpan diferencaDatas = dataFinal.Subtract(dataInicial);
            //o "+ 1" serve para incluir o dia inicial no total
            return diferencaDatas.Days + 1;
        }

        /// <summary>
        /// Retorna o número de meses entre duas datas ignorando os dias.
        /// 30/08/2007 e 01/10/2007 = 2 meses
        /// </summary>
        /// <param name="dataInicial">Data inicial</param>
        /// <param name="dataFinal">Data final</param>
        /// <returns>Número de meses entre duas datas</returns>
        public static int MesesEntreDatas(this DateTime dataInicial, DateTime dataFinal)
        {
            int anos = dataFinal.Year - dataInicial.Year;
            //anos * 12 é utilizado no caso da data final estar em um ano diferente da data de pagamento
            return (anos * 12) + dataFinal.Month - dataInicial.Month;
        }

        /// <summary>
        /// Retorna o números de meses entre duas datas ignorando os dias.
        /// 30/08/2007 e 01/10/2007 = 3 meses => mês 8, 9 e 10.
        /// </summary>
        /// <param name="dataInicial"></param>
        /// <param name="dataFinal"></param>
        /// <param name="contagemComercial"></param>
        /// <returns></returns>
        public static int MesesEntreDatas(this DateTime dataInicial, DateTime dataFinal, bool contagemComercial)
        {
            int x = contagemComercial ? 1 : 0;
            return MesesEntreDatas(dataInicial, dataFinal) + x;
        }

        public enum Estado { AC, AL, AP, AM, BA, CE, DF, GO, ES, MA, MT, MS, MG, PA, PB, PR, PE, PI, RJ, RN, RS, RO, RR, SP, SC, SE, TO };

        public enum Direcao
        {
            Posterior,
            Anterior
        };
    }
}
