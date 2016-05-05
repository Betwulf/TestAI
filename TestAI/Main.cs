using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;

namespace TestAI
{
#pragma warning disable 1998, 4014
    public partial class Main : Form
    {
        private readonly SynchronizationContext synchronizationContext;

        private AIController ctrlr;
        private readonly string mUniverse = "S&P500";

        public Main()
        {
            synchronizationContext = SynchronizationContext.Current;
            InitializeComponent();
            ctrlr = new AIController();
            dteCallAIEndDate.Value = DateTime.Today;
        }

        

        private void UpdateUIError(Exception ex)
        {
            Console.WriteLine(JsonConvert.SerializeObject(ex));
            synchronizationContext.Post(new SendOrPostCallback(p =>
            {
                UpdateUI("Fault: " + JsonConvert.SerializeObject(p));
            }), ex);
        }

        private void UpdateUI(string output)
        {
            Console.WriteLine("UpdateUI: " + output);
            synchronizationContext.Post(new SendOrPostCallback(p => 
            {
                txtOut.AppendText(p.ToString() + Environment.NewLine);
            }), output);
        }



        private async void btnGetUniverse_Click(object sender, EventArgs e)
        {
            await ctrlr.CreateUniverse(mUniverse, UpdateUI);
        }


        private async void btnMktData_Click(object sender, EventArgs e)
        {
            await ctrlr.UpdateMarketData(mUniverse, UpdateUI);
            UpdateUI("MarketData Call DONE");
        }


        private async void btnGetMorehistory_Click(object sender, EventArgs e)
        {
            await ctrlr.GetMoreHistoricalMarketData(mUniverse, UpdateUI);
        }




        private async void btnCalcData_Click(object sender, EventArgs e)
        {
            await ctrlr.UpdateCalcData(mUniverse, UpdateUI);
        }

        


        private async void btnClear_Click(object sender, EventArgs e)
        {
            await ctrlr.DeleteAllData(UpdateUI);
        }

        private async void btnTraining_Click(object sender, EventArgs e)
        {
            await ctrlr.CalculateTrainingData(mUniverse, UpdateUI);
        }



        private async void btnClearCalcData_Click(object sender, EventArgs e)
        {
            await ctrlr.ClearCalcData(UpdateUI);
        }


        private async void btnCallML_Click(object sender, EventArgs e)
        {
            await ctrlr.UpdateMLData(mUniverse, dteCallAIStartDate.Value, dteCallAIEndDate.Value, UpdateUI);
        }




        private async void btnAIRefresh_Click(object sender, EventArgs e)
        {
            ctrlr.RefreshMLData(dteCallAIStartDate.Value, dteCallAIEndDate.Value, UpdateUI);
        }






        private async void btnSimulate_Click(object sender, EventArgs e)
        {
            await ctrlr.RunSimulation(dteCallAIStartDate.Value, dteCallAIEndDate.Value, UpdateUI);
        }




        private async void btnDaily_Click(object sender, EventArgs e)
        {
            await ctrlr.DailyFullUpdate(mUniverse, dteCallAIStartDate.Value, dteCallAIEndDate.Value, UpdateUI);
        }
    }
}
