using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace TYAP_SH
{
    internal class Translator
    {
        List<string> KeyWords = new List<string>();
        List<string[]> replaceruleslist = new List<string[]>();
        List<string> Vars = new List<string>();
        List<string> neterminals;
        List<string[]> ruls = new List<string[]>();
        Dictionary<string,int?> varsWithVals=new Dictionary<string, int?>();
        public Translator()
        {
            string[][] tmp = {
                new string[]{ "<E>", "<S>"},
                new string[]{ "<E>", "<S><E>"},
                new string[]{ "<Y>","\\-" },
                new string[]{ "<J>", "\\-"},
                new string[]{ "<J>", "\\+"},
                new string[]{ "<J>", "\\*"},
                new string[]{"<I>","<A><B>"},
                new string[] { "<B>", "BEGIN<C>END"},
                new string[]{ "<A>", "VAR<D>:INTEGER;" },
                new string[]{ "<D>", "<E>"},
                new string[] { "<D>", "<E>,<D>"},
                new string[] { "<C>", "<F>"},
                new string[]{ "<C>", "<F>;<C>"},
                new string[]{ "<F>", "<E>=<G>"},
                new string[]{ "<F>", "READ\\(<D>\\)"},
                new string[]{ "<F>", "WRITE\\(<D>\\)"},
                new string[]{ "<F>", "FOR<E>=<G>TO<G>DO<F>END_FOR"},
                new string[]{ "<G>", "<Y><U>"},
                new string[]{ "<G>", "<U>"},
                new string[]{ "<O>", "<E>"},
                new string[]{ "<O>", "<K>"},
                new string[]{ "<U>", "\\(<G>\\)"},
                new string[]{ "<U>", "<O>"},
                new string[]{ "<U>", "<U><J><U>"}
                
            };
            for (int i = 97; i < 123; i++)
                ruls.Add(new string[] { "<S>", char.ConvertFromUtf32(i) });
            for (int i = 48; i < 58; i++)
                ruls.Add(new string[] { "<K>", char.ConvertFromUtf32(i) });
            for (int i = 0; i < tmp.Length; i++)
                ruls.Add(tmp[i]);
            

            
            
            replaceruleslist.Add(new string[]{ "<S>","[a-z]"});
            replaceruleslist.Add(new string[] { "<K>", "[0-9]+" });
            replaceruleslist.Add(new string[] { "<J>", "(\\+|\\-|\\*)" });
            replaceruleslist.Add(new string[] { "<E>", "[a-z]+" });
            replaceruleslist.Add(new string[] { "<Y>", "(\\-)" });
            replaceruleslist.Add(new string[] { "<G>", "([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+" });
            replaceruleslist.Add(new string[] { "<U>", "([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+" });
            string[][] tmpList ={
            new string[] { "<O>", "(<E>|<K>)" },
            new string[] { "<D>", "(<E>(,<E>)*)" },
            new string[] { "<F>", "(READ\\(<D>\\)|WRITE\\(<D>\\)|(<E>=<G>)|FOR<E>=<G>TO<G>DO<F>(;<F>)*END_FOR)" },
            new string[] { "<C>", "<F>(;<F>)*" },
            new string[] { "<A>", "VAR<D>:INTEGER;" },
            new string[] { "<B>", "BEGIN<C>END" },
            new string[] { "<I>", "<A><B>" }
            };
            int levlRec = 1;
            for (int i = 0; i < levlRec; i++)
                tmpList[2][1] = tmpList[2][1].Replace(tmpList[2][0], tmpList[2][1]);
            tmpList[2][1] = tmpList[2][1].Replace(tmpList[2][0], tmpList[2][1].Replace(tmpList[2][0],".*"));
            for (int i = 0; i < tmpList.Length; i++)
            {
                string[] tmprul =tmpList[i];
                int end = replaceruleslist.Count();
                for (int j = 0; j < end; j++)
                {
                    tmprul[1]=tmprul[1].Replace(replaceruleslist.ElementAt(j)[0], replaceruleslist.ElementAt(j)[1]);
                }
                replaceruleslist.Add(tmprul);
                end++;
            }

        }
        public void Tranclste(string program)
        {
            string lecsems = LecsicalAnalyze(program);
            if (!UpDownParse(lecsems)) throw new Exception("синтаксическая ошибка");
            string comands =lecsems.Substring(lecsems.IndexOf("BEGIN") + 5);
            comands = comands.Remove(comands.LastIndexOf("END"));
            Execute(comands.Split(';'));



        }
        string[][] Execute(string[] comands)
        {
            string[][] logs=new string[comands.Length][];
            for (int i = 0; i < comands.Length; i++)
            {
                if (comands[i].Contains("READ"))
                {
                    string vars = comands[i].Remove(0, 5).Replace(")", "");
                    string[] varToRead = vars.Split(',');
                    CheckVars(varToRead);
                    string valsTmp = Console.ReadLine();
                    string[] valstmp = valsTmp.Trim(' ').Replace("  ", " ").Split(' ');
                    if (valstmp.Length != varToRead.Length) throw new Exception("введено неверное количество переменных");
                    int j = 0;
                    try
                    {
                        for (j = 0; j < varToRead.Length; j++)
                        {

                            varsWithVals.Remove(varToRead[j]);
                            varsWithVals.Add(varToRead[j], Int32.Parse(valstmp[j]));
                        }

                    }
                    catch (FormatException)
                    {
                        throw new Exception($"неккоректно введённое значение -> '{valstmp[j]}'");
                    }
                    logs[i] = new string[1];
                    logs[i][0] = comands[i] + "Выполнено успешно";
                }
                else if (comands[i].Contains("WRITE"))
                {
                    string vars = comands[i].Remove(0, 6).Replace(")", "");
                    string[] varToWrite = vars.Split(',');
                    CheckVars(varToWrite);
                    string stToWrite = "";
                    for (int j = 0; j < varToWrite.Length; j++)
                    {
                        int? val = null;
                        varsWithVals.TryGetValue(varToWrite[j], out val);
                        if (val == null) throw new Exception("Значение переменной " + varToWrite[j] + " не присвоено");
                        stToWrite = stToWrite + val + " ";
                    }
                    Console.WriteLine(stToWrite);
                    logs[i] = new string[1];
                    logs[i][0] = comands[i] + "Выполнено успешно";
                }
                else if (comands[i].Contains("FOR"))
                {
                    string tmpComand = comands[i];
                    while ((tmpComand.Length - tmpComand.Replace("FOR", "").Length) / ("FOR".Length*2) != (tmpComand.Length - tmpComand.Replace("END_FOR", "").Length) / "END_FOR".Length)
                    {
                        tmpComand += comands[++i];
                    }
                    string comandstmp = comands[i].Remove(0, 3);
                    comandstmp = comandstmp.Remove(comandstmp.Length - 7, 7);
                    string eq = comandstmp.Substring(0, comandstmp.IndexOf("TO"));
                    string[] tmp = eq.Split('=');
                    CheckVars(new string[] { tmp[0] });
                    string varName = tmp[0].Replace(" ", "");
                    string[] varsToReplace = Regex.Split(tmp[1], "[^a-z]");
                    CheckVars(varsToReplace);
                    for (int j = 0; j < varsToReplace.Length; j++)
                    {

                        if (Regex.IsMatch(varsToReplace[j], "^[a-z]$"))
                        {
                            int? val = null;
                            varsWithVals.TryGetValue(varsToReplace[j], out val);
                            if (val == null) throw new Exception("Значение переменной " + varsToReplace[j] + " не присвоено");
                            tmp[1]=tmp[1].Replace(varsToReplace[j], val.ToString());
                        }
                    }
                    int tmpVarVal = Convert.ToInt32(new DataTable().Compute(tmp[1], ""));
                    varsWithVals.Remove(varName);
                    varsWithVals.Add(varName, tmpVarVal);
                    comandstmp = comandstmp.Remove(0, comandstmp.IndexOf("TO") + 2);
                    eq = comandstmp.Substring(0, comandstmp.IndexOf("DO"));
                    string[] varsToReplaceNew = Regex.Split(eq, "[^a-z]");
                    CheckVars(varsToReplaceNew);
                    for (int j = 0; j < varsToReplaceNew.Length; j++)
                    {

                        if (Regex.IsMatch(varsToReplaceNew[j], "^[a-z]$"))
                        {
                            int? val = null;
                            varsWithVals.TryGetValue(varsToReplaceNew[j], out val);
                            if (val == null) throw new Exception("Значение переменной " + varsToReplaceNew[j] + " не присвоено");
                            eq = eq.Replace(varsToReplaceNew[j], val.ToString());
                        }
                    }
                    int tmpVarValnew = Convert.ToInt32(new DataTable().Compute(eq, ""));
                    int? a = null;
                    varsWithVals.TryGetValue(varName, out a);
                    comandstmp = comandstmp.Remove(0, comandstmp.IndexOf("DO") + 2);
                    while (a < tmpVarValnew)
                    {
                        Execute(comandstmp.Split(';'));
                        varsWithVals.Remove(varName);
                        varsWithVals.Add(varName, ++a);
                    }
                    //string[][] logsnew; 
                    //logs[i] = new string[tmp.Length + 1];
                    //for (int j = 0; j < tmp.Length; j++)
                    //    logs[i][j] = tmp[j][0];
                    logs[i] = new string[1];
                    logs[i][0] = tmpComand + "Выполнено успешно";
                }
                else if (comands[i].Contains("="))
                {
                    ExecEquals(comands[i]);
                    logs[i] = new string[1];
                    logs[i][0] = comands[i] + "Выполнено успешно";
                }
                else
                {
                    throw new Exception("Неопознаный оператор");
                }
            }
            return logs;
        }
        bool CheckVars(string[] vars)
        {
            for (int i = 0; i<vars.Length; i++)
            {
                if (!Vars.Contains(vars[i])&& vars[i]!="")
                throw new Exception("Необявленная переменная-> "+ vars[i]);
            }
            return true;
        }
        void ExecEquals(string comand)
        {
            string[] tmp = comand.Split('=');
            string varName = tmp[0].Replace(" ", "");
            CheckVars(new string[] { tmp[0] });
            string[] varsToReplace = Regex.Split(tmp[1], "[^a-z]");
            CheckVars(varsToReplace);
            for (int j = 0; j < varsToReplace.Length; j++)
            {

                if (Regex.IsMatch(varsToReplace[j], "^[a-z]$"))
                {
                    int? val = null;
                    varsWithVals.TryGetValue(varsToReplace[j], out val);
                    if (val == null) throw new Exception("Значение переменной " + varsToReplace[j] + " не присвоено");
                    tmp[1]=tmp[1].Replace(varsToReplace[j], val.ToString());
                }
            }
            int tmpVarVal = Convert.ToInt32(new DataTable().Compute(tmp[1], ""));
            varsWithVals.Remove(varName);
            varsWithVals.Add(varName, tmpVarVal);
        }
        string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        string LecsicalAnalyze(string program)
        {
            List<string> tmp = new List<string>();
            int indEnd = program.IndexOf("INTEGER");
            if (program.IndexOf("VAR") < 0) throw new Exception("Не найдено ключевое слово VAR"); else KeyWords.Add("VAR");
            if (indEnd < 0) throw new Exception("Неккоректный тип переменных");
            if (program.IndexOf("BEGIN") < 0) throw new Exception("Не найдено ключевое слово BEGIN"); else KeyWords.Add("BEGIN");
            if (program.IndexOf("END") < 0) throw new Exception("Не найдено ключевое слово END"); else KeyWords.Add("END");
            if (program.IndexOf("FOR") >= 0) {
                if (program.IndexOf("TO") < 0) throw new Exception("Не найдено ключевое слово TO");
                if (program.IndexOf("DO") < 0) throw new Exception("Не найдено ключевое слово DO");
                if (program.IndexOf("END_FOR") < 0) throw new Exception("Не найдено ключевое слово END_FOR");
                KeyWords.Add("FOR"); KeyWords.Add("TO"); KeyWords.Add("DO"); KeyWords.Add("END_FOR");
            }

            if (program.IndexOf("READ") >= 0) KeyWords.Add("READ");
            if (program.IndexOf("WRITE") >= 0) KeyWords.Add("WRITE");
            string vars = program.Substring(3, indEnd - 4).Replace(" ", "");
            string[] varsNames = vars.Split(',');
            for (int i = 0; i < varsNames.Length; i++)
            {
                if (!Regex.IsMatch(varsNames[i], "[a-z]")) throw new Exception("Неккоректно написано название переменной -> " + varsNames[i]);
                if (varsNames[i].Length > 12) throw new Exception("Неккоректная длина переменной -> " + varsNames[i]);
            }
            for (int i = 0; i < varsNames.Length; i++)
            {
                for (int j = i + 1; j < varsNames.Length; j++)
                {
                    if (varsNames[i] == varsNames[j]) throw new Exception("Дублирующиеся названия переменных -> " + varsNames[i]);
                }
                varsWithVals.Add(varsNames[i],null);
                Vars.Add(varsNames[i]);
            }
            return program.Replace(" ", "");
        }

        bool UpDownParse(string program)
        {
            return UpDown("<I>", program);
        }
        string replaceRules(string program)
        {
            for(int i = 0; i < replaceruleslist.Count(); i++)
            {
                program = program.Replace(replaceruleslist.ElementAt(i)[0], replaceruleslist.ElementAt(i)[1]);
            }
            return program;
        }

        bool UpDown(string tmp_program,string program) 
        { 
            string test ="^"+replaceRules(tmp_program)+"$";
            if(!Regex.IsMatch(program, test))return false;
            for (int i = 0; i < ruls.Count(); i++) 
            { 
                string tmp = ReplaceFirst(tmp_program, ruls[i][0], ruls[i][1]);
                if (tmp != tmp_program)
                    if(!UpDown(tmp, program))
                    {
                        while (i + 1 != ruls.Count() && ruls[i + 1][0] == ruls[i][0])
                        {
                            i++;
                            if (UpDown(ReplaceFirst(tmp_program, ruls[i][0], ruls[i][1]), program))
                                return true;
                            
                        }
                        return false;
                    }  
                    else return true;
            }

        return (Regex.IsMatch(program, test)&& !tmp_program.Contains("<"));
        }

    }
}
//"VARa,b,c:INTEGER;BEGINa=1;READ(b,c);WRITE(a,b);FORa=-2TObDOc=-(a*b-1+2)END_FOREND"
//(READ\(([a-z]+(,[a-z]+)*)\)|WRITE\(([a-z]+(,[a-z]+)*)\)|([a-z]+=([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+TO([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+DO(READ\(([a-z]+(,[a-z]+)*)\)|WRITE\(([a-z]+(,[a-z]+)*)\)|([a-z]+=([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+TO([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+DO(READ\(([a-z]+(,[a-z]+)*)\)|WRITE\(([a-z]+(,[a-z]+)*)\)|([a-z]+=([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+TO([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+DO(READ\(([a-z]+(,[a-z]+)*)\)|WRITE\(([a-z]+(,[a-z]+)*)\)|([a-z]+=([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+TO([0-9]|[a-z]|[\+]|[\-]|[\*]|[\(]|[\)])+DO.*END_FOR)END_FOR)END_FOR)END_FOR)END
//VARa,b,c:INTEGER;BEGINa=1;READ(<D>);<F>;<F>END
//"^VARa,b,c:INTEGER;BEGINa=1;READ(([a-z]+(,[a-z]+)*));(READ\\(([a-z]+(,[a-z]+)*)\\)|WRITE\\(([a-z]+(,[a-z]+)*)\\)|([a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+TO([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+DO(READ\\(([a-z]+(,[a-z]+)*)\\)|WRITE\\(([a-z]+(,[a-z]+)*)\\)|([a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+TO([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+DO(READ\\(([a-z]+(,[a-z]+)*)\\)|WRITE\\(([a-z]+(,[a-z]+)*)\\)|([a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+TO([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+DO(READ\\(([a-z]+(,[a-z]+)*)\\)|WRITE\\(([a-z]+(,[a-z]+)*)\\)|([a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+TO([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+DO.*END_FOR)END_FOR)END_FOR)END_FOR);(READ\\(([a-z]+(,[a-z]+)*)\\)|WRITE\\(([a-z]+(,[a-z]+)*)\\)|([a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+)|FOR[a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[..."