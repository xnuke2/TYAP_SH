using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TYAP_SH
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //"VAR a,b,c:INTEGER; BEGIN ab =1;READ(b,c);WRITE(a,b);FOR a=-2 TO b DO c=(-a*b-1+2) END_FOR END"
            Translator translator = new Translator();
            Console.WriteLine("Введите программу");
            translator.Tranclste(Console.ReadLine());
            Console.ReadLine();
        }
    }
}
