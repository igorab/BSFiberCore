using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Mat;
using BSFiberCore.Models.BL.Rep;
using BSFiberCore.Models.BL.Uom;

namespace BSFiberCore.Models.BL
{
    public class BSFiberMain
    {
        private BSFiberCalculation BSFibCalc;
        private LameUnitConverter _UnitConverter;

        public bool UseReinforcement { get; set; } = false; 
        public BeamSection BeamSection { get; set; }
        public BSMatFiber MatFiber { get; set; }

        public BSFiberMain()
        {
            _UnitConverter = new LameUnitConverter();
        }

        /// <summary>
        /// Расчет прочности сечения на действие момента
        /// </summary>        
        public BSFiberReportData FiberCalculate_M(double _M, double[] _prms, double[] _sz)
        {            
            bool calcOk;
            BSFiberReportData reportData = new BSFiberReportData();

            try
            {
                BSFibCalc = BSFiberCalculation.construct(BeamSection, UseReinforcement);
                BSFibCalc.MatFiber = MatFiber;
                InitRebar(BSFibCalc);

                BSFibCalc.SetParams(_prms);
                BSFibCalc.SetSize(_sz);
                BSFibCalc.Efforts = new Dictionary<string, double> { { "My", _M } };

                calcOk = BSFibCalc.Calculate();
                if (calcOk)
                    reportData.InitFromBSFiberCalculation(BSFibCalc, _UnitConverter);

                // расчет по второй группе предельных состояний
                var FibCalcGR2 = FiberCalculate_Cracking();
                //reportData.m_Messages.AddRange(FibCalcGR2.Msg?? "");
                //reportData.m_CalcResults2Group = FibCalcGR2.Results();

                return reportData;
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка в расчете: " + _e.Message);
                return reportData;
            }
        }

        private void InitRebar(BSFiberCalculation bSFibCalc)
        {
            //throw new NotImplementedException();
            return;
        }

        private FiberCalculate_Cracking FiberCalculate_Cracking()
        {
            //throw new NotImplementedException();
            return null;
        }
    }

    internal class FiberCalculate_Cracking
    {
        public List<string> Msg { get; internal set; }

        internal Dictionary<string, double> Results()
        {
            return new Dictionary<string, double>();
        }
    }    
}
