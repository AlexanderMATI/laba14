using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace laba14
{
    public class Calc
    {
        ~Calc()
        {
            _sw.Close();
        }
        private SortedSet<string> _operations;
        private List<string> _uniqueVarsSet;
        private List<List<String>> _examples;
        private StreamWriter _sw;
        public Calc(string fileName)
        {
            _sw = new StreamWriter(fileName);
            _operations = new SortedSet<string> {"!", "*", "→", "+", "^", "~", "-", "*", "/"};
            _examples = new List<List<string>>();
            GetExamplesFromFile();
        }

        public void CalculateExampleNo(int equationNumber = 0)
        {
            var first = 0;
            var last = _examples.Count - 1;

            bool isCorrectNum = (equationNumber > 0 && equationNumber <= last);
            if (isCorrectNum)
                first = last = equationNumber - 1;

            for (; first <= last; first++)
            {
                Console.WriteLine("выражение: ");
                _sw.Write("выражение: ");
                foreach (var k in _examples[first])
                {
                    Console.Write(k + " ");
                    _sw.Write(k + " ");
                }
                _sw.WriteLine();

                var garbageSymbols = new List<string> { "(", ")", ";", "1", "0" };

                _uniqueVarsSet = new List<string>(_examples[first]);
                _uniqueVarsSet = _uniqueVarsSet.Distinct().ToList();
                _uniqueVarsSet = _uniqueVarsSet.Except(_operations.ToList()).ToList();
                _uniqueVarsSet = _uniqueVarsSet.Except(garbageSymbols.ToList()).ToList();
                
                var postfix = GetPostfix(_examples[first]);

                Console.WriteLine("выражение в постфиксе:");
                foreach (var j in postfix)
                {
                    Console.WriteLine(j + " ");
                }
                CalcExample(postfix);
            }
        }

        private int OperPrior(string oper)
        {
           
            switch (oper)
            {
                case "!":
                    return 5;
                case "*":
                    return 4;
                case "+":
                    return 3;
                case "^":
                    return 3;
                case "→":
                    return 2;
                case "~":
                    return 1;
            }
            return 0;
        }

        private string GetSDNF(IReadOnlyDictionary<string, List<bool>> table)
        {
            string SDNF = "";
            var result = table.Last().Value;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i])
                {
                    SDNF += "(";
                    for (int j = 0; j < _uniqueVarsSet.Count; j++)
                    {
                        bool isFuncTrue = !table[_uniqueVarsSet[j]][i];
                        if (isFuncTrue) 
                        {
                            SDNF += _uniqueVarsSet[j]+"*";
                        }
                        else
                        {
                            SDNF += "!"+_uniqueVarsSet[j] + "*";
                        }
                    }
                    SDNF=SDNF.Remove(SDNF.Length-1);
                    SDNF += ")+";
                }
                Console.WriteLine(SDNF.Remove(SDNF.Length - 1));
            }
            SDNF = SDNF.Remove(SDNF.Length-1); 
            return SDNF;
        }

        private string GetSKNF(IReadOnlyDictionary<string, List<bool>> table)
        {
          string SKNF = "";
            var result = table.Last().Value;
            for (int i = 0; i < result.Count; i++)
            {
                if (!result[i])
                {
                    SKNF += "(";
                    for (int j = 0; j < _uniqueVarsSet.Count; j++)
                    {
                        bool isFuncFalse = !table[_uniqueVarsSet[j]][i];
                        if (isFuncFalse) 
                        {
                            SKNF += _uniqueVarsSet[j]+ "+";
                        }
                        else
                        {
                            SKNF += "!"+_uniqueVarsSet[j] + "+";
                        }
                    }

                    SKNF=SKNF.Remove(SKNF.Length-1);
                    SKNF += ")*";

                }

                Console.WriteLine(SKNF);
            }
            SKNF= SKNF.Remove(SKNF.Length-1); 
            return SKNF;
        }



        private void GetExamplesFromFile()
        {
            string fileIN = "../../../in.txt";
            char[] delSet = { ' ', '\n', '\t' };
            
            var sr = new StreamReader(fileIN);

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();

                line = line.Replace("  ", " ");

                if (line.Length == 0)
                    continue;

                var splittedExample = new List<string>(line.Split(delSet));
                _examples.Add(splittedExample);
            }
            sr.Close();
        }

        private IEnumerable<String> GetPostfix(IEnumerable<string> Example)
        {
            var PostExample=new List<string>();

            var stack = new Stack<string>();
            
            foreach (var i in Example)
            {
                if (i == ";")
                {
                    while (stack.Count != 0)
                    {
                        PostExample.Add(stack.Pop());
                    }
                    continue;
                }

                if (i == ")")
                {
                    while (stack.First() != "(")
                    {
                        PostExample.Add(stack.Pop());
                    }
                    stack.Pop();
                    continue;
                }

                if (i == "(")
                {
                    stack.Push(i);
                    continue;
                }

                if (!_operations.Contains(i) && i != "(" && i != ")") 
                {
                    PostExample.Add(i);
                    continue;
                }

                if (_operations.Contains(i)) 
                {
                    if (stack.Count == 0 || stack.First() == "(")
                    {
                        stack.Push(i);
                        continue;
                    }

                    if (OperPrior(i) > OperPrior(stack.First()))
                    {
                        stack.Push(i);
                        continue;
                    }

                    if (OperPrior(i) <= OperPrior(stack.First()))
                    {

                        bool isLowerPriority = (i != "(") && stack.Count != 0 &&
                                               OperPrior(i) <= OperPrior(stack.First());
                        while (isLowerPriority)
                        {
                            if (stack.Count != 0) PostExample.Add(stack.Pop());
                            isLowerPriority = (i != "(") && stack.Count != 0 &&
                                              OperPrior(i) <= OperPrior(stack.First());
                        }

                        stack.Push(i);
                    }
                }
            }
            return PostExample;
        }

        private void CalcExample(IEnumerable<string> example)
        {
            var num = 0;
            var VarCount = _uniqueVarsSet.Count;
            var table = new Dictionary<string, List<bool>>();
            
            for (int i = 0; i < VarCount; i++)
            {
                var col = new List<bool>();

                int k = 0;
                
                bool defenition = false;
               
                for (int j = 0; j < Math.Pow(2, VarCount); j++, k++)
                {
                    if (k >= Math.Pow(2, VarCount - i - 1))
                    {
                        defenition = !defenition;
                        k = 0;
                    }
                    col.Add(defenition);
                }
                table.Add(_uniqueVarsSet[i], col);
            }

            //подсчет постфикса
            var stack = new Stack<List<bool>>();
            
            foreach (var m in example)
            {
                if (!_operations.Contains(m))
                {
                    if (m.Contains('1'))
                    {
                        stack.Push(Enumerable.Repeat(true, (int)Math.Pow(2, VarCount)).ToList()); 
                    }
                    else if(m.Contains('0'))
                    {
                        stack.Push(Enumerable.Repeat(false, (int)Math.Pow(2, VarCount)).ToList()); 
                    }
                    else
                    stack.Push(table[m]);
                }

                else
                {
                    List<bool> b = stack.Pop();
                    if (m == "!") 
                    {
                        stack.Push(CalculateOper(m, b));
                        table.Add((++num).ToString(), stack.First());
                    }
                    else 
                    {
                        List<bool> a = stack.Pop();
                        stack.Push(CalculateOper(m, a, b));
                        table.Add((++num).ToString(), stack.First());
                    }
                }
              
            }
            var keys = new List<string>(table.Keys);
            
            for (var j = 0; j < keys.Count; j++)
                {

                Console.Write(" |"+keys[j].PadLeft(2));
            }
            Console.WriteLine(" |");
            Console.WriteLine();
            for (var i = 0; i < table.Last().Value.Count; i++)
            {
                for (var j = 0; j < keys.Count; j++)
                {
                    Console.Write(" |"+Convert.ToByte(table[keys[j]][i]).ToString().PadLeft(2));
                }
               Console.WriteLine(" |");
            }
            Console.WriteLine();
            var SDNf = GetSDNF(table);
            var SKNf = GetSKNF(table);

            _sw.WriteLine("СКНФ: " + SKNf);
            _sw.WriteLine("СДНФ: " + SDNf);
            Console.WriteLine("СДНФ: "+SDNf);
            Console.WriteLine("СКНФ: " + SKNf);
            Console.WriteLine("");
        }

        List<bool> CalculateOper(string oper, List<bool> a, List<bool> b = null)
        {
            var result = new List<bool>();

            switch (oper)
            {
                
                case "!":
                    for (int i = 0; i < a.Count; i++)
                    {
                        result.Add(!a[i]);
                    }
                    break;
                case "*":
                    for (int i = 0; i < a.Count; i++)
                    {
                        result.Add(a[i] & b[i]);
                    }
                    break;
                case "+":
                    for (int i = 0; i < a.Count; i++)
                    {
                        result.Add(a[i] | b[i]);
                    }
                    break;
                case "→":
                    for (int i = 0; i < a.Count; i++)
                    {
                        result.Add(!a[i] | b[i]);
                    }
                    break;
                case "~":
                    for (int i = 0; i < a.Count; i++)
                    {
                        result.Add(!a[i] & !b[i] | a[i] & b[i]);
                    }
                    break;
                case "^":
                    for (int i = 0; i < a.Count; i++)
                    {
                        result.Add(!(!a[i] & !b[i] | a[i] & b[i]));
                    }
                    break;
            }
            return result;
        }
    }
}
