using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Mat;

namespace BSFiberCore.Models.BL
{
    public class BSFiberMain
    {
        private BSFiberCalculation BSFibCalc;
        private object _UnitConverter;

        public BeamSection m_BeamSection { get; private set; }
        public BSMatFiber m_MatFiber { get; private set; }

        /// <summary>
        /// Расчет прочности сечения на действие момента
        /// </summary>        
        private BSFiberReportData FiberCalculate_M(double _M, double[] _prms, double[] _sz)
        {
            bool useReinforcement = false; // checkBoxRebar.Checked;
            bool calcOk;
            BSFiberReportData reportData = new BSFiberReportData();

            try
            {
                BSFibCalc = BSFiberCalculation.construct(m_BeamSection, useReinforcement);
                BSFibCalc.MatFiber = m_MatFiber;
                InitRebar(BSFibCalc);

                BSFibCalc.SetParams(_prms);
                BSFibCalc.SetSize(_sz);
                BSFibCalc.Efforts = new Dictionary<string, double> { { "My", _M } };

                calcOk = BSFibCalc.Calculate();
                if (calcOk)
                    reportData.InitFromBSFiberCalculation(BSFibCalc, _UnitConverter);

                // запуск расчет по второй группе предельных состояний
                var FibCalcGR2 = FiberCalculate_Cracking();
                reportData.m_Messages.AddRange(FibCalcGR2.Msg);
                reportData.m_CalcResults2Group = FibCalcGR2.Results();

                return reportData;
            }
            catch (Exception _e)
            {
                // MessageBox.Show("Ошибка в расчете: " + _e.Message);
                return null;
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

    internal class BSFiberReportData
    {
        internal List<string> m_Messages;
        internal Dictionary<string, double> m_CalcResults2Group;

        internal void InitFromBSFiberCalculation(BSFiberCalculation bSFibCalc, object unitConverter)
        {
            //throw new NotImplementedException();
        }
    }
}
