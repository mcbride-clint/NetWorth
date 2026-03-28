namespace NetWorth.Services;

public record MonteCarloResult(double SuccessRate, double[] P10, double[] P50, double[] P90, int Years, int DeferYears);

public class MonteCarloService
{
    public MonteCarloResult RunSimulation(
        double startNW,
        double annualWithdrawal,
        double meanReturn,
        double stdDev,
        int years,
        int simCount = 1000,
        double inflation = 0.0,
        int deferYears = 0)
    {
        var rng = new Random();
        int totalYears = deferYears + years;
        var yearlyValues = new double[simCount, totalYears];
        int successes = 0;

        for (int s = 0; s < simCount; s++)
        {
            double nw = startNW;
            bool failed = false;

            // Accumulation phase — grow without withdrawals
            for (int y = 0; y < deferYears; y++)
            {
                double ret = SampleGaussian(rng, meanReturn, stdDev);
                nw = nw * (1 + ret);
                yearlyValues[s, y] = Math.Max(0, nw);
            }

            // Withdrawal phase — inflation-adjusted withdrawals
            for (int y = 0; y < years; y++)
            {
                double withdrawal = annualWithdrawal * Math.Pow(1 + inflation, y);
                double ret = SampleGaussian(rng, meanReturn, stdDev);
                nw = nw * (1 + ret) - withdrawal;
                yearlyValues[s, deferYears + y] = failed ? 0 : Math.Max(0, nw);
                if (nw <= 0 && !failed)
                {
                    failed = true;
                    nw = 0;
                }
            }

            if (!failed) successes++;
        }

        double successRate = (double)successes / simCount * 100;

        var p10 = new double[totalYears];
        var p50 = new double[totalYears];
        var p90 = new double[totalYears];

        for (int y = 0; y < totalYears; y++)
        {
            var yearSlice = new double[simCount];
            for (int s = 0; s < simCount; s++)
                yearSlice[s] = yearlyValues[s, y];
            Array.Sort(yearSlice);
            p10[y] = Math.Max(0, yearSlice[(int)(simCount * 0.10)]) / 1_000;
            p50[y] = Math.Max(0, yearSlice[(int)(simCount * 0.50)]) / 1_000;
            p90[y] = Math.Max(0, yearSlice[(int)(simCount * 0.90)]) / 1_000;
        }

        return new MonteCarloResult(successRate, p10, p50, p90, years, deferYears);
    }

    private static double SampleGaussian(Random rng, double mean, double stdDev)
    {
        // Box-Muller transform
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * z;
    }
}
