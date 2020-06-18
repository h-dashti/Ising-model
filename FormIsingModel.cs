using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace IsingModel
{
    public partial class FormIsingModel : Form
    {
        #region New Fields

        public Ising ising;
        Bitmap spinsMap;
        int L, mcs;
        double Tfirst, Tend, dT, T;
        int rectSize;
        Color colorSpinUp = Color.Blue;
        Plot plotCurveC, plotCurveX, plotCurveE_Ave, plotCurveM_Ave;

        Rectangle rectPlot;

        Pen penLine = new Pen(Color.Blue);
        Pen penAxes = new Pen(Color.FromArgb(192, 192, 255));
        Pen penRects = new Pen(Color.Red);
        double maxC, TC;
        public bool canShowPic;
        public Thread threadLoading;
        Thread threadSimulate;
        int sleepForDrawing;
        //Thread thread2;
       
        #endregion
        


        #region New Methods

        

        public void DoAftherInitalizeSpin()
        {
            ShowData();
            L = ising.L;
            if (L <= 16) rectSize = 16;
            else if (L <= 32) rectSize = 12;
            else if (L <= 64) rectSize = 8;
            else rectSize = 1;
            pictureBoxSpinSite.Size = new Size(L * rectSize, L * rectSize);

            groupBoxCondition.Left = pictureBoxSpinSite.Right + 20;
            groupBoxPic.Left = groupBoxCondition.Left;
            groupBoxPic.Top = groupBoxCondition.Bottom + 10;
            labeldata.Left = groupBoxCondition.Left;
            labeldata.Top = groupBoxPic.Bottom + 10;
            pictureBox_PlotC.Left = groupBoxCondition.Right + 40;
            pictureBox_PlotX.Left = pictureBox_PlotC.Right + 25;
            pictureBox_PlotEAve.Left = pictureBox_PlotC.Left;
            pictureBox_PlotMAve.Left = pictureBox_PlotX.Left;

            rectPlot = new Rectangle(50, 30, pictureBox_PlotC.Width - 60, pictureBox_PlotC.Height - 40);
            plotCurveC = new Plot(rectPlot);
            plotCurveX = new Plot(rectPlot);
            plotCurveE_Ave = new Plot(rectPlot);
            plotCurveM_Ave = new Plot(rectPlot);


            spinsMap = new Bitmap(pictureBoxSpinSite.Width, pictureBoxSpinSite.Height);
            if (checkBox_ShowSpin.Checked)
                RedrawSpinsBitmap();

            maxC = double.MinValue;
            TC = 0;

            GC.Collect();
        }
        void Reset()
        {

            canShowPic = false;
            buttonStart.Text = "Start";
            buttonStart.Enabled = true;
            if (threadSimulate != null && threadSimulate.IsAlive )
            {
                if (threadSimulate.ThreadState == ThreadState.Suspended)
                    threadSimulate.Resume();
                threadSimulate.Abort();
                threadSimulate = null;
            }
            
            try
            {
                L = int.Parse(textBox_L.Text);
                mcs = int.Parse(textBox_MCS.Text);
                Tfirst = double.Parse(textBox_Tfirst.Text);
                Tend = double.Parse(textBox_Tend.Text);
                dT = double.Parse(textBox_dT.Text);
                if (Tfirst > Tend) dT = -dT;
                T = Tfirst;
   
            }
            catch(FormatException fe)
            {
                MessageBox.Show(fe.ToString());
                return;
                
            }


            Loading formLoading = new Loading(this);
            if (threadLoading != null && threadLoading.IsAlive)
            {
                threadLoading.Abort();
                threadLoading = null;
            }
            threadLoading = new Thread(new ThreadStart(DoInitialzeIsing));
            threadLoading.Start();
            formLoading.ShowDialog();
            
            
        }

        void DoInitialzeIsing()
        {
            ising = new Ising(L, mcs);
            ising.InitializeSimulation(T, true);
        }

        void RedrawSpinsBitmap()
        {
            if (rectSize > 1)
            {
                Graphics g = Graphics.FromImage(spinsMap);
                g.Clear(Color.Black);
                for (int i = 0; i < L; i++)
                    for (int j = 0; j < L; j++)
                        if (ising.spin[i, j] == 1)
                            g.FillRectangle(new SolidBrush(colorSpinUp), new Rectangle(i * rectSize, j * rectSize, rectSize, rectSize));
                g.Dispose();
            }
            else
            {
                spinsMap = new Bitmap(L, L);
                for (int i = 0; i < L; i++)
                    for (int j = 0; j < L; j++)
                        if (ising.spin[i, j] == 1)
                            spinsMap.SetPixel(i, j, colorSpinUp);
                //GC.Collect();
            }
        }


        void ShowData()
        {
            if (!canShowPic)
                return;
            int steps = ising.n_countingE;
            double E_ave = (ising.data[0] * mcs) / steps;    // per spin
            double E2_ave = (ising.data[1] * mcs) / steps;
            double M_ave = (ising.data[2] * mcs) / steps;
            double M2_ave = (ising.data[3] * mcs) / steps;
            double M_abs = (ising.data[4] * mcs) / steps;

            string st = "steps =  " + steps;
            st += "\r\n" + "  T   = " + T.ToString("f3");
            st += "\r\n" + " <E>  = " + E_ave.ToString("f3");
            st += "\r\n" + " <E2> = " + E2_ave.ToString("f3");
            st += "\r\n" + " <M>  = " + M_ave.ToString("f3");
            st += "\r\n" + " <M2> = " + M2_ave.ToString("f3");
            st += "\r\n" + "<|M|> = " + M_abs.ToString("f3");
            //st += "\r\n\r\n" + "  C  = " + ((E2_ave - E_ave * E_ave) / (T * T)).ToString("f2");
            //st += "\r\n" + "  X  = " + ((M2_ave - M_ave * M_ave) / T).ToString("f2");

            labeldata.Text = st;
            
            
        }

        void ResetTextBoxes()
        {
            textBox_L.Text = "32";
            textBox_MCS.Text = "10000";
            textBox_Tfirst.Text = "3";
            textBox_Tend.Text = "1.5";
            textBox_dT.Text = "0.1";
           
        }

       


        void UpdatePlotClasses()
        {   
            plotCurveE_Ave.AddPoint(T, ising.E_ave);
            plotCurveM_Ave.AddPoint(T, ising.M_ave);
            plotCurveC.AddPoint(T, ising.C);
            plotCurveX.AddPoint(T, ising.X);

            InvalidatePlotPictureBoxes();
            
        }

        void InvalidatePlotPictureBoxes()
        {
            pictureBox_PlotC.Invalidate();
            pictureBox_PlotX.Invalidate();
            pictureBox_PlotEAve.Invalidate();
            pictureBox_PlotMAve.Invalidate();
        }

        void FindT_C()
        {
        }
        #endregion
//-----------------------------------------------------------------------------------------------------

        public FormIsingModel()
        {
            InitializeComponent();
            ResizeRedraw = true;

            ResetTextBoxes();
            
            Reset();
        }

       

        
        private void buttonReset_Click(object sender, EventArgs e)
        {
            Reset();
            if (checkBox_ShowSpin.Checked)
                pictureBoxSpinSite.Invalidate();

            InvalidatePlotPictureBoxes();
        }


        #region PictureBoxes Plot
        private void pictureBoxSpinSite_Paint(object sender, PaintEventArgs e)
        {
            if (!canShowPic)
                return;
            if (!checkBox_ShowSpin.Checked)
                return;

            Graphics g = e.Graphics;
            g.DrawImage(spinsMap, 0, 0);

        }

        private void pictureBox_PlotC_Paint(object sender, PaintEventArgs e)
        {
            if (!canShowPic)
                return;
            Graphics g = e.Graphics;
            g.TranslateTransform(0, pictureBox_PlotC.Height);
            g.ScaleTransform(1, -1);
            plotCurveC.DrawAxes(g, penAxes, "T", "C", 7);
            plotCurveC.DrawLines(g, penLine, penRects);


        }

        private void pictureBox_PlotX_Paint(object sender, PaintEventArgs e)
        {
            if (!canShowPic)
                return;
            Graphics g = e.Graphics;
            g.TranslateTransform(0, pictureBox_PlotX.Height);
            g.ScaleTransform(1, -1);
            plotCurveX.DrawAxes(g, penAxes, "T", "X", 7);
            plotCurveX.DrawLines(g, penLine, penRects);
        }

        private void pictureBox_PlotEAve_Paint(object sender, PaintEventArgs e)
        {
            if (!canShowPic)
                return;
            Graphics g = e.Graphics;
            g.TranslateTransform(0, pictureBox_PlotEAve.Height);
            g.ScaleTransform(1, -1);
            plotCurveE_Ave.DrawAxes(g, penAxes, "T", "<E>", 7);
            plotCurveE_Ave.DrawLines(g, penLine, penRects);
        }

        private void pictureBox_PlotMAve_Paint(object sender, PaintEventArgs e)
        {
            if (!canShowPic)
                return;
            Graphics g = e.Graphics;
            g.TranslateTransform(0, pictureBox_PlotMAve.Height);
            g.ScaleTransform(1, -1);
            plotCurveM_Ave.DrawAxes(g, penAxes, "T", "<M>", 7);
            plotCurveM_Ave.DrawLines(g, penLine, penRects);
        }

        #endregion

       
        private void buttonStart_Click(object sender, EventArgs e)
        {
            string text = buttonStart.Text;

            if (text == "Start")
            {
                threadSimulate = new Thread(new ThreadStart(DoIsingSimulation));
                threadSimulate.Start();
                buttonStart.Text = "Pause";
            }
            else if (text == "Pause")
            {
                threadSimulate.Suspend();
                buttonStart.Text = "Continue";
            }
            else if (text == "Continue")
            {
                threadSimulate.Resume();
                buttonStart.Text = "Pause";
            }
            
        }



        void DoIsingSimulation()
        {
            
            for (; (dT > 0 && T <= Tend) || (dT < 0 && T >= Tend); T += dT)
            {
               
                    ising.InitializeSimulation(T, false);
                int counterE = 0;
                    while (counterE < mcs)
                    {
                        int value = 100, timer = 10;
                        this.Invoke((ThreadStart)delegate
                            {
                                value = (int)numericUpDownIt.Value;
                                timer = CalculateTimerSleep();
                            });
                        if (timer > 0)
                            Thread.Sleep(timer);
                        for (int i = 0; i < value && counterE < mcs; i++)
                        {
                            ising.DoOneMonteCarloStep();
                            counterE++;
                        }
                        this.BeginInvoke((ThreadStart)delegate
                           {
                               DoThreadEachStep();
                           });
                                             
                    }//
                    ising.CalculateAverageParameters(); //  evaluate C , X for Current T
                    if (ising.C > maxC)
                    {
                        maxC = ising.C;
                        TC = T;
                    }
                    this.BeginInvoke((ThreadStart)delegate
                             {
                                 UpdatePlotClasses();  // plot new points(C, X, ...) in picture boxes
                             });

               

            }// End for T
            //canShowData = true;

            buttonStart.Enabled = false;
            MessageBox.Show("Reached to end of temperature !" + "\r\n" + "The TC = " + TC + " (J/K)");
                  


            
            

        }

        void DoThreadEachStep()
        {
            ShowData();
            if (checkBox_ShowSpin.Checked)
            {
                RedrawSpinsBitmap();
                pictureBoxSpinSite.Invalidate();
            }
          
          
        }


        int CalculateTimerSleep()
        {
            int maxInterval = 400, minInterval = 1,
                min = trackBar_Latency.Minimum, max = trackBar_Latency.Maximum, value = trackBar_Latency.Value;

            if (value == max) return 1;
            else if (value == max - 1) return 10;
            else
                return (minInterval - maxInterval) * (value - min) / (max - min) + maxInterval;
        }

        private void trackBarLatency_ValueChanged(object sender, EventArgs e)
        {
            sleepForDrawing = CalculateTimerSleep();
        }

        private void FormIsingModel_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadSimulate != null && threadSimulate.IsAlive)
            {
                if (threadSimulate.ThreadState == ThreadState.Suspended)
                    threadSimulate.Resume();
                threadSimulate.Abort();
            }
          
          
        }

        private void linkLabelAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string text = "Designed by Hor Dashti (h.dashti2@gmail.com)";
            MessageBox.Show(text, "About");
        }

        

      

       
    }
}