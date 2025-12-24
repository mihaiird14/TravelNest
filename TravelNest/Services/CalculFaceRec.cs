namespace TravelNest.Services
{
    public class CalculFaceRec
    {
        public double CalculDistanta(List<double> a, List<double> b)
        {
            double sum = 0;
            for (int i = 0; i < a.Count; i++)
                sum += Math.Pow(a[i] - b[i], 2);

            return Math.Sqrt(sum);
        }

    }
}
