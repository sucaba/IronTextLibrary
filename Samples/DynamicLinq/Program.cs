using System;
using System.Linq;
using IronText.Framework;

namespace Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var employees = new Employee[]
            {
                new Employee { Id = 10, FirstName = "John",     LastName = "Doe",     Age = 25 },
                new Employee { Id = 20, FirstName = "Samantha", LastName = "Wells",   Age = 35 },
                new Employee { Id = 30, FirstName = "Michel",   LastName = "Jackson", Age = 45 },
            };

            var query = DynamicLinqCompiler.Compile<Func<IQueryable<string>>>(
                            "from e in $0 where e.Age > $1 select e.FirstName",
                            employees.AsQueryable(), //.AsQueryable() is optional in this case
                            30);

            foreach (var name in query())
            {
                Console.WriteLine(name);
            }

            
            // Should produce:
            //   Samantha
            //   Michel
        }
    }
}
