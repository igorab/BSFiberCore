
using BSFiberCore.Models.BL;
using System.Drawing.Text;

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
            FiberQ = "";
            FiberAns = "";
            Bft3 = "";
            Bft = "";
            Bf = "";
        }

        internal void RunCalc()
        {
            double Efb = 2141404.0200;

            double Yft = 1.3, Yb = 1.3, Yb1 =  0.9, Yb2 = 0.9, Yb3 = 1, Yb5 = 1;

            var MatFiber = new BL.Mat.BSMatFiber(Efb, Yft, Yb, Yb1, Yb2, Yb3, Yb5);
            MatFiber.B = 30;

            MatFiber.Rfbt3n = 35.69;
            MatFiber.Rfbt2n = 39.67;
            MatFiber.Rfbtn = 30.59;
            MatFiber.Rfbn = 188.65;
            

            BSFiberMain fiberMain = new BSFiberMain()
            {
                UseReinforcement = false,
                BeamSection = BL.Beam.BeamSection.Rect,
                MatFiber = MatFiber
            };

            double[] prms = { Yft, Yb, Yb1, Yb2, Yb3, Yb5 };
            double[] sz = {Length, Width, 0};

            fiberMain.FiberCalculate_M(My, prms, sz);
        }
    }
}
