using System;
using System.Collections.Generic;
using System.Text;

namespace Intech.Lib.Util.Date
{
    /// <summary>
    /// Implementação do Pattern Strategy para isolar os algoritmos de calculo de tempo.
    /// Esta é a classe base que define o comportamento esperado do strategy.
    /// Caso algum algoritmo não suporte uma destas operações ele deverá lançar uma exceção NotImplementedExeception
    /// </summary>
    public abstract class CalculoAnosMesesDiasStrategy
    {
        public abstract int Compara(Intervalo i1, Intervalo i2);

        public abstract int ConverteParaDias(int anos, int meses, int dias);

        public abstract void ConverteParaDiasMesesAnos(DateTime dataFinal, DateTime dataInicial, out int anos, out int meses, out int dias);

        public abstract void ConverteParaDiasMesesAnos(int totalDias, out int anos, out int meses, out int dias);

        public abstract void CalculaValoresIntervalo(DateTime dataFinal, DateTime dataInicial, out int anos,
                                                     out int meses, out int dias, out int totalDeDias);

        public abstract Intervalo Adiciona(Intervalo i1, Intervalo i2);

        public abstract Intervalo Adiciona(Intervalo i1, int dias);

        public abstract Intervalo Subtrai(Intervalo i1, Intervalo i2);

        public abstract Intervalo Subtrai(Intervalo i1, int dias);

        protected void ValidaMesmoStrategy(Intervalo i1, Intervalo i2)
        {
            if (i1.Strategy.GetType() != i2.Strategy.GetType())
            {
                throw new Exception("Os dois intervalos utilizam algoritmos de cálculo de datas diferentes e por isto não podem ser utilizados nesta operação. " +
                                   $"Algoritmo1:{i1.Strategy.GetType().ToString()}, Algoritmo2:{i2.Strategy.GetType().ToString()}");
            }
        }
    }
}
