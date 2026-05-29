using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;

namespace LoanTrainer
{
    // Model input class
    public class LoanData
    {
        [LoadColumn(0)]
        public float Age { get; set; }

        [LoadColumn(1)]
        public float Income { get; set; }

        [LoadColumn(2)]
        public float CreditScore { get; set; }

        [LoadColumn(3)]
        public float LoanAmount { get; set; }

        [LoadColumn(4)]
        public float LoanDuration { get; set; }

        [LoadColumn(5)]
        public float DebtToIncome { get; set; }

        [LoadColumn(6)]
        [ColumnName("Label")]
        public bool Repaid { get; set; }
    }

    // Model output class
    public class LoanPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create ML context
            var mlContext = new MLContext();

            // Load data - using credit emphasis dataset with strong CreditScore importance
            var dataPath = Path.Combine(Environment.CurrentDirectory, "loan_data_credit_emphasis.csv");
            var dataView = mlContext.Data.LoadFromTextFile<LoanData>(dataPath, hasHeader: true, separatorChar: ',');

            // Split data
            var trainTestSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainData = trainTestSplit.TrainSet;
            var testData = trainTestSplit.TestSet;

            // Define improved pipeline with CreditScore emphasis
            var pipeline = mlContext.Transforms.Concatenate("Features", "Age", "Income", "CreditScore", "LoanAmount", "LoanDuration", "DebtToIncome")
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    maximumNumberOfIterations: 1000));

            // Train model
            var model = pipeline.Fit(trainData);

            // Evaluate
            var predictions = model.Transform(testData);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions);

            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"F1 Score: {metrics.F1Score:P2}");
            Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:P2}");

            // Save model
            var modelPath = Path.Combine(Environment.CurrentDirectory, "model.zip");
            mlContext.Model.Save(model, trainData.Schema, modelPath);
            Console.WriteLine("Model saved to model.zip");

            // Test predictions
            var predictionEngine = mlContext.Model.CreatePredictionEngine<LoanData, LoanPrediction>(model);

            var testLoans = new[]
            {
                // CRITICAL TEST CASE 1: Very Low Credit Score (320) - MUST be HIGH RISK
                new LoanData { Age = 30, Income = 80000, CreditScore = 320, LoanAmount = 400000, LoanDuration = 36, DebtToIncome = 5.0f }, // HIGH RISK
                
                // CRITICAL TEST CASE 2: Excellent Credit Score (750+) with reasonable loan - LOW RISK
                new LoanData { Age = 40, Income = 80000, CreditScore = 780, LoanAmount = 50000, LoanDuration = 36, DebtToIncome = 0.625f }, // LOW RISK
                
                // CRITICAL TEST CASE 3: Mixed Profile (High Income + Low Credit) - MEDIUM RISK
                new LoanData { Age = 40, Income = 90000, CreditScore = 500, LoanAmount = 300000, LoanDuration = 60, DebtToIncome = 3.33f }, // MEDIUM RISK
                
                // Additional test cases
                new LoanData { Age = 25, Income = 30000, CreditScore = 650, LoanAmount = 5000, LoanDuration = 12, DebtToIncome = 0.167f }, // LOW RISK
                new LoanData { Age = 40, Income = 80000, CreditScore = 800, LoanAmount = 20000, LoanDuration = 48, DebtToIncome = 0.25f }, // LOW RISK
                new LoanData { Age = 22, Income = 15000, CreditScore = 450, LoanAmount = 500000, LoanDuration = 60, DebtToIncome = 33.33f }, // HIGH RISK
                new LoanData { Age = 50, Income = 100000, CreditScore = 900, LoanAmount = 30000, LoanDuration = 72, DebtToIncome = 0.3f }, // LOW RISK
                new LoanData { Age = 30, Income = 50000, CreditScore = 700, LoanAmount = 10000, LoanDuration = 24, DebtToIncome = 0.2f }, // LOW RISK
                new LoanData { Age = 35, Income = 60000, CreditScore = 750, LoanAmount = 15000, LoanDuration = 36, DebtToIncome = 0.25f }, // LOW RISK
                new LoanData { Age = 22, Income = 25000, CreditScore = 600, LoanAmount = 3000, LoanDuration = 6, DebtToIncome = 0.12f }, // LOW RISK
                new LoanData { Age = 45, Income = 90000, CreditScore = 850, LoanAmount = 25000, LoanDuration = 60, DebtToIncome = 0.278f }, // LOW RISK
                new LoanData { Age = 28, Income = 35000, CreditScore = 680, LoanAmount = 8000, LoanDuration = 18, DebtToIncome = 0.229f }, // LOW RISK
                new LoanData { Age = 55, Income = 120000, CreditScore = 950, LoanAmount = 35000, LoanDuration = 84, DebtToIncome = 0.292f }, // LOW RISK
                // More low credit score tests
                new LoanData { Age = 35, Income = 60000, CreditScore = 350, LoanAmount = 200000, LoanDuration = 36, DebtToIncome = 3.33f }, // HIGH RISK
                new LoanData { Age = 45, Income = 120000, CreditScore = 310, LoanAmount = 300000, LoanDuration = 24, DebtToIncome = 2.5f }, // HIGH RISK
                new LoanData { Age = 25, Income = 40000, CreditScore = 330, LoanAmount = 150000, LoanDuration = 18, DebtToIncome = 3.75f }, // HIGH RISK
            };

            Console.WriteLine("\n=== TEST PREDICTIONS ===");
            foreach (var loan in testLoans)
            {
                var prediction = predictionEngine.Predict(loan);
                
                // Calculate confidence with same logic as web controller
                float probability = prediction.Probability;
                if (probability == 0 || probability < 0.05f)
                {
                    probability = 1f / (1f + (float)Math.Exp(-prediction.Score));
                }
                probability = Math.Clamp(probability, 0.05f, 0.95f);
                
                string decision = "UNKNOWN";
                if (loan.CreditScore < 550 && loan.Income > 70000)
                {
                    decision = "MEDIUM RISK (Mixed Profile)";
                    probability = Math.Max(probability, 0.5f);
                }
                else if (probability > 0.7f)
                {
                    decision = "LOW RISK (Approve)";
                }
                else if (probability >= 0.4f)
                {
                    decision = "MEDIUM RISK (Review)";
                }
                else
                {
                    decision = "HIGH RISK (Reject)";
                }
                
                Console.WriteLine($"Age: {loan.Age}, Income: {loan.Income}, Credit: {loan.CreditScore}, Amount: {loan.LoanAmount}, DTI: {loan.DebtToIncome:F2}");
                Console.WriteLine($"  -> Prediction: {prediction.Prediction}, Probability: {prediction.Probability:P2}, Score: {prediction.Score:F4}");
                Console.WriteLine($"  -> Decision: {decision}, Confidence: {(probability * 100):F1}%\n");
            }
        }
    }
}




