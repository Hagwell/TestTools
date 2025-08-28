namespace TestTools.Services
{
    public class DOBGenerator
    {
        public static class DateOfBirth
        {
            public static string Generate()
            {
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                DateTime start = new DateTime(1930, 1, 1);
                int range = (DateTime.Today - start).Days;
                return start.AddDays(rnd.Next(range)).ToString("dd/MM/yyyy");
            }
        }

        public static class AltDob
        {
            public static string Generate()
            {
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                DateTime start = new DateTime(1930, 1, 1);
                int range = (DateTime.Today - start).Days;
                return start.AddDays(rnd.Next(range)).ToString("yyyyMMdd");
            }
        }
    }
}
