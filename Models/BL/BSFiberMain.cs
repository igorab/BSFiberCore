using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Mat;
using BSFiberCore.Models.BL.Ndm;
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

        private List<Elements> FiberConcrete;

        private List<BSFiberBeton> Bft3Lst;
        private List<FiberBft> BftnLst;
        private List<Beton> BfnLst;

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
            List<RebarDiameters>   m_RebarDiameters = BSData.LoadRebarDiameters();
            List<Rebar>            m_Rebar = BSData.LoadRebar();
            /*List<Elements>*/     FiberConcrete = BSData.LoadFiberConcreteTable();

            /*List<BSFiberBeton>*/ Bft3Lst =  BSQuery.LoadBSFiberBeton();

            /*List<FiberBft>*/     BftnLst = BSData.LoadFiberBft();

            /*List<Beton>*/        BfnLst = BSData.LoadBetonData(0);           
        }

        private void SelectedFiberBetonValues(string fib_i, string bft3n,  ref double numRfbt3n, ref double numRfbt2n)
        {
            try
            {
                var b_i = Convert.ToString(fib_i);
                BSFiberBeton? beton = Bft3Lst.FirstOrDefault(fbt => fbt.Name == bft3n);
                if (beton == null)
                    return;

                string btName = beton.Name.Replace("i", b_i);

                var getQuery = FiberConcrete.Where(f => f.BT == btName);
                if (getQuery?.Count() > 0)
                {
                    Elements? fib = getQuery?.First();

                    numRfbt3n = BSHelper.MPA2kgsm2(fib?.Rfbt3n);
                    numRfbt2n = BSHelper.MPA2kgsm2(fib?.Rfbt2n);
                }
            }
            catch 
            {
                numRfbt3n = 0;
                numRfbt2n = 0;
            }
        }

        private void BftnSelectedValue(string  bft_n, ref double numRfbt_n)
        {
            try
            {
                FiberBft? bft = BftnLst.FirstOrDefault(bft => bft.ID == bft_n);

                numRfbt_n = BSHelper.MPA2kgsm2(bft?.Rfbtn);
            }
            catch 
            {
                numRfbt_n = 0;
            }
        }

        /// <summary>
        /// выбрать по классу бетона на сжатие
        /// </summary>
        /// <param name="bf_n"></param>
        /// <param name="_betonTypeId">тип: тяжелый / легкий</param>
        /// <param name="_airHumidityId">влажность</param>
        /// <param name="numRfb_n"></param>
        /// <param name="numE_beton"></param>
        /// <param name="B_class"></param>
        private void BfnSelectedValue(string bf_n, int _betonTypeId, int _airHumidityId, ref double numRfb_n, ref double numE_beton, ref double B_class)
        {
            try
            {                                
                Beton? bfn = BfnLst.FirstOrDefault(bfn => bfn.BT == bf_n);

                if (bfn != null)
                {
                    string betonClass = Convert.ToString(bfn.BT);
                    B_class = bfn.B;

                    if (string.IsNullOrEmpty(betonClass)) return;

                    Beton bt = BSQuery.HeavyBetonTableFind(betonClass, _betonTypeId);

                    if (bt.Rbn != 0)
                        numRfb_n = BSHelper.MPA2kgsm2(bt.Rbn);

                    double fi_b_cr = 0;

                    if (_airHumidityId >= 0 && _airHumidityId <= 3 && bt.B >= 10)
                    {
                        int iBClass = (int)Math.Round(bt.B, MidpointRounding.AwayFromZero);

                        fi_b_cr = BSFiberLib.CalcFi_b_cr(_airHumidityId, iBClass);
                    }

                    if (bt.Eb != 0)
                    {
                        double _eb = BSHelper.MPA2kgsm2(bt.Eb * 1000);
                        numE_beton = _eb / (1.0 + fi_b_cr);
                    }
                }
            }
            catch 
            {
                numRfb_n = 0;
                numE_beton = 0;
                B_class = 0;
            }
        }

        /// <summary>
        ///  классы материала
        /// </summary>
        public void SelectMaterialFromList()
        {                                 
            double rfbt3n = 0, rfbt2n = 0;
            double rfbtn  = 0;
            double rfbn = 0;
            double Eb = 0;
            double Bclass = 0;

            SelectedFiberBetonValues(Fiber.BetonIndex, Fiber.Bft3, ref rfbt3n, ref rfbt2n);

            BftnSelectedValue(Fiber.Bft, ref rfbtn);

            BfnSelectedValue(Fiber.Bfb, 0, 1, ref rfbn, ref Eb, ref Bclass);
            
            MatFiber = new BSMatFiber(Fiber.Efb, Fiber.Yft, Fiber.Yb, Fiber.Yb1, Fiber.Yb2, Fiber.Yb3, Fiber.Yb5)
            {
                B = Bclass,
                Rfbt3n = rfbt3n,
                Rfbt2n = rfbt2n,
                Rfbtn  = rfbtn,
                Rfbn   = rfbn
            };
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
                reportData.Messages.AddRange(FibCalcGR2.Msg);
                reportData.CalcResults2Group = FibCalcGR2.Results();

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

        private Dictionary<string, double> DictCalcParams(BeamSection _beamSection)
        {
            Dictionary<string, double> MNQ = new Dictionary<string, double>();

            double numYft=0, numYb = 0, numYb1 = 0, numYb2 = 0, numYb3 = 0, numYb5 = 0;
            double numE_beton = 0, numE_fbt = 0;
            double numRfb_n = 0, numRfbt_n = 0, numRfbt2n = 0, numRfbt3n = 0;

            double numEs = 0;
            // нормативные 
            double numRscn = 0;
            double numRsn = 0;
            // расчетные
            double numRsc = 0;
            double numRs = 0;
            // деформации                
            double numEps_s_ult = 0;

            BSMatFiber mf = new BSMatFiber(numE_beton, numYft, numYb, numYb1, numYb2, numYb3, numYb5);
            mf.Rfbn = numRfb_n;
            mf.Rfbtn = numRfbt_n;
            mf.Rfbt2n = numRfbt2n;
            mf.Rfbt3n = numRfbt3n;

            double lgth, coeflgth;
            (lgth, coeflgth) = (0, 0); // BeamLength();

            Dictionary<string, double> D = new Dictionary<string, double>()
            {
                // enforces
                ["N"]  = MNQ.ContainsKey("N") ? -MNQ["N"] : 0,
                ["My"] = MNQ.ContainsKey("My") ? MNQ["My"] : 0,
                ["Mz"] = MNQ.ContainsKey("Mx") ? MNQ["Mx"] : 0,
                ["Qx"] = MNQ.ContainsKey("Qx") ? MNQ["Qx"] : 0,
                ["Qy"] = MNQ.ContainsKey("Qy") ? MNQ["Qy"] : 0,
                //
                //length
                ["lgth"] = lgth,
                ["coeflgth"] = coeflgth,
                //
                //section size
                ["b"] = 0,
                ["h"] = 0,

                ["bf"] = 0,
                ["hf"] = 0,
                ["bw"] = 0,
                ["hw"] = 0,
                ["b1f"] = 0,
                ["h1f"] = 0,

                ["r1"] = 0,
                ["R2"] = 0,
                //
                //Mesh
                ["ny"] = 0, //_beamSectionMeshSettings.NY,
                ["nz"] = 0, //_beamSectionMeshSettings.NX, // в алгоритме плосткость сечения YOZ

                // beton
                ["Eb0"] = numE_beton, // сжатие
                ["Ebt"] = numE_fbt, // растяжение

                // - нормативные
                ["Rbcn"]  = numRfb_n,
                ["Rbtn"]  = numRfbt_n,
                ["Rbt2n"] = numRfbt2n,
                ["Rbt3n"] = numRfbt3n,
                // - расчетные 
                ["Rbc"]  = mf.Rfb,
                ["Rbt"]  = mf.Rfbt,
                ["Rbt2"] = mf.Rfbt2,
                ["Rbt3"] = mf.Rfbt3,
                // - деформации
                // сжатие
                ["ebc0"]   = 0,
                ["ebc2"]   = 0.0035d,
                ["eb_ult"] = 0.0035d,

                // растяжение
                ["ebt0"] = 0,
                ["ebt1"] = 0,
                ["ebt2"] = 0,
                ["ebt3"] = 0,
                ["ebt_ult"] = 0.015d,
                // арматура steel                
                ["Es0"] = numEs,
                // нормативные 
                ["Rscn"] = numRscn,
                ["Rstn"] = numRsn,
                // расчетные
                ["Rsc"] = numRsc,
                ["Rst"] = numRs,
                // деформации
                ["esc2"] = 0,
                ["est2"] = 0,
                ["es_ult"] = numEps_s_ult,
                // коэффициенты надежности
                ["Yft"] = numYft,
                ["Yb"]  = numYb,
                ["Yb1"] = numYb1,
                ["Yb2"] = numYb2,
                ["Yb3"] = numYb3,
                ["Yb5"] = numYb5
            };

            double[] beam_sizes = BeamSizes();

            double b = 0;
            double h = 0;

            if (_beamSection == BeamSection.Rect)
            {
                b = beam_sizes[0];
                h = beam_sizes[1];
            }
            else if (BSHelper.IsITL(_beamSection))
            {
                D["bf"] = beam_sizes[0];
                D["hf"] = beam_sizes[1];
                D["bw"] = beam_sizes[2];
                D["hw"] = beam_sizes[3];
                D["b1f"] = beam_sizes[4];
                D["h1f"] = beam_sizes[5];

                b = D["bf"];
                h = D["hf"] + D["hw"] + D["h1f"];
            }
            else if (_beamSection == BeamSection.Ring)
            {
                D["r1"] = beam_sizes[0];
                D["R2"] = beam_sizes[1];

                b = 2 * D["R2"];
                h = 2 * D["R2"];
            }

            D["b"] = b;
            D["h"] = h;

            return D;
        }

        private void InitStrengthFactorsFromForm(double[] prms)
        {
            int idx = -1;
            if (prms.Length >= 8)
            {
                prms[++idx] = 0; // Convert.ToDouble(numRfbt3n.Value);
                prms[++idx] = 0; // Convert.ToDouble(numRfb_n.Value);
                prms[++idx] = Fiber.Yft;
                prms[++idx] = Fiber.Yb;
                prms[++idx] = Fiber.Yb1;
                prms[++idx] = Fiber.Yb2;
                prms[++idx] = Fiber.Yb3;
                prms[++idx] = Fiber.Yb5;
                prms[++idx] = 0;
            }
        }

        private Dictionary<string, double> FiberCalculate_QxQy(Dictionary<string, double> _MNQ, double[] _sz)
        {
            double[] prms = new double[9];

            InitStrengthFactorsFromForm(prms);

            BSMatFiber fiber = new BSMatFiber(Fiber.Efb, Fiber.Yft, Fiber.Yb, Fiber.Yb1, Fiber.Yb2, Fiber.Yb3, Fiber.Yb5);
            
            var betonType = BSQuery.BetonTypeFind(0);

            BSFiberCalc_QxQy fiberCalc = new BSFiberCalc_QxQy();
            fiberCalc.MatFiber = MatFiber;
            fiberCalc.UseRebar = true;// UseRebar;
            fiberCalc.Rebar = null;// m_SectionChart.Rebar; // поперечная амрматура из полей в контроле m_SectionChart
            fiberCalc.BetonType = betonType;
            fiberCalc.UnitConverter = _UnitConverter;
            // fiberCalc.SetFiberFromLoadData(fiber);
            fiberCalc.SetSize(_sz);
            fiberCalc.SetParams(prms);
            fiberCalc.SetEfforts(_MNQ);
            fiberCalc.SetN_Out();

            bool calcOk = fiberCalc.Calculate();

            if (calcOk)
                fiberCalc.Msg.Add("Расчет успешно выполнен!");
            else
                fiberCalc.Msg.Add("Расчет по наклонному сечению на действие Q не выполнен!");

            Dictionary<string, double> xR = fiberCalc.Results();

            return xR;
        }


        /// <summary>
        /// Расчет на действие поперечных сил действующих по двум направлениям
        /// </summary>
        private Dictionary<string, double> CalcQxQy(double[] sz)
        {
            //SetFiberMaterialProperties();

            //RecalRandomEccentricity_e0();

            Dictionary<string, double> dMNQ = new Dictionary<string, double>(); // GetEffortsForCalc();

            if (dMNQ["Qx"] == 0 && dMNQ["Qy"] == 0)
            {
                return null;
            }

            Dictionary<string, double> resQxQy = FiberCalculate_QxQy(dMNQ, sz);

            return resQxQy;
        }


        /// <summary>
        /// Расчет по НДМ 
        /// </summary>
        /// <param name="_beamSection">Тип сечения</param>
        /// <returns></returns>
        private BSCalcResultNDM CalcNDM(BeamSection _beamSection)
        {            
            double[] sz = BeamSizes(/*length*/);

            if (_beamSection == BeamSection.Any)
            {
                sz[0] = Fiber.Width;
                sz[1] = Fiber.Length;
            }

            Dictionary<string, double> resQxQy = CalcQxQy(sz);

            // данные с формы:
            Dictionary<string, double> _D = DictCalcParams(_beamSection);

            // расчет на MxMyN по НДМ            
            NDMSetup _setup = NDMSetupValuesFromForm();

            // расчет:
            CalcNDM calcNDM = new CalcNDM(_beamSection) { setup = _setup, Dprm = _D };

            Dictionary<string, double> resGr2 = null;

            if (_beamSection == BeamSection.Any ||
                _beamSection == BeamSection.Ring)
            {
                calcNDM.RunGroup1();

                calcNDM.CalcRes.b = Fiber.Width;
                calcNDM.CalcRes.h = Fiber.Length;

                resGr2 = FiberCalculateGroup2(calcNDM.CalcRes);
            }
            else if (BSHelper.IsRectangled(_beamSection))
            {
                calcNDM.RunGroup1();

                resGr2 = FiberCalculateGroup2(calcNDM.CalcRes);
            }
            else
            {
                calcNDM.Run();
            }

            BSCalcResultNDM calcRes = new BSCalcResultNDM();
            if (calcNDM.CalcRes != null)
                calcRes = calcNDM.CalcRes;
            if (resGr2 != null)
                calcRes.SetRes2Group(resGr2, true, true);
            calcRes.ResQxQy = resQxQy;
            //calcRes.ImageStream = ImageStream;
            //calcRes.Coeffs = Coeffs;
            calcRes.UnitConverter = _UnitConverter;

            return calcRes;
        }

        private Dictionary<string, double>? FiberCalculateGroup2(object calcRes)
        {
            return new Dictionary<string, double>() { };
        }

        private NDMSetup NDMSetupValuesFromForm()
        {
            return new NDMSetup();
        }
    }   
}
