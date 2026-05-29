# Generate improved dataset with emphasis on CreditScore importance
# Focus: Low credit score = HIGH RISK

$ci = [System.Globalization.CultureInfo]::InvariantCulture
$outputFile = "loan_data_credit_emphasis.csv"

# Header
"Age,Income,CreditScore,LoanAmount,LoanDuration,DebtToIncome,Repaid" | Out-File -FilePath $outputFile -Encoding utf8

# Function to add a row
function Add-LoanData($age, $income, $credit, $amount, $duration, $repaid) {
    $dti = if ($income -gt 0) { $amount / $income } else { 0 }
    "$age,$($income.ToString('F0', $ci)),$credit,$($amount.ToString('F0', $ci)),$duration,$($dti.ToString('F2', $ci)),$repaid" | Out-File -FilePath $outputFile -Append -Encoding utf8
}

# ==============================================================================
# HIGH RISK CASES - Very Low Credit Score (< 400) - ALWAYS REJECT
# ==============================================================================
# Low credit score should be the strongest rejection signal
for ($i = 0; $i -lt 200; $i++) {
    $age = Get-Random -Minimum 18 -Maximum 65
    $income = Get-Random -Minimum 20000 -Maximum 150000
    $credit = Get-Random -Minimum 300 -Maximum 399  # Very low credit
    $amount = Get-Random -Minimum 50000 -Maximum 500000
    $duration = Get-Random -Minimum 12 -Maximum 84
    Add-LoanData $age $income $credit $amount $duration 0  # REJECT
}

# ==============================================================================
# HIGH RISK CASES - Low Credit Score (400-499) - MOSTLY REJECT
# ==============================================================================
for ($i = 0; $i -lt 150; $i++) {
    $age = Get-Random -Minimum 18 -Maximum 65
    $income = Get-Random -Minimum 20000 -Maximum 150000
    $credit = Get-Random -Minimum 400 -Maximum 499  # Low credit
    $amount = Get-Random -Minimum 50000 -Maximum 500000
    $duration = Get-Random -Minimum 12 -Maximum 84
    # 90% reject rate for low credit
    $repaid = if ((Get-Random -Minimum 0 -Maximum 100) -lt 90) { 0 } else { 1 }
    Add-LoanData $age $income $credit $amount $duration $repaid
}

# ==============================================================================
# HIGH RISK CASES - High Loan + Low Credit (< 550) - REJECT
# ==============================================================================
for ($i = 0; $i -lt 100; $i++) {
    $age = Get-Random -Minimum 18 -Maximum 65
    $income = Get-Random -Minimum 30000 -Maximum 200000
    $credit = Get-Random -Minimum 300 -Maximum 549  # Low to medium-low credit
    $amount = Get-Random -Minimum 200000 -Maximum 600000  # High loan
    $duration = Get-Random -Minimum 24 -Maximum 84
    Add-LoanData $age $income $credit $amount $duration 0  # REJECT
}

# ==============================================================================
# HIGH RISK CASES - Low Income + Low Credit - REJECT
# ==============================================================================
for ($i = 0; $i -lt 100; $i++) {
    $age = Get-Random -Minimum 18 -Maximum 65
    $income = Get-Random -Minimum 15000 -Maximum 35000  # Low income
    $credit = Get-Random -Minimum 300 -Maximum 500  # Low credit
    $amount = Get-Random -Minimum 30000 -Maximum 200000
    $duration = Get-Random -Minimum 12 -Maximum 60
    Add-LoanData $age $income $credit $amount $duration 0  # REJECT
}

# ==============================================================================
# MEDIUM RISK CASES - Medium Credit Score (500-649) - MIXED
# ==============================================================================
for ($i = 0; $i -lt 150; $i++) {
    $age = Get-Random -Minimum 18 -Maximum 65
    $income = Get-Random -Minimum 30000 -Maximum 150000
    $credit = Get-Random -Minimum 500 -Maximum 649  # Medium credit
    $amount = Get-Random -Minimum 20000 -Maximum 300000
    $duration = Get-Random -Minimum 12 -Maximum 60
    # 60% reject rate for medium credit
    $repaid = if ((Get-Random -Minimum 0 -Maximum 100) -lt 60) { 0 } else { 1 }
    Add-LoanData $age $income $credit $amount $duration $repaid
}

# ==============================================================================
# LOW RISK CASES - Good Credit Score (650-749) - MOSTLY APPROVE
# ==============================================================================
for ($i = 0; $i -lt 150; $i++) {
    $age = Get-Random -Minimum 18 -Maximum 65
    $income = Get-Random -Minimum 40000 -Maximum 200000
    $credit = Get-Random -Minimum 650 -Maximum 749  # Good credit
    $amount = Get-Random -Minimum 10000 -Maximum 250000
    $duration = Get-Random -Minimum 12 -Maximum 60
    # 80% approve rate for good credit
    $repaid = if ((Get-Random -Minimum 0 -Maximum 100) -lt 80) { 1 } else { 0 }
    Add-LoanData $age $income $credit $amount $duration $repaid
}

# ==============================================================================
# LOW RISK CASES - Excellent Credit Score (750-900) - ALMOST ALWAYS APPROVE
# ==============================================================================
for ($i = 0; $i -lt 150; $i++) {
    $age = Get-Random -Minimum 18 -Maximum 65
    $income = Get-Random -Minimum 50000 -Maximum 250000
    $credit = Get-Random -Minimum 750 -Maximum 900  # Excellent credit
    $amount = Get-Random -Minimum 10000 -Maximum 300000
    $duration = Get-Random -Minimum 12 -Maximum 84
    # 95% approve rate for excellent credit
    $repaid = if ((Get-Random -Minimum 0 -Maximum 100) -lt 95) { 1 } else { 0 }
    Add-LoanData $age $income $credit $amount $duration $repaid
}

# ==============================================================================
# CRITICAL TEST CASES - Ensure model learns specific patterns
# ==============================================================================
# Case 1: Very low credit (320) with any income = REJECT
Add-LoanData 30 80000 320 400000 36 0
Add-LoanData 45 120000 310 300000 24 0
Add-LoanData 35 60000 330 200000 12 0
Add-LoanData 50 150000 340 500000 48 0
Add-LoanData 25 40000 350 150000 18 0

# Case 2: Excellent credit with reasonable loan = APPROVE
Add-LoanData 40 80000 780 50000 36 1
Add-LoanData 35 60000 800 30000 24 1
Add-LoanData 50 100000 820 80000 48 1
Add-LoanData 30 50000 760 40000 12 1
Add-LoanData 45 120000 850 100000 60 1

# Case 3: Mixed profile (high income + low credit) = MEDIUM RISK
Add-LoanData 40 90000 500 300000 60 0
Add-LoanData 35 100000 520 250000 48 0
Add-LoanData 45 110000 480 200000 36 0
Add-LoanData 50 95000 510 350000 72 0
Add-LoanData 30 85000 490 150000 24 0

Write-Host "Generated improved dataset: $outputFile"
Write-Host "Total records: 1000+"
Write-Host "Emphasis on CreditScore importance for risk assessment"
