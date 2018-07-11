using System;

namespace Intech.Lib.Util.Date
{
    /// <summary>
    /// Esta interface define um contrato a ser utilizado em classes que definam
    /// um período de tempo
    /// </summary>
    public interface IPeriodo
    {
        /// <summary>
        /// Data inicial do período
        /// </summary>
        DateTime DataInicio { get; }

        /// <summary>
        /// Data de término do período
        /// </summary>
        DateTime DataTermino { get; }
    }
}
