using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Intech.Lib.Data.Util.Tradutor
{
    class Tokenizer
    {
        private readonly TextReader fonte;
        private char caracterAtual;
        private readonly StringBuilder buffer;
        private readonly Queue<Token> fila;

        public bool Fim
        {
            get { return caracterAtual == '\0'; }
        }

        public Tokenizer(string fonte)
        {
            if (fonte == null)
            {
                throw new ArgumentNullException("fonte");
            }

            this.fonte = new StringReader(fonte);
            buffer = new StringBuilder();
            fila = new Queue<Token>();
            leProximoCaracter();
        }

        private void leProximoCaracter()
        {
            int codigoCaracter = fonte.Read();

            caracterAtual = codigoCaracter > 0 ? (char)codigoCaracter : '\0';
        }

        private void pulaCaracterVazio()
        {
            while (char.IsWhiteSpace(caracterAtual))
            {
                leProximoCaracter();
            }
        }

        private void armezenaCaracterAtualLeProximo()
        {
            buffer.Append(caracterAtual);
            leProximoCaracter();
        }

        private string extraiCaracteresArmazenados()
        {
            string valor = buffer.ToString();
            buffer.Length = 0;
            return valor;
        }

        private void verificaFimInesperado()
        {
            if (Fim)
            {
                throw new Exception("Fim inesperado.");
            }
        }

        private void lancaExcecaoCaracterInvalido()
        {
            if (buffer.Length == 0)
            {
                throw new Exception(string.Format("Caracter inválido: '{0}'", caracterAtual));
            }
            else
            {
                throw new Exception(string.Format("Caracter inválido: '{0}' depois de '{1}'", caracterAtual, buffer));
            }
        }

        public Token LeProximoToken()
        {
            pulaCaracterVazio();

            if (Fim)
            {
                return null;
            }

            if (char.IsLetter(caracterAtual) || caracterAtual == '[')
            {
                return lePalavra();
            }

            if (char.IsDigit(caracterAtual))
            {
                return leConstanteNumerica();
            }

            if (caracterAtual == '\'')
            {
                return leConstanteString();
            }

            if (caracterAtual == '@')
            {
                return leVariavel();
            }
            Token tk = leSimbolo();
            return tk;
        }

        private Token leVariavel()
        {
            leProximoCaracter();

            do
            {
                armezenaCaracterAtualLeProximo();
            } while (char.IsLetterOrDigit(caracterAtual) || caracterAtual == '_');

            string palavra = extraiCaracteresArmazenados();

            return new Token(TipoToken.Variavel, palavra);
        }

        public Queue<Token> ObtemTokens()
        {
            while (!Fim)
            {
                Token tk = LeProximoToken();
                if (tk != null)
                {
                    fila.Enqueue(tk);
                }
            }

            return fila;
        }

        private Token leSimbolo()
        {
            switch (caracterAtual)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '^':
                case '(':
                case ')':
                case '[':
                case ']':
                case ';':
                case ':':
                case '=':
                case '.':
                case ',':
                    armezenaCaracterAtualLeProximo();
                    return new Token(TipoToken.Simbolo, extraiCaracteresArmazenados());
                case '<':
                    armezenaCaracterAtualLeProximo();
                    if (caracterAtual == '>' || caracterAtual == '=')
                    {
                        armezenaCaracterAtualLeProximo();
                    }
                    return new Token(TipoToken.Simbolo, extraiCaracteresArmazenados());
                case '>':
                    armezenaCaracterAtualLeProximo();
                    if (caracterAtual == '=')
                    {
                        armezenaCaracterAtualLeProximo();
                    }
                    return new Token(TipoToken.Simbolo, extraiCaracteresArmazenados());
                default:
                    verificaFimInesperado();
                    lancaExcecaoCaracterInvalido();
                    break;
            }

            return null;
        }

        private Token leConstanteString()
        {
            leProximoCaracter();

            while (!Fim && caracterAtual != '\'')
            {
                armezenaCaracterAtualLeProximo();
            }

            verificaFimInesperado();

            leProximoCaracter();

            return new Token(TipoToken.String, extraiCaracteresArmazenados());
        }

        private Token leConstanteNumerica()
        {
            do
            {
                armezenaCaracterAtualLeProximo();
            } while (char.IsDigit(caracterAtual) || caracterAtual == '.');

            return new Token(TipoToken.Numero, extraiCaracteresArmazenados());
        }

        private Token lePalavra()
        {
            do
            {
                if (caracterAtual == '[' || caracterAtual == ']')
                {
                    leProximoCaracter();
                }
                else
                {
                    armezenaCaracterAtualLeProximo();
                }
            } while (char.IsLetterOrDigit(caracterAtual) || caracterAtual == '_' || caracterAtual == '.' ||
                     caracterAtual == '[' || caracterAtual == ']');

            string palavra = extraiCaracteresArmazenados();

            return palavraChave(palavra) ? new Token(TipoToken.PalavraChave, palavra) :
                                                                                          new Token(TipoToken.Palavra, palavra);
        }

        private static bool palavraChave(string palavra)
        {
            string[] palavrasChave = new[] {"SELECT", "FROM", "WHERE", "UNION", "ORDER", "BY", "AS", "ALL", "DISTINCT", "CAST", "YEAR",
                                            "MONTH", "DAY", "DATEPART", "AND", "OR", "NOLOCK", "INSERT", "VALUES", "INTO", "UPDATE", "SET", "DELETE", "VARCHAR",
                                            "NUMERIC", "DATETIME", "CHAR","MAX", "MIN", "COUNT", "HAVING", "CASE", "WHEN", "THEN", "ELSE", "END", "IN", "EXISTS",
                                            "ISNULL", "SUM", "AVG", "TOP", "SCOPE_IDENTITY", "INT", "CONVERT", "BETWEEN", "GETDATE", "INNER", "JOIN", "OUTER",
                                            "LEFT", "RIGHT", "DATEADD", "LIKE", "ON", "IS", "NULL", "DESC", "ASC", "CROSS", "NOT", "EXISTS", "WITH", "GROUP", "ALL"};

            return palavrasChave.Contains(palavra.ToUpper());
        }
    }
}
