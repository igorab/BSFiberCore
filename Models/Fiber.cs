
using BSFiberCore.Models.BL;
using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Rep;
using System.Drawing.Text;

namespace BSFiberCore.Models
{
    public class Fiber
    {
        public int Id { get; set; }
        public string FiberQ { get; set; }        
        public string FiberAns { get; set; }

        // размеры
        public int SectionType { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }

        public double bf { get; set; }
        public double hf { get; set; }
        public double bw { get; set; }
        public double hw { get; set; }
        public double b1f { get; set; }
        public double h1f { get; set; }

        public double R2 { get; set; }
        public double R1 { get; set; }


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

        public string A_Rs { get; set; }

        public string A_Rsc { get; set; }

        public double Rs { get; set; }

        public double Rsc { get; set; }

        public double Es { get; set; }


        public Fiber()
        {
            FiberQ = "";
            FiberAns = "";
            Bft3 = "";
            Bft = "";
            Bf = "";
        }

        /// <summary>
        ///  Расчеты по методу предельных усилий
        /// </summary>
        internal string RunCalc()
        {
            double Efb = 2141404.0200;

            double Yft = 1.3, Yb = 1.3, Yb1 =  0.9, Yb2 = 0.9, Yb3 = 1, Yb5 = 1;

            var MatFiber = new BL.Mat.BSMatFiber(Efb, Yft, Yb, Yb1, Yb2, Yb3, Yb5)
            {
                B = 30,
                Rfbt3n = 35.69,
                Rfbt2n = 39.67,
                Rfbtn = 30.59,
                Rfbn = 188.65
            };

            List<BSFiberReportData> calcResults_MNQ = new List<BSFiberReportData>();

            bool use_reinforcement = As > 0 || A1s > 0;

            BSFiberMain fiberMain = new BSFiberMain()
            {
                UseReinforcement = use_reinforcement,
                BeamSection = (BeamSection)SectionType,
                MatFiber = MatFiber
            };
            
            fiberMain.Fiber = this;            

            double[] prms = { Yft, Yb, Yb1, Yb2, Yb3, Yb5 };
            
            // расчет на чистый изгиб
            BSFiberReportData fibCalc_M = fiberMain.FiberCalculate_M(My, prms);
            calcResults_MNQ.Add(fibCalc_M);

            // расчет по наклонной полосе на действие момента [6.1.7]
            string htmlcontent = BSFiberReport_M.RunMultiReport(calcResults_MNQ);
            return htmlcontent;
        }
    }
}
