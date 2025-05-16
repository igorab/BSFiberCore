using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Ndm;
using BSFiberCore.Models.BL.Uom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;


namespace BSFiberCore.Models.BL.Rep
{
    public class BSReport
    {
        private BeamSection m_BeamSection;

        private Dictionary<string, double> m_Beam;

        private LameUnitConverter _UnitConverter;

        public BSCalcResultNDM CalcRes { get; set; }

        public static void RunFromCode(BeamSection m_BeamSection, BSCalcResultNDM calcRes)
        {
            BSReport bSReport = new BSReport(m_BeamSection);
            bSReport.CalcRes = calcRes;
            bSReport.CreateReportNDM();
        }

        /// <summary>
        /// Получить отчет
        /// </summary>
        public static void RunReport(BeamSection m_BeamSection, List<BSCalcResultNDM> calcResults)
        {
            string reportName = "Расчет по прочности нормальных сечений на основе нелинейной деформационной модели";
            string path2file = "FiberCalculationMultiReport.htm";
            File.CreateText(path2file).Dispose();

            if (calcResults.Count > 0)
            {
                // Создаем единую часть отчета для всех расчетов
                BSReport bSReport = new BSReport(m_BeamSection);
                bSReport.CalcRes = calcResults[0] ;

                string pathToHtmlFile = bSReport.CreateHeaderMultiReport(path2file, m_BeamSection, reportName);

                for (int i = 0; calcResults.Count > i; i++)
                {
                    using (StreamWriter w = new StreamWriter(path2file, true, Encoding.UTF8))
                    {
                        w.WriteLine($"<H2>Результат расчета по комбинациям загружений: {i + 1}</H2>");
                    }
                    bSReport = new BSReport(m_BeamSection);
                    bSReport.CalcRes = calcResults[i];
                    pathToHtmlFile = bSReport.CreateBodyMultiReport(path2file, m_BeamSection, reportName);
                }
            }
            System.Diagnostics.Process.Start(path2file);
        }


        public BSReport(BeamSection _beamSection)
        {
            m_BeamSection = _beamSection;
            m_Beam = new Dictionary<string, double>();
        }

        private void InitReportSections(ref BSFiberReport report)
        {
            if (CalcRes == null) return;

            report.Beam = CalcRes.Beam;
            report.Coeffs = CalcRes.Coeffs;
            report.Efforts = CalcRes.Efforts;
            report.GeomParams = CalcRes.GeomParams;
            report.PhysParams = CalcRes.PhysParams; //m_PhysParams;
            report.Reinforcement = CalcRes.Reinforcement;
            report.CalcResults1Group = CalcRes.GetResults1Group();
            report.CalcResults2Group = CalcRes.GetResults2Group();
            report.ImageStream = CalcRes.ImageStream;
            report.Messages = CalcRes.Msg;
            report.PictureToHeadReport = CalcRes.PictureForHeaderReport;
            report.PictureToBodyReport = CalcRes.PictureForBodyReport;

            report.CreateRebarTable(CalcRes.RebarDiametersByIndex, CalcRes.Eps_S, CalcRes.Sig_S);

            report._unitConverter = CalcRes.UnitConverter;
        }

        private string CreateReport(int _fileId,
                                    BeamSection _BeamSection,
                                    string _reportName = "",
                                    bool _useReinforcement = false)
        {
            try
            {
                string path = "";
                BSFiberReport report = new BSFiberReport();

                if (_reportName != "")
                    report.ReportName = _reportName;

                report.BeamSection = _BeamSection;
                report.UseReinforcement = _useReinforcement;

                InitReportSections(ref report);

                path = report.CreateReport(_fileId);
                return path;
            }
            catch (Exception _e)
            {
                throw _e;
            }
        }


        /// <summary>
        /// Сформировать Единую часть для всех групп расчетов
        /// </summary>
        /// <param name="pathToFile">Путь к файлу, в который будет осуществляться запись</param>
        /// <param name="_BeamSection"></param>
        /// <param name="_reportName"></param>
        /// <param name="_useReinforcement"></param>
        /// <returns></returns>
        private string CreateHeaderMultiReport(string pathToFile,
                                    BeamSection _BeamSection,
                                    string _reportName = "",
                                    bool _useReinforcement = false)
        {
            try
            {
                string path = "";
                BSFiberReport report = new BSFiberReport();

                if (_reportName != "")
                    report.ReportName = _reportName;

                report.BeamSection = _BeamSection;
                report.UseReinforcement = _useReinforcement;

                InitReportSections(ref report);

                report.HeaderForMultiReport(pathToFile);
                return path;
            }
            catch (Exception _e)
            {
                throw _e;
            }
        }


        /// <summary>
        /// Сформировать Тело отчета
        /// </summary>
        /// <param name="pathToFile">Путь к файлу, в который будет осуществляться запись</param>
        /// <param name="_BeamSection"></param>
        /// <param name="_reportName"></param>
        /// <param name="_useReinforcement"></param>
        /// <returns></returns>
        private string CreateBodyMultiReport(string pathToFile,
                                    BeamSection _BeamSection,
                                    string _reportName = "",
                                    bool _useReinforcement = false)
        {
            try
            {
                string path = "";
                BSFiberReport report = new BSFiberReport();

                if (_reportName != "")
                    report.ReportName = _reportName;

                report.BeamSection = _BeamSection;
                report.UseReinforcement = _useReinforcement;

                InitReportSections(ref report);

                report.BodyForMultiReport(pathToFile);
                return path;
            }
            catch (Exception _e)
            {
                throw _e;
            }
        }

        [DisplayName("Расчет по прочности нормальных сечений на основе нелинейной деформационной модели")]
        public void CreateReportNDM()
        {
            try
            {
                string reportName = "";
                try
                {
                    MethodBase method = MethodBase.GetCurrentMethod();
                    DisplayNameAttribute attr = (DisplayNameAttribute)method.GetCustomAttributes(typeof(DisplayNameAttribute), true)[0];
                    reportName = attr.DisplayName;
                }
                catch
                {
                    MessageBox.Show("Не задан атрибут DisplayName метода");
                }

                string pathToHtmlFile = CreateReport(1, m_BeamSection, reportName);

                System.Diagnostics.Process.Start(pathToHtmlFile);
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка в отчете " + _e.Message);
            }
        }
    }



}
