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
            //if (Regex.IsMatch("BEGINa=1END", "^BEGIN(READ\\((([a-z]+(,[a-z]+)+))\\)|WRITE\\((([a-z]+(,[a-z]+)+))\\)|([a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+))(;(READ\\((([a-z]+(,[a-z]+)+))\\)|WRITE\\((([a-z]+(,[a-z]+)+))\\)|([a-z]+=([0-9]|[a-z]|[\\+]|[\\-]|[\\*]|[\\(]|[\\)])+)))*END$"))
            //    Console.WriteLine("yes");
            //else Console.WriteLine("no");
            if (Regex.IsMatch("READ(b,c)", "^READ\\(([a-z]+(,[a-z]+)*)\\)$"))
                Console.WriteLine("yes");
            else Console.WriteLine("no");
            //"VAR a,b,c:INTEGER; BEGIN a =1;READ(b,c);WRITE(a,b);FOR a=-2 TO b DO c=-a*b-1+2) END_FOR END"
            Translator translator = new Translator();
            translator.Tranclste("VAR a,b,c:INTEGER; BEGIN a =1;READ(b,c);WRITE(a,b);FOR a=-2 TO b DO c=(-a*b-1+2) END_FOR END");
        }
    }
}
