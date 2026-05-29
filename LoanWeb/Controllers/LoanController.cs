using Microsoft.AspNetCore.Mvc;
using LoanWeb.Models;
using LoanWeb.Services;
using System;

namespace LoanWeb.Controllers
{
    public class LoanController : Controller
    {
        private readonly LoanService _loanService;

        public LoanController()
        {
            _loanService = new LoanService();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Predict(LoanData loanData)
        {
            // VALIDATION: Age (must be >= 18)
            if (loanData.Age < 18)
            {
                ModelState.AddModelError("Age", "Age must be 18 or above to apply for a loan.");
            }

            // VALIDATION: Income (must be > 0)
            if (loanData.Income <= 0)
            {
                ModelState.AddModelError("Income", "Please enter a valid income greater than 0.");
            }

            // VALIDATION: Credit Score (300-900)
            if (loanData.CreditScore < 300 || loanData.CreditScore > 900)
            {
                ModelState.AddModelError("CreditScore", "Credit score must be between 300 and 900.");
            }

            // VALIDATION: Loan Amount (must be > 0)
            if (loanData.LoanAmount <= 0)
            {
                ModelState.AddModelError("LoanAmount", "Loan amount must be greater than 0.");
            }

            // VALIDATION: Loan Duration (must be > 0)
            if (loanData.LoanDuration <= 0)
            {
                ModelState.AddModelError("LoanDuration", "Loan duration must be at least 1 month.");
            }

            // If validation fails, return to form with error messages
            if (!ModelState.IsValid)
            {
                return View("Index", loanData);
            }

            var prediction = _loanService.Predict(loanData);

            // FIXED: Improved confidence calculation with sigmoid fix
            float probability = prediction.Probability;

            if (probability == 0 || probability < 0.05f)
            {
                // Apply sigmoid fix for near-zero probabilities
                probability = 1f / (1f + (float)Math.Exp(-prediction.Score));
            }

            // Clamp probability to realistic range (5-95%)
            probability = Math.Clamp(probability, 0.05f, 0.95f);

            // ============================================================
            // BUSINESS RULE SAFETY LAYER - Override ML prediction if needed
            // ============================================================

            // RULE 1: Very Low Credit Score (< 400) - FORCE HIGH RISK
            // Credit score is the most critical factor - very low score = automatic reject
            if (loanData.CreditScore < 400)
            {
                probability = 0.1f; // Strong rejection confidence
            }

            // RULE 2: Low Credit Score (400-499) - Mostly HIGH RISK
            // Unless income is very high and loan is very small
            if (loanData.CreditScore < 500 && loanData.LoanAmount > loanData.Income * 0.5f)
            {
                probability = Math.Min(probability, 0.25f); // Force toward rejection
            }

            // RULE 3: Strong financial profile - FORCE LOW RISK
            // If applicant has excellent credit AND sufficient income AND loan is reasonable
            if (loanData.CreditScore > 700 && loanData.Income > 50000 && loanData.LoanAmount < loanData.Income)
            {
                probability = 0.85f; // Strong approval confidence
            }

            // RULE 4: Obviously dangerous profile - FORCE HIGH RISK
            // If applicant has low income BUT wants to borrow huge amount
            if (loanData.Income < 20000 && loanData.LoanAmount > 300000)
            {
                probability = 0.15f; // Strong rejection confidence
            }

            string decision;
            string riskColor;

            // IMPROVED DECISION LOGIC
            if (probability > 0.7f)
            {
                decision = "LOW RISK (Approve)";
                riskColor = "green";
            }
            else if (probability >= 0.4f)
            {
                decision = "MEDIUM RISK (Review)";
                riskColor = "orange";
            }
            else
            {
                decision = "HIGH RISK (Reject)";
                riskColor = "red";
            }

            Console.WriteLine($"Prediction debug: PredictedLabel={prediction.Prediction}, Probability={prediction.Probability}, Score={prediction.Score}, UsedProbability={probability}");

            ViewBag.Decision = decision;
            ViewBag.Confidence = (probability * 100f).ToString("F1") + "%";
            ViewBag.RiskColor = riskColor;

            return View("Index", loanData);
        }
    }
}