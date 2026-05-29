using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO;

namespace LoanWeb.Models
{
    // Model input class
    public class LoanData
    {
        public float Age { get; set; }
        public float Income { get; set; }
        public float CreditScore { get; set; }
        public float LoanAmount { get; set; }
        public float LoanDuration { get; set; }

        public float DebtToIncome => Income > 0 ? LoanAmount / Income : 0;
    }

    // Model output class
    public class LoanPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}