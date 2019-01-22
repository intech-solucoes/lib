namespace System
{
    public static class DecimalExtensoes
    {
        public static decimal Arredonda(this decimal numero, int casas)
        {
            decimal multiplicador = (decimal)Math.Pow(10, casas);
            decimal temp = numero * multiplicador;
            temp = Math.Round(temp);
            return temp / multiplicador;
        }

        public static decimal ElevadoA(this decimal valor, decimal potencia)
        {
            return (decimal)Math.Pow((double)valor, (double)potencia);
        }

        public static decimal Trunca(this decimal numero, int casas)
        {
            decimal multiplicador = (decimal)Math.Pow(10, casas);
            decimal temp = numero * multiplicador;
            temp = Math.Truncate(temp);
            return temp / multiplicador;
        }
    }
}
