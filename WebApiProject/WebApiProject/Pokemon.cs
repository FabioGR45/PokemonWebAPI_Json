using System;

namespace WebApiProject
{
    public class Pokemon
    {

        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Type { get; set; } = String.Empty;
        public string Region { get; set; } = String.Empty;
        public int Hp { get; set; } = 0;
        public int Attack { get; set; } = 0;

    }
}
