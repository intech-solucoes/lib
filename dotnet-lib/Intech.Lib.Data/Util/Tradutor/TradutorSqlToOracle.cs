using System;
using System.Collections.Generic;
using System.Linq;

namespace Intech.Lib.Data.Util.Tradutor
{
    enum TipoOperacao
    {
        Indefinida,
        Adicao,
        Concatenacao
    }

    public class TradutorSqlToOracle
    {
        private Tokenizer Tokenizer;

        private Queue<Token> Tokens;

        private readonly Stack<TipoOperacao> PilhaOperacao = new Stack<TipoOperacao>();

        private Token TokenAtual;

        private bool GerarInsertComPK;

        private readonly List<string> ListaSaida = new List<string>();

        private TipoOperacao TipoOperacaoCorrente = TipoOperacao.Indefinida;

        private readonly string[] Operadores = new[] { "CASE" };

        int TopCount;

        public string SqlOriginal { get; private set; }

        public string Traduz(string sql, bool gerarInsertComPK)
        {
            ListaSaida.Clear();
            TopCount = 0;
            SqlOriginal = sql;
            Tokenizer = new Tokenizer(sql);
            GerarInsertComPK = gerarInsertComPK;
            return Traduz(Tokenizer.ObtemTokens());
        }

        private string Traduz(Queue<Token> tokens)
        {
            this.Tokens = tokens;
            leProximoToken();
            if (TokenAtual.Equals(TipoToken.PalavraChave, "SELECT"))
            {
                return traduzSelect();
            }
            if (TokenAtual.Equals(TipoToken.PalavraChave, "INSERT"))
            {
                return traduzInsert();
            }
            if (TokenAtual.Equals(TipoToken.PalavraChave, "UPDATE"))
            {
                return traduzUpdate();
            }
            if (TokenAtual.Equals(TipoToken.PalavraChave, "DELETE"))
            {
                return traduzDelete();
            }
            throw new Exception("Comando não reconhecido: " + TokenAtual);
        }

        private string traduzSelect()
        {
            string valor = parseSelect();
            valor += parseFrom();
            valor += parseWhere();
            valor += parseGroupBy();
            valor += parseHaving();
            valor += parseOrderBy();
            valor += parseUnion();
            return valor;
        }

        private static bool ehTabelaDeLog(string nomeTabela)
        {
            return nomeTabela.StartsWith("L_");
        }

        /// <summary>
        /// Retorna o nome da sequence da tabela.
        /// </summary>
        /// <param name="nomeTabela"></param>
        /// <returns></returns>
        /// <remarks>
        /// Nas tabelas convencionais o nome da tabela é precedido de 'S_'
        /// (Ex: BNP_RUBRICA - S_BNP_RUBRICA)
        /// Nas tabelas de log devido a uma restrição do oracle, os objetos de 
        /// banco podem tem no máximo 30 caracteres e por isso foi decidio 
        /// utilizar-se apenas o prefixo S (Ex: L_BNP_RUBRICA - SL_BNP_RUBRICA).
        /// </remarks>
        private static string obtemNomeSequence(string nomeTabela)
        {
            string prefixo = ehTabelaDeLog(nomeTabela) ? "S" : "S_";
            return prefixo + nomeTabela;
        }

        /// <summary>
        /// Retorna o nome da PK baseado no nome da tabela.
        /// </summary>
        /// <param name="nomeTabela"></param>
        /// <returns></returns>
        /// <remarks>
        /// As tabelas de LOG possuem um mesmo nome para suas PK's: OID_LOG, já nas demais tabelas
        /// o nome da PK é o prefixo 'OID_' mais o nome da tabela sem seu prefixo de sistema.
        /// Ex: Tabela: BNP_RUBRICA PK: OID_RUBRICA
        /// </remarks>
        private static string obtemNomePk(string nomeTabela)
        {
            return ehTabelaDeLog(nomeTabela) ? "OID_LOG" : "OID_" + nomeTabela.Substring(4);
        }

        private string traduzInsert()
        {
            pulaTokenEsperado(TipoToken.PalavraChave, "INSERT");
            pulaTokenEsperado(TipoToken.PalavraChave, "INTO");
            string nomeTabela = TokenAtual.Valor;
            string nomePk = obtemNomePk(nomeTabela);
            leProximoToken();
            string colunas = "";
            bool autoincremento = GerarInsertComPK;
            pulaTokenEsperado(TipoToken.Simbolo, "(");
            while (!TokenAtual.Equals(TipoToken.Simbolo, ")"))
            {
                if (TokenAtual.Equals(TipoToken.Palavra, nomePk))
                {
                    autoincremento = false;
                }
                colunas += parseColunaOuVariavelOuFuncao();
                if (!TokenAtual.Equals(TipoToken.Simbolo, ")"))
                {
                    pulaTokenEsperado(TipoToken.Simbolo, ",");
                    colunas += ", ";
                }
            }
            pulaTokenEsperado(TipoToken.Simbolo, ")");

            /* Zenildo:
             * Traduzindo o INSERT SELECT 
             * É obrigatório informar as colunas do INSERT
             */
            if (TokenAtual.Equals(TipoToken.PalavraChave, "SELECT"))
            {
                var indiceSelect = SqlOriginal.IndexOf("SELECT");
                string select = SqlOriginal.Substring(indiceSelect, SqlOriginal.Length - indiceSelect);

                string selectTraduzido = Traduz(select, GerarInsertComPK);

                return string.Format("INSERT INTO {0} SELECT {1}.NEXTVAL,{2}",
                                        nomeTabela, nomePk, selectTraduzido.Replace("SELECT", string.Empty));
            }
            else
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "VALUES");
                string valores = "";
                pulaTokenEsperado(TipoToken.Simbolo, "(");
                while (!TokenAtual.Equals(TipoToken.Simbolo, ")"))
                {
                    valores += parseColunaOuVariavelOuFuncao();
                    if (!TokenAtual.Equals(TipoToken.Simbolo, ")"))
                    {
                        pulaTokenEsperado(TipoToken.Simbolo, ",");
                        valores += ", ";
                    }
                }
                pulaTokenEsperado(TipoToken.Simbolo, ")");
                if (GerarInsertComPK)
                {
                    if (ehTabelaDeLog(nomeTabela))
                        return $"INSERT INTO {nomeTabela} ({nomePk},{colunas}) VALUES ({obtemNomeSequence(nomeTabela)}.NEXTVAL,{valores})";

                    return $"INSERT INTO {nomeTabela} ({nomePk},{colunas}) VALUES ({obtemNomeSequence(nomeTabela)}.NEXTVAL,{valores}) RETURNING {nomePk} INTO :PK";
                }

                return $"INSERT INTO {nomeTabela} ({colunas}) VALUES ({valores})";
            }
        }

        private string traduzUpdate()
        {
            pulaTokenEsperado(TipoToken.PalavraChave, "UPDATE");
            string expressao = "UPDATE " + TokenAtual.Valor + " SET ";
            leProximoToken();
            pulaTokenEsperado(TipoToken.PalavraChave, "SET");
            while (!fimDoSql && !TokenAtual.Equals(TipoToken.PalavraChave, "FROM") && !TokenAtual.Equals(TipoToken.PalavraChave, "WHERE"))
            {
                string atribuicao = parseColunaOuVariavelOuFuncao();
                pulaTokenEsperado(TipoToken.Simbolo, "=");
                atribuicao += "=" + parseExpressao();
                if (TokenAtual != null && TokenAtual.Equals(TipoToken.Simbolo, ","))
                {
                    pulaTokenEsperado(TipoToken.Simbolo, ",");
                    atribuicao += ", ";
                }
                expressao += atribuicao;
            }
            if (TokenAtual != null && TokenAtual.Equals(TipoToken.PalavraChave, "FROM"))
                expressao += parseFrom();
            expressao += parseWhere();

            //para reduzir a tamanho dos parâmetros os parâmetros que inicial com Original_
            //são transformados em 'O_'
            expressao = expressao.Replace(":ORIGINAL_", ":O_");
            return expressao;
        }

        private string traduzDelete()
        {
            string expressao = "DELETE FROM ";
            pulaTokenEsperado(TipoToken.PalavraChave, "DELETE");
            pulaTokenEsperado(TipoToken.PalavraChave, "FROM");
            expressao += TokenAtual.Valor;
            leProximoToken();
            expressao += parseWhere();
            //para reduzir a tamanho dos parâmetros os parâmetros que inicial com Original_
            //são transformados em 'O_'
            expressao = expressao.Replace(":ORIGINAL_", ":O_");
            return expressao;
        }

        private string parseSelect()
        {
            string valor = "SELECT ";
            pulaTokenEsperado(TipoToken.PalavraChave, "SELECT");

            if (TokenAtual.Equals(TipoToken.PalavraChave, "DISTINCT") ||
                TokenAtual.Equals(TipoToken.PalavraChave, "ALL"))
            {
                valor += TokenAtual.Valor + " ";
                leProximoToken();
            }


            if (TokenAtual.Equals(TipoToken.PalavraChave, "TOP"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "TOP");
                if (TokenAtual.Tipo == TipoToken.Numero)
                {
                    TopCount = int.Parse(TokenAtual.Valor);
                    leProximoToken();
                }
                else
                {
                    pulaTokenEsperado(TipoToken.Simbolo, "(");
                    TopCount = int.Parse(TokenAtual.Valor);
                    leProximoToken();
                    pulaTokenEsperado(TipoToken.Simbolo, ")");
                }
            }

            while (!fimDoSelect)
            {
                TipoOperacaoCorrente = TipoOperacao.Indefinida;
                valor += parseAliasDeColuna(parseExpressao());
                if (!fimDoSelect)
                {
                    pulaTokenEsperado(TipoToken.Simbolo, ",");
                    valor += ", ";
                }
            }
            return valor;
        }

        private string parseFrom()
        {
            string expressao = " FROM ";
            PilhaOperacao.Clear();
            pulaTokenEsperado(TipoToken.PalavraChave, "FROM");
            while (!fimDoFrom)
            {
                TipoOperacaoCorrente = TipoOperacao.Indefinida;
                expressao += parseSourceTable();
            }
            return expressao;
        }

        private string parseWhere()
        {
            //[ WHERE <search_condition> ]
            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "WHERE"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "WHERE");
                string expressao = " WHERE ";
                expressao += parseExpressao();
                if (TopCount > 0)
                {
                    expressao += string.Format(" AND ROWNUM <= {0} ", TopCount);
                }
                return expressao;
            }
            if (TopCount > 0)
            {
                return string.Format(" WHERE ROWNUM <= {0} ", TopCount);
            }
            return "";
        }

        private string parseGroupBy()
        {
            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "GROUP"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "GROUP");
                pulaTokenEsperado(TipoToken.PalavraChave, "BY");
                string expressao = " GROUP BY ";
                while (!fimDoGroupBy)
                {
                    expressao += parseExpressao();
                    if (!fimDoGroupBy)
                    {
                        pulaTokenEsperado(TipoToken.Simbolo, ",");
                        expressao += ", ";
                    }
                }
                return expressao;
            }
            return "";
        }

        private string parseHaving()
        {
            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "HAVING"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "HAVING");
                return " HAVING " + parseExpressao();
            }
            return "";
        }

        private string parseOrderBy()
        {
            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "ORDER"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "ORDER");
                pulaTokenEsperado(TipoToken.PalavraChave, "BY");
                string expressao = " ORDER BY ";
                while (!fimDoOrderBy)
                {
                    expressao += parseExpressaoBase();
                    if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "DESC"))
                    {
                        pulaTokenEsperado(TipoToken.PalavraChave, "DESC");
                        expressao += " DESC";
                    }
                    if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "ASC"))
                    {
                        pulaTokenEsperado(TipoToken.PalavraChave, "ASC");
                        expressao += " ASC";
                    }
                    if (!fimDoOrderBy)
                    {
                        pulaTokenEsperado(TipoToken.Simbolo, ",");
                        expressao += ", ";
                    }
                }
                return expressao;
            }
            return "";
        }

        private string parseUnion()
        {
            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "UNION"))
            {
                //pulaTokenEsperado(TipoToken.PalavraChave, "UNION");
                string expressao = " UNION ";

                if (proximoTokenIgualA(TipoToken.PalavraChave, "ALL"))
                {
                    leProximoToken();
                    expressao += "ALL ";
                }

                TradutorSqlToOracle tradutor = new TradutorSqlToOracle();
                expressao += tradutor.Traduz(Tokens);
                return expressao;
            }
            return "";
        }

        private void empilhaTipoOperacao(TipoOperacao tipo)
        {
            PilhaOperacao.Push(TipoOperacaoCorrente);
            TipoOperacaoCorrente = tipo;
        }

        private void desemplilhaTipoOperacao()
        {
            TipoOperacaoCorrente = PilhaOperacao.Pop();
        }

        private string parseExpressao()
        {
            string expressao = parseSubquery();

            return expressao;
        }

        private string parseSubquery()
        {
            if (TokenAtual.Equals(TipoToken.PalavraChave, "SELECT"))
            {
                int cont = 1;
                Queue<Token> tokensSubquery = new Queue<Token>();

                while (cont > 0)
                {
                    if (TokenAtual.Equals(TipoToken.Simbolo, "("))
                    {
                        cont += 1;
                    }
                    else if (TokenAtual.Equals(TipoToken.Simbolo, ")"))
                    {
                        cont -= 1;
                    }
                    if (cont > 0)
                    {
                        tokensSubquery.Enqueue(TokenAtual);
                        leProximoToken();
                    }
                }
                TradutorSqlToOracle tradutor = new TradutorSqlToOracle();
                return tradutor.Traduz(tokensSubquery);
            }
            return parseExpressaoAnd();
        }

        private string parseExpressaoAnd()
        {
            string expressao = parseExpressaoOr();

            while (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "AND"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "AND");
                expressao += " AND " + parseExpressaoOr();
            }

            return expressao;
        }

        private string parseExpressaoOr()
        {
            string expressao = parseIsNull();

            while (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "OR"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "OR");
                expressao += " OR " + parseIsNull();
            }

            return expressao;
        }

        private string parseIsNull()
        {
            //expression IS [ NOT ] NULL 

            string expressao = parseExists();

            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "IS"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "IS");
                expressao += " IS ";
                expressao += parseNot();
                pulaTokenEsperado(TipoToken.PalavraChave, "NULL");
                expressao += "NULL ";
            }
            return expressao;
        }

        private string parseExists()
        {
            //string expressao = parseIn();
            string expressao = parseNot();

            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "EXISTS"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "EXISTS");
                expressao += " EXISTS ";
                pulaTokenEsperado(TipoToken.Simbolo, "(");
                string valor = "";
                while (!TokenAtual.Equals(TipoToken.Simbolo, ")"))
                {
                    valor += parseExpressao();
                    if (TokenAtual.Equals(TipoToken.Simbolo, ","))
                    {
                        pulaTokenEsperado(TipoToken.Simbolo, ",");
                        valor += ", ";
                    }
                }
                pulaTokenEsperado(TipoToken.Simbolo, ")");
                expressao += string.Format("({0})", valor);
                return expressao;
            }

            return expressao + parseIn();
        }

        private string parseIn()
        {
            //test_expression [ NOT ] IN ( subquery | expression [ ,...n ])
            string expressao = parseLike();

            expressao += parseNot();

            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "IN"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "IN");
                expressao += " IN ";
                pulaTokenEsperado(TipoToken.Simbolo, "(");
                string valor = "";
                while (!TokenAtual.Equals(TipoToken.Simbolo, ")"))
                {
                    valor += parseExpressao();
                    if (TokenAtual.Equals(TipoToken.Simbolo, ","))
                    {
                        pulaTokenEsperado(TipoToken.Simbolo, ",");
                        valor += ", ";
                    }
                }
                pulaTokenEsperado(TipoToken.Simbolo, ")");
                expressao += string.Format("({0})", valor);
            }
            return expressao;
        }

        private string parseLike()
        {
            //match_expression [ NOT ] LIKE pattern [ ESCAPE escape_character ]
            string expressao = parseBetween();

            expressao += parseNot();

            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "LIKE"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "LIKE");
                expressao += " LIKE ";
                expressao += parseExpressaoAdicao(true);
            }

            return expressao;
        }

        private string parseNot()
        {
            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "NOT"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "NOT");
                return " NOT ";
            }
            return "";
        }

        private string parseBetween()
        {
            string expressao = parseComparacao();

            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "BETWEEN"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "BETWEEN");
                expressao += parseNot();
                expressao += " BETWEEN ";
                expressao += parseComparacao();
                pulaTokenEsperado(TipoToken.PalavraChave, "AND");
                expressao += " AND ";
                expressao += parseComparacao();
            }

            return expressao;
        }

        private string parseComparacao()
        {
            string expressao = parseExpressaoAdicao();

            if (!fimDoSql && TokenAtual.Tipo == TipoToken.Simbolo &&
                (TokenAtual.Valor == "=" || TokenAtual.Valor == "<=" || TokenAtual.Valor == "<" || TokenAtual.Valor == ">=" ||
                 TokenAtual.Valor == ">" || TokenAtual.Valor == "<>"))
            {
                expressao += TokenAtual.Valor;
                leProximoToken();
                expressao += parseExpressaoAdicao();
            }

            return expressao;
        }

        private string parseExpressaoAdicao()
        {
            return parseExpressaoAdicao(false);
        }

        private string parseExpressaoAdicao(bool forcaConcatencao)
        {
            string valor = parseExpressaoBase();
            //{numero | string |coluna | funcao }

            //como o sqlserver utiliza o mesmo caracter '+' para adição em expressões numéricas
            //e para concatenação de strings é preciso marcar o início de expressões de concatenação
            //com duas aspas simples ('') e o início  de expressões numéricas com um zero (0)
            if (valor.Equals("''") || forcaConcatencao)
            {
                empilhaTipoOperacao(TipoOperacao.Concatenacao);
            }
            else if (valor.Equals("0"))
            {
                empilhaTipoOperacao(TipoOperacao.Adicao);
            }
            else
            {
                empilhaTipoOperacao(TipoOperacao.Indefinida);
            }
            while (!fimDoSql && (TokenAtual.Valor == "+" || TokenAtual.Valor == "-" || TokenAtual.Valor == "*" || TokenAtual.Valor == "/"))
            {
                if (TokenAtual.Equals(TipoToken.Simbolo, "+"))
                {
                    pulaTokenEsperado(TipoToken.Simbolo, "+");
                    switch (TipoOperacaoCorrente)
                    {
                        case TipoOperacao.Concatenacao:
                            valor += " || " + parseExpressaoBase();
                            break;
                        case TipoOperacao.Adicao:
                            valor += " + " + parseExpressaoBase();
                            break;
                        default:
                            throw new Exception("Operador '+' foi encontrado mas é ambíguo. É preciso adicioar '' ou 0 no início " +
                                                "da expressão para determinar se é uma expressão de adição ou concatenação");
                    }
                }
                else if(TokenAtual.Equals(TipoToken.Simbolo, "*"))
                {
                    valor += TokenAtual.Valor;
                    //pula o token atual
                    leProximoToken();
                }
                //-(subtração) | *(multiplicação) | /(divisão)
                else
                {
                    valor += " " + TokenAtual.Valor + " ";
                    //pula o token atual
                    leProximoToken();
                    valor += parseColunaOuVariavelOuFuncao();
                }
            }
            desemplilhaTipoOperacao();
            return valor;
            //o token atual deve estar ser: ',' ou 'FROM'
        }

        /// <summary>
        /// Verifica se há um alias ao fim da expressão e o concatena à expressão, caso não haja
        /// retorna a própria expressão inalterada
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private string parseAliasDeColuna(string valor)
        {
            if (TokenAtual.Equals(TipoToken.PalavraChave, "AS"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "AS");
                valor = string.Format("{0} AS {1}", valor, TokenAtual.Valor);
                leProximoToken();
            }
            return valor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private string parseAliasDeTabela(string valor)
        {
            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "AS"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "AS");
                valor = string.Format("{0} {1}", valor, TokenAtual.Valor);
                leProximoToken();
            }
            return valor;
        }

        private string parseSourceTable()
        {
            //table_or_view_name [ [ AS ] table_alias ] [ <tablesample_clause> ] [ WITH ( < table_hint > [ [ , ]...n ] ) ] 

            string tabela = parseAliasDeTabela(parseExpressaoBase());
            string joinedTable = "";

            parseWith();
            parseNoLock();

            while (!fimDoFrom && !TokenAtual.Equals(TipoToken.Simbolo, ","))
            {
                joinedTable += parseJoinedTable();
            }

            if (!fimDoFrom && TokenAtual.Equals(TipoToken.Simbolo, ","))
            {
                pulaTokenEsperado(TipoToken.Simbolo, ",");
                return tabela + ", ";
            }

            return tabela + joinedTable;

        }

        private void parseNoLock()
        {
            if (!fimDoSql && TokenAtual.Equals(TipoToken.Simbolo, "("))
            {
                pulaTokenEsperado(TipoToken.Simbolo, "(");
                pulaTokenEsperado(TipoToken.PalavraChave, "NOLOCK");
                pulaTokenEsperado(TipoToken.Simbolo, ")");
            }
            //if (!fimDoSql && tokenAtual.Equals(TipoToken.PalavraChave, "NOLOCK"))
            //{
            //    pulaTokenEsperado(TipoToken.PalavraChave, "NOLOCK");
            //}
        }

        private void parseWith()
        {
            if (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "WITH"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "WITH");
            }
        }

        private string parseJoinedTable()
        {
            // <table_source> <join_type> <table_source> ON <search_condition> | <table_source> CROSS JOIN <table_source> 
            // | left_table_source { CROSS | OUTER } APPLY right_table_source | [ ( ] <joined_table> [ ) ] 
            string join;
            string tabela;

            if (TokenAtual.Equals(TipoToken.PalavraChave, "CROSS"))
            {
                join = parseJoin();
                tabela = parseAliasDeTabela(parseExpressaoBase());
                return string.Format("{0} {1}", join, tabela);
            }

            join = parseJoin();
            tabela = parseAliasDeTabela(parseExpressaoBase());
            parseWith();
            parseNoLock();
            string expressao = "";
            while (!fimDoSql && TokenAtual.Equals(TipoToken.PalavraChave, "ON"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "ON");

                expressao += " ON " + parseExpressao();
            }

            return string.Format("{0} {1} {2}", join, tabela, expressao);
        }

        private string parseJoin()
        {
            // [ { INNER | { { LEFT | RIGHT | FULL } [ OUTER ] } } [ <join_hint> ] ] JOIN
            string valor = " ";
            if (TokenAtual.Equals(TipoToken.PalavraChave, "CROSS"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "CROSS");
                pulaTokenEsperado(TipoToken.PalavraChave, "JOIN");
                valor += "CROSS JOIN ";
                return valor;
            }
            if (TokenAtual.Equals(TipoToken.PalavraChave, "INNER"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "INNER");
                valor += "INNER ";
            }
            if (TokenAtual.Equals(TipoToken.PalavraChave, "LEFT") ||
                TokenAtual.Equals(TipoToken.PalavraChave, "RIGHT") ||
                TokenAtual.Equals(TipoToken.PalavraChave, "FULL"))
            {
                valor += TokenAtual.Valor;
                leProximoToken();
                if (TokenAtual.Equals(TipoToken.PalavraChave, "OUTER"))
                {
                    pulaTokenEsperado(TipoToken.PalavraChave, "OUTER");
                    valor += " OUTER";
                }

            }
            if (TokenAtual.Equals(TipoToken.PalavraChave, "JOIN"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "JOIN");
                valor += " JOIN";
            }
            return valor;
        }

        private string parseExpressaoBase()
        {
            verificaFimInesperadoDoSql();

            switch (TokenAtual.Tipo)
            {
                case TipoToken.Numero:
                case TipoToken.String:
                    return parseConstate();
                case TipoToken.Variavel:
                case TipoToken.PalavraChave:
                case TipoToken.Palavra:
                    return parseColunaOuVariavelOuFuncao();
                default:
                    if (TokenAtual.Equals(TipoToken.Simbolo, "("))
                    {
                        return parseExpressaoAgrupadora();
                    }
                    if (TokenAtual.Equals(TipoToken.Simbolo, "*"))
                    {
                        return parseColunaOuVariavelOuFuncao();
                    }
                    throw new Exception("Tipo de token não esperado: " + TokenAtual);
            }
        }

        /// <summary>
        /// Trata a utilização de parentesis para agrupar uma expressão
        /// </summary>
        /// <returns></returns>
        private string parseExpressaoAgrupadora()
        {
            pulaTokenEsperado(TipoToken.Simbolo, "(");
            string valor = parseExpressao();
            pulaTokenEsperado(TipoToken.Simbolo, ")");
            return string.Format("({0})", valor);
        }

        private string parseConstate()
        {
            string valor = TokenAtual.Tipo == TipoToken.Numero ? TokenAtual.Valor : string.Format("'{0}'", TokenAtual.Valor);
            //pula a constante já lida e passa para o próximo token
            leProximoToken();
            return valor;
        }

        private string parseColunaOuVariavelOuFuncao()
        {
            string valor = TokenAtual.Valor;
            //caso seja uma variável ou uma coluna o valor já está lido

            if (TokenAtual.Tipo == TipoToken.Variavel)
            {
                valor = ":" + valor;
                leProximoToken();
                return valor;
            }

            leProximoToken();

            if (Operadores.Contains(valor))
            {
                return parseOperador(valor);
            }

            //se o token atual for um '(' então é preciso continuar lendo pois uma chamada a função foi encontrada
            if (TokenAtual != null && TokenAtual.Equals(TipoToken.Simbolo, "("))
            {
                if (proximoTokenIgualA(TipoToken.PalavraChave, "NOLOCK"))
                {
                    parseNoLock();
                    return valor;
                }
                return parseFuncao(valor);
            }

            return valor;
        }

        private string parseOperador(string valor)
        {
            switch (valor)
            {
                case "CASE":
                    return traduzCase();
                default:
                    throw new Exception("Tratamento não implementado para operador: " + valor);
            }
        }

        /// <summary>
        /// Faz o parse da função do SQLServer
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>Função equivalente no Oracle</returns>
        private string parseFuncao(string valor)
        {
            pulaTokenEsperado(TipoToken.Simbolo, "(");
            string funcao = traduzFuncao(valor);
            pulaTokenEsperado(TipoToken.Simbolo, ")");
            return funcao;
        }


        private string traduzCase()
        {
            //Simple CASE expression: 
            //CASE input_expression 
            //     WHEN when_expression THEN result_expression [ ...n ] 
            //     [ ELSE else_result_expression ] 
            //END 

            //Searched CASE expression:
            //CASE
            //     WHEN Boolean_expression THEN result_expression [ ...n ] 
            //     [ ELSE else_result_expression ] 
            //END

            string valor = "CASE ";

            //Simple CASE
            if (!TokenAtual.Equals(TipoToken.PalavraChave, "WHEN"))
            {
                //input_expression
                valor += parseExpressao();
            }

            //WHEN Boolean_expression THEN result_expression [ ...n ] 
            while (TokenAtual.Equals(TipoToken.PalavraChave, "WHEN"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "WHEN");

                //Boolean_expression
                valor += " WHEN " + parseExpressao();

                pulaTokenEsperado(TipoToken.PalavraChave, "THEN");

                //result_expression
                valor += " THEN " + parseExpressao();
            }

            //[ ELSE else_result_expression ] 
            if (TokenAtual.Equals(TipoToken.PalavraChave, "ELSE"))
            {
                pulaTokenEsperado(TipoToken.PalavraChave, "ELSE");

                valor += " ELSE " + parseExpressao();
            }

            //END
            pulaTokenEsperado(TipoToken.PalavraChave, "END");
            valor += " END ";
            return valor;
        }

        #region Métodos de tradução

        /// <summary>
        /// Verifica qual a nome da função e chama o método de conversão (SQLServerr -> Oralce) adequado
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private string traduzFuncao(string valor)
        {
            switch (valor)
            {
                case "GETDATE":
                    return traduzGetDate();
                case "DAY":
                    return traduzDayMonthYear("DD");
                case "MONTH":
                    return traduzDayMonthYear("MM");
                case "YEAR":
                    return traduzDayMonthYear("YYYY");
                case "CAST":
                    return traduzCast();
                case "CONVERT":
                    return traduzConvert();
                case "DATEPART":
                    return traduzDatePart();
                case "CASE":
                    return traduzCase();
                case "ISNULL":
                    return traduzIsNull();
                case "DATEADD":
                    return traduzDateAdd();
                case "MAX":
                case "MIN":
                case "AVG":
                case "COUNT":
                case "SUM":
                case "ABS":
                    return traduzFuncaoAnsiComUmParametro(valor);
                default:
                    throw new Exception("Função não reconhecida:" + valor);
            }
        }

        /// <summary>
        /// Traduz a função DATEADD do SQLServer para o Oralce
        /// </summary>
        /// <returns></returns>
        private string traduzDateAdd()
        {
            //DATEADD (datepart , number, date )
            string datepart = TokenAtual.Valor;
            leProximoToken();
            pulaTokenEsperado(TipoToken.Simbolo, ",");
            string sinal = string.Empty;
            /* Zenildo
             * A criação da variavel 'sinal' é para que a função DataAdd aceite números negativos nos casos de decrementar meses ou anos.
             */
            if (TokenAtual.Valor == "-")
            {
                sinal = TokenAtual.Valor;
                pulaTokenEsperado(TipoToken.Simbolo, "-");
            }

            string number = parseConstate();
            pulaTokenEsperado(TipoToken.Simbolo, ",");
            string date = parseExpressao();

            switch (datepart)
            {
                case "YEAR":
                    return string.Format("ADD_MONTHS({0}, 12*({1}{2}))", date, sinal, number);
                case "MONTH":
                    return string.Format("ADD_MONTHS({0}, {1}{2})", date, sinal, number);
                default:
                    throw new Exception("Parametro inválido para função DATEADD: " + datepart);
            }
        }

        /// <summary>
        /// Converte a função SQLServer ISNULL para a função NVL do Oracle
        /// </summary>
        /// <returns></returns>
        private string traduzIsNull()
        {
            //ISNULL ( check_expression , replacement_value )
            string check_expression = parseExpressao();
            pulaTokenEsperado(TipoToken.Simbolo, ",");
            string replacement_value = parseExpressao();

            return string.Format("NVL({0},{1})", check_expression, replacement_value);
        }

        /// <summary>
        /// Traduz uma função ANSI para Oralce. 
        /// Normalmente nenhuma tradução é necessária para funções ANSI mas o parâmetro
        /// para esta função pode ser uma expressão que exija tradução.
        /// </summary>
        /// <param name="funcao"></param>
        /// <returns></returns>
        private string traduzFuncaoAnsiComUmParametro(string funcao)
        {
            return string.Format("{0}({1})", funcao, parseExpressao());
        }

        /// <summary>
        /// Traduz a função DATEPART do SQLServer para sua equivalente no Oracle
        /// </summary>
        /// <returns></returns>
        private string traduzDatePart()
        {
            //DATEPART ( datepart , date )
            string datepart = parseExpressaoBase();

            pulaTokenEsperado(TipoToken.Simbolo, ",");

            string date = parseExpressao();

            string formato;

            switch (datepart)
            {
                case "DAY":
                    formato = "DD";
                    break;
                case "MONTH":
                    formato = "MM";
                    break;
                case "YEAR":
                    formato = "YYYY";
                    break;
                default:
                    throw new Exception("Parametro inválido para a funçao DATEPART:" + datepart);
            }

            return string.Format("TO_CHAR({0},'{1}')", date, formato);
        }

        /// <summary>
        /// Traduz a função CONVERT do SQLSever para Oracle
        /// </summary>
        /// <returns></returns>
        private string traduzConvert()
        {
            //CONVERT ( data_type [ ( length ) ] , expression [ , style ] )

            //data_type
            string data_type = parseDataType();

            pulaTokenEsperado(TipoToken.Simbolo, ",");

            //expressao
            string valor = parseExpressao();

            string formato = null;

            //possui o bloco de formatação opcional "[, style]"
            if (TokenAtual.Equals(TipoToken.Simbolo, ","))
            {
                pulaTokenEsperado(TipoToken.Simbolo, ",");
                //style
                formato = parseExpressaoBase();
            }

            switch (data_type)
            {
                case "VARCHAR":
                case "CHAR":
                    if (formato != null)
                    {
                        switch (formato)
                        {
                            case "103":
                                valor += ", 'DD/MM/YYYY'";
                                break;
                            default:
                                throw new Exception("Formato não reconhecido em função CONVERT: " + formato);
                        }
                    }
                    return string.Format("TO_CHAR({0})", valor);
                default:
                    throw new Exception("Tipo inválido para função CONVERT: " + data_type);
            }
        }

        /// <summary>
        /// Faz o parse de um Tipo do SQLServer e retorna apenas o tipo ignorando tamanho e precisão
        /// pois para realizar a tradução para o Oracle estas informações não são relevantes
        /// </summary>
        /// <returns></returns>
        private string parseDataType()
        {
            //data_type [ ( length ) ]
            string datatype = TokenAtual.Valor;

            //passa para o próximo caracter
            leProximoToken();

            //se a função CAST definir um tamanho (lenght) para o data_type
            //este código irá ignorar esta declaração pois o Oralce não utiliza esta informação
            if (TokenAtual.Equals(TipoToken.Simbolo, "("))
            {
                //pula o parentese
                pulaTokenEsperado(TipoToken.Simbolo, "(");
                //pula a parte inteira do número
                leProximoToken();
                //pula a parte decimal do número (caso exista)
                if (TokenAtual.Equals(TipoToken.Simbolo, ","))
                {
                    pulaTokenEsperado(TipoToken.Simbolo, ",");
                    leProximoToken();
                }
                //pula o fechamento do parentese
                pulaTokenEsperado(TipoToken.Simbolo, ")");
            }

            return datatype;
        }

        /// <summary>
        /// Traduz a função CAST do SQLServer para a equivalente em Oracle
        /// </summary>
        /// <returns></returns>
        private string traduzCast()
        {
            //CAST ( expression AS data_type [ (length ) ])
            string valor = parseExpressao();

            pulaTokenEsperado(TipoToken.PalavraChave, "AS");

            string datatype = parseDataType();
            string funcao;
            switch (datatype)
            {
                case "VARCHAR":
                case "CHAR":
                    funcao = "TO_CHAR({0})";
                    break;
                case "DATETIME":
                    funcao = "TO_DATE({0},'DD/MM/YYYY')";
                    break;
                case "NUMERIC":
                    funcao = "TO_NUMBER({0})";
                    break;
                default:
                    throw new Exception("Parâmetro inválido para função CAST: " + TokenAtual.Valor);
            }

            return string.Format(funcao, valor);
        }

        /// <summary>
        /// Traduz a função GETDATE() do SQL para SYSDATE no Oracle
        /// </summary>
        /// <returns></returns>
        private string traduzGetDate()
        {
            return "SYSDATE";
        }

        /// <summary>
        /// Parse da função DAY, MONTH, YEAR do SQL
        /// </summary>
        /// <returns></returns>
        private string traduzDayMonthYear(string formato)
        {
            //todo: verificar se precisa converter pra number depois do to_char pois a Day do SQLServer retorna um INT
            string valor = parseExpressao();
            string coluna = string.Format("TO_CHAR({0},'{1}')", valor, formato);
            return coluna;
        }

        #endregion Métodos de tradução

        #region Métodos auxiliares

        /// <summary>
        /// Verifica se chegou ao fim da cláusula SELECT
        /// </summary>
        private bool fimDoSelect
        {
            get { return TokenAtual.Equals(TipoToken.PalavraChave, "FROM"); }
        }

        /// <summary>
        /// Verifica se chegou ao fim da cláusula SELECT
        /// </summary>
        private bool fimDoFrom
        {
            get
            {
                return fimDoSql ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "WHERE") ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "GROUP") ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "ORDER") ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "UNION");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool fimDoGroupBy
        {
            get
            {
                return fimDoSql ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "HAVING") ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "ORDER") ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "UNION");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool fimDoHaving
        {
            get
            {
                return fimDoSql ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "ORDER") ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "UNION");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool fimDoOrderBy
        {
            get
            {
                return fimDoSql ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "UNION");
            }
        }
        /// <summary>
        /// Verifica se todos os tokens já foram lidos
        /// </summary>
        private bool fimDoSql
        {
            get { return TokenAtual == null || TokenAtual.Equals(TipoToken.Simbolo, ";"); }
        }

        /// <summary>
        /// Verifica se chegou ao fim de uma expressão de coluna
        /// </summary>
        private bool fimExpressaoColuna
        {
            get
            {
                return fimDoSelect ||
                       TokenAtual.Equals(TipoToken.Simbolo, ",") ||
                       TokenAtual.Equals(TipoToken.PalavraChave, "AS");
            }
        }

        /// <summary>
        /// Esta função é chamada em momentos onde é necessário garantir que ainda
        /// não se chegou ao fim do sql. Caso o fim do sql tenha sido encontrado uma
        /// exceção é lançada.
        /// </summary>
        private void verificaFimInesperadoDoSql()
        {
            if (fimDoSql)
            {
                throw new Exception("Fim inesperado do sql");
            }
        }

        /// <summary>
        /// Este método verifica se o token atual é realmente o informado através dos parâmetros
        /// deste método e caso seja irá ignorar este token e passar para o token seguinte
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="valor"></param>
        private void pulaTokenEsperado(TipoToken tipo, string valor)
        {
            verificaFimInesperadoDoSql();
            if (!TokenAtual.Equals(tipo, valor))
            {
                throw new Exception(string.Format("Valor esperado: '{0}' diferente do encontrado:{1}", valor, TokenAtual.Valor));
            }
            leProximoToken();
        }

        /// <summary>
        /// Lê o próximo token da fila e o coloca como o "tokenAtual"
        /// </summary>
        private void leProximoToken()
        {
            TokenAtual = Tokens.Count > 0 ? Tokens.Dequeue() : null;
            if (TokenAtual != null && (TokenAtual.Equals(TipoToken.Simbolo, "[") || TokenAtual.Equals(TipoToken.Simbolo, "]")))
            {
                leProximoToken();
            }
        }

        private bool proximoTokenIgualA(TipoToken tipo, string valor)
        {
            if (Tokens.Count > 0)
            {
                Token tk = Tokens.Peek();
                return tk.Equals(tipo, valor);
            }
            return false;
        }

        #endregion

    }
}
