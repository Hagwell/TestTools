namespace TestTools.Models
{
    public static class WpasRef
    {
        public static string GetWDSWpas
        {
            get
            {
                Random rnd = new Random();
                string Wpas = rnd.Next(0, 9999999).ToString("D7");
                const string letter = "T";
                return string.Concat(letter, Wpas);
            }
        }

        public static string GetHddWpas
        {
            get
            {
                Random rnd = new Random();
                string Wpas = rnd.Next(0, 9999999).ToString("D7");
                const string letter = "H";
                return string.Concat(letter, Wpas);
            }
        }

        public static string GetCttWpas
        {
            get
            {
                Random rnd = new Random();
                string Wpas = rnd.Next(0, 9999999).ToString("D7");
                const string letter = "M";
                return string.Concat(letter, Wpas);
            }
        }

        public static string GetSbuWpas
        {
            get
            {
                Random rnd = new Random();
                string Wpas = rnd.Next(0, 9999999).ToString("D7");
                const string letter = "N";
                return string.Concat(letter, Wpas);
            }
        }
    }
}