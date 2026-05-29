using Microsoft.ML;
using LoanWeb.Models;
using System;
using System.IO;

namespace LoanWeb.Services
{
    public class LoanService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<LoanData, LoanPrediction> _predictionEngine;

        public LoanService()
        {
            _mlContext = new MLContext();
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModels", "model.zip");
            Console.WriteLine($"Loading ML model from: {modelPath}");
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"ML model file not found: {modelPath}");
            }
            var model = _mlContext.Model.Load(modelPath, out var modelInputSchema);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<LoanData, LoanPrediction>(model);
        }

        public LoanPrediction Predict(LoanData loanData)
        {
            return _predictionEngine.Predict(loanData);
        }
    }
}