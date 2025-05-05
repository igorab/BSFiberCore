using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Mat;
using BSFiberCore.Models.BL.Rep;
using BSFiberCore.Models.BL.Uom;
using System.Data;


namespace BSFiberCore.Models.BL
{
    public class BSFiberMain
    {
        private BSFiberCalculation BSFibCalc;
        private LameUnitConverter _UnitConverter;
        private double[] sz;

        public bool UseReinforcement { get; set; } = false;
        public BeamSection BeamSection { get; set; }
        public BSMatFiber MatFiber { get; set; }
        public Fiber Fiber { get; internal set; }
        public List<string> m_Message { get; private set; }
        public Dictionary<string, double> m_CalcResults2Group { get; private set; }

        public BSFiberMain()
        {
            _UnitConverter = new LameUnitConverter();
        }

        /// <summary>
        ///  размеры балки (поперечное сечение + длина)
        /// </summary>
        /// <param name="_length">Длина балки </param>
        /// <returns>массив размеров </returns>
        private double[] BeamSizes(double _length = 0)
        {
            double[] sz = new double[2];
            double b = Fiber.Width, h = Fiber.Length;
            double bf = Fiber.bf, hf = Fiber.hf, bw = Fiber.bw, hw = Fiber.hw, b1f = Fiber.b1f, h1f = Fiber.h1f;
            double r1 = Fiber.R1, r2 = Fiber.R2;

            if (BeamSection == BeamSection.Any)
            {
                return sz;
            }
            else if (BeamSection == BeamSection.Rect)
            {
                sz = new double[] { b, h, _length };
            }
            else if (BeamSection == BeamSection.TBeam)
            {
                sz = new double[] { bf, hf, bw, hw, b1f, h1f, _length };
            }
            else if (BeamSection == BeamSection.LBeam)
            {
                sz = new double[] { bf, hf, bw, hw, b1f, h1f, _length };
            }
            else if (BeamSection == BeamSection.IBeam)
            {
                sz = new double[] { bf, hf, bw, hw, b1f, h1f, _length };
            }
            else if (BeamSection == BeamSection.Ring)
            {
                sz = new double[] { r2, r1, _length };
            }

            return sz;
        }

        /// <summary>
        ///  Размеры балки
        /// </summary>        
        private double[] BeamWidtHeight(out double _w, out double _h, out double _area)
        {
            double[] sz = BeamSizes();

            if (BeamSection == BeamSection.Rect)
            {
                _w = sz[0];
                _h = sz[1];
                _area = _w * _h;
            }
            else if (BeamSection == BeamSection.Ring)
            {
                _w = Math.Max(sz[0], sz[1]);
                _h = Math.Max(sz[0], sz[1]);
                _area = Math.PI * Math.Pow(Math.Abs(sz[1] - sz[0]), 2) / 4.0;
            }
            else if (BSHelper.IsITL(BeamSection))
            {
                //_w = Math.Max(sz[0], sz[4]);
                _w = sz[2];
                _h = sz[1] + sz[3] + sz[5];
                _area = sz[0] * sz[1] + sz[2] * sz[3] + sz[4] * sz[5];
            }
            else if (BeamSection == BeamSection.Any)
            {
                _w = 0;
                _h = 0;
                _area = 0;
            }
            else
            {
                throw new Exception("Не определен тип сечения");
            }

            return sz;
        }

        public BSFiberCalc_MNQ FibCalc_MNQ(Dictionary<string, double> _MNQ, double[] _prms)
        {
            BSFiberCalc_MNQ fibCalc = BSFiberCalc_MNQ.Construct(BeamSection);

            fibCalc.MatFiber = MatFiber;
            fibCalc.UseRebar = UseReinforcement;
            fibCalc.Rebar         = new Rebar() { };
            fibCalc.BetonType     = new BetonType() { };
            fibCalc.UnitConverter = _UnitConverter;
            fibCalc.SetFiberFromLoadData(new FiberBeton() { });
            fibCalc.SetSize(sz);
            fibCalc.SetParams(_prms);
            fibCalc.SetEfforts(_MNQ);
            fibCalc.SetN_Out();

            return fibCalc;
        }

        /// <summary>
        /// Расчеты стельфибробетона по предельным состояниям второй группы
        /// 1) Расчет предельного момента образования трещин
        /// 2) Расчет ширины раскрытия трещины
        /// </summary>        
        public BSFiberCalc_Cracking FiberCalculate_Cracking(Dictionary<string, double> MNQ)
        {
            bool calcOk;
            try
            {
                BSBeam bsBeam = BSBeam.construct(BeamSection);

                bsBeam.SetSizes(BeamSizes());
                
                BSFiberCalc_Cracking calc_Cracking = new BSFiberCalc_Cracking(MNQ)
                {
                    Beam = bsBeam,
                    typeOfBeamSection = BeamSection
                };

                double selectedDiameter = 10;
                double Es = 0;
                // задать тип арматуры
                calc_Cracking.MatRebar = new BSMatRod(Es)
                {
                    RCls = "",
                    Rs = 0,
                    e_s0 = 0,
                    e_s2 = 0,
                    As = 0,
                    As1 = 0,
                    a_s = 0,
                    a_s1 = 0,
                    Reinforcement = true,
                    SelectedRebarDiameter = selectedDiameter
                };

                // SetFiberMaterialProperties();

                calc_Cracking.MatFiber = MatFiber;

                calcOk = calc_Cracking.Calculate();

                if (m_Message == null) m_Message = new List<string>();
                m_Message.AddRange(calc_Cracking.Msg);

                if (calcOk)
                    m_CalcResults2Group = calc_Cracking.Results();

                return calc_Cracking;
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка в расчете: " + _e.Message);
            }

            return null;
        }

        public void InitSize()
        {
            sz = BeamWidtHeight(out double w, out double h, out double area);
        }

        public void InitMaterials()
        {
            var m_RebarDiameters = BSData.LoadRebarDiameters();
            var m_Rebar = BSData.LoadRebar();
            var FiberConcrete = BSData.LoadFiberConcreteTable();
        }

        /// <summary>
        /// Расчет прочности сечения на действие момента
        /// </summary>        
        public BSFiberReportData FiberCalculate_M(double _M, double[] _prms)
        {
            bool calcOk;
            BSFiberReportData reportData = new BSFiberReportData();

            try
            {                
                BSFibCalc = BSFiberCalculation.Construct(BeamSection, UseReinforcement);
                BSFibCalc.MatFiber = MatFiber;
                InitRebar(BSFibCalc);

                BSFibCalc.SetParams(_prms);
                BSFibCalc.SetSize(sz);
                BSFibCalc.Efforts = new Dictionary<string, double> { { "My", _M } };

                calcOk = BSFibCalc.Calculate();
                if (calcOk)
                    reportData.InitFromBSFiberCalculation(BSFibCalc, _UnitConverter);

                // расчет по второй группе предельных состояний
                var FibCalcGR2 = FiberCalculate_Cracking(BSFibCalc.Efforts);
                reportData.m_Messages.AddRange(FibCalcGR2.Msg);
                reportData.m_CalcResults2Group = FibCalcGR2.Results();

                return reportData;
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка в расчете: " + _e.Message);
                return reportData;
            }
        }

        /// <summary>
        ///  Задать армирование
        /// </summary>
        /// <param name="bSFibCalc"></param>
        private void InitRebar(BSFiberCalculation bSFibCalc)
        {
            double[] matRod = { Fiber.Rs, Fiber.Rsc, Fiber.As, Fiber.A1s, Fiber.Es, Fiber.a_cm, Fiber.a1_cm };

            if (bSFibCalc is BSFiberCalc_RectRods)
            {
                BSFiberCalc_RectRods _bsCalcRods = (BSFiberCalc_RectRods)bSFibCalc;

                _bsCalcRods.SetLTRebar(matRod);
            }
            else if (bSFibCalc is BSFiberCalc_IBeamRods)
            {
                BSFiberCalc_IBeamRods _bsCalcRods = (BSFiberCalc_IBeamRods)bSFibCalc;

                //TODO refactoring
                _bsCalcRods.GetLTRebar(matRod);
            }
        }
    }   
}
