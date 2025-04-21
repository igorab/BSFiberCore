namespace BSFiberCore.Models
{
    public class Fiber
    {
        public int Id { get; set; }
        public string FiberQ { get; set; }        
        public string FiberAns { get; set; }

        // размеры
        public double Length { get; set; }

        public double Width { get; set; }

        // класс бетона
        public string Bft3 { get; set; }

        public string Bft { get; set; }

        public string Bf { get; set; }

        // усилия внешние
        public double My { get; set; }

        public double N { get; set; }

        public double Qx { get; set; }

        // арматура
        public double As { get; set; }

        public double A1s { get; set; }

        public double a_cm { get; set; }

        public double a1_cm { get; set; }


        public Fiber()
        {
                
        }
    }
}
