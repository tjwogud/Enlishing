namespace Enlishing
{
    public static class RandomUtils
    {
        public static T SelectOne<T>(T[] options, double[] rates, Random random = null)
        {
            random ??= Random.Shared;

            double sum = rates.Sum();
            for (int i = 0; i < rates.Length; i++)
                rates[i] /= sum;

            double next = random.NextDouble();
            double d = 0;
            for (int i = 0; i < rates.Length; i++)
            {
                if (d + next < rates[i])
                    return options[i];
                d += rates[i];
            }
            return options.Last();
        }
    }
}
