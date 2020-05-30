using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laba14
{
    class Program
    {
        static void Main(string[] args)
        {
            Calc g = new Calc("out.txt");
            g.CalculateExampleNo();
            Console.ReadKey();
        }
    }
}
