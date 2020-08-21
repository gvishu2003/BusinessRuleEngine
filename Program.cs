using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRE
{
    public class Program
    {
        static void Main(string[] args)
        {
        var rule = BusinessEngineService.LoadFromString(@"
        name: BR2
        rules:
            membership upgrade
        actions:
            membership activate upgrade
        ");
            Console.ReadKey();
        }
    }
}
