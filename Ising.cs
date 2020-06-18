using System;


namespace IsingModel
{
    public class Ising
    {
        #region Fields
        public int L = 32;
        public  int n_equilib = 10000;
        public int[,] spin;
        public int N;
        public int E, M;
        public double T, C, X, E_ave, M_ave, E2_ave, M2_ave, Mabs_ave;
  
        public double[] data = new double[5];
        public int accept, n_countingE;
        public int mcs = 10000; // montecarlo steps
        public double norm;

        double[] W_ratio = new double[5];
        Random rand = new Random();

        public int counterLoadingInit;
        
        #endregion


        public Ising()
        {

            InitalizeSpins();
        }
        public Ising(int L, int mcs)
        {
            this.L = L;
            this.mcs = mcs;
            //n_equilib = 3 * L * L;
            InitalizeSpins();
        }

        private void InitalizeSpins()
        {
            spin = new int[L, L];
            int i, j;
            for (i = 0; i < L; i++)
                for (j = 0; j < L; j++)
                    spin[i, j] = (int)(2 * (rand.Next(2) - 0.5));

            E = 0; M = 0; accept = 0;
            N = L * L;
            norm = 1.0 / (N * mcs);
            

            int iRight, jUp;
            for (i = 0; i < L; i++)
            {
                iRight = (i == L - 1) ? 0 : i + 1;
                for (j = 0; j < L; j++)
                {
                    jUp = (j == L - 1) ? 0 : j + 1;
                    E += -spin[i, j] * (spin[iRight, j] + spin[i, jUp]);
                    M += spin[i, j];
                }
            }          
        }


        void MetroPolice()
        {
            int i, j, de;
            bool changedEnergy;

            for (int k = 0; k < N; k++)
            {
                i = rand.Next(L);
                j = rand.Next(L);

                de = DeltaE(i, j);
                changedEnergy = false;
                if (de <= 0)
                    changedEnergy = true;
                else
                    if (rand.NextDouble() <= W_ratio[(de + 8) / 4])
                        changedEnergy = true;

                if (changedEnergy)
                {
                    spin[i, j] = -spin[i, j];
                    E += de;
                    M += 2 * spin[i, j];
                    accept++;
                }
            }
        }


        int DeltaE(int i, int j)
        {
            int iRight = (i == L - 1) ? 0 : i + 1;
            int jUp = (j == L - 1) ? 0 : j + 1;

            int iLeft = (i == 0) ? L - 1 : i - 1;
            int jDown = (j == 0) ? L - 1 : j - 1;

            return 2 * spin[i, j] * (spin[iRight, j] + spin[iLeft, j] + spin[i, jUp] + spin[i, jDown]);

        }

        void InitData()
        {
            accept = 0;
            for (int i = 0; i < data.Length; i++)
                data[i] = 0;
        }

        void CalculateData()
        {
            n_countingE++;
            data[0] += E * norm; // just for avoiding big number we product norm  :  norm = 1/(N*mcs)
            data[1] += E * (E * norm); // per spin
            data[2] += M * norm;
            data[3] += M * (M * norm);
            data[4] += Math.Abs(M) * norm;
            
        }

       


        public void InitializeSimulation(double currentT, bool isFirstT)
        {
            int i, de;
            T = currentT;
            n_countingE = 0;
            for (de = -8; de <= 8; de += 4)
                W_ratio[(de + 8) / 4] = Math.Exp(-de / T);

            if (isFirstT)
                for (i = 0, counterLoadingInit = 1; i < n_equilib; i++, counterLoadingInit++)
                    MetroPolice();

            InitData();
            CalculateData();
        }


        public void DoOneMonteCarloStep()
        {
            MetroPolice();
            CalculateData();
        }

        public void CalculateAverageParameters()
        {
            E_ave = (data[0] * mcs) /n_countingE;
            E2_ave = (data[1] * mcs) / n_countingE;
            M_ave = (data[2] * mcs) / n_countingE;
            M2_ave = (data[3] * mcs) / n_countingE;         // just for avoiding big number for E and M
            Mabs_ave = (data[4] * mcs) / n_countingE;  
            
            C = (E2_ave - E_ave * E_ave * N) / (T * T);  // per spin
            X = (M2_ave - M_ave * M_ave * N) / T;       // per spin
        }


    }
}
