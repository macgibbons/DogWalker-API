using System;
using System.Collections.Generic;
using System.Text;

namespace DogWalkerAPI.Models

{
    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }
        public String Breed { get; set; }
        public string Notes { get; set; }
        public OWNER Owner { get; set; }
        public void walk(Walker walker, OWNER Owner, DateTime date, int duration)
        {
            Console.WriteLine($"{walker.Name} took on a {duration} minute walk.");
        }
    }
}
