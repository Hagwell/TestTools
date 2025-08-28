namespace TestTools.Services
{
    public static class GetTitle
    {
        public static class GetMaleTitle
        {
            private static readonly List<string> GetMale = new List<string>
            {
                "Mr", "Ms.", "Bshop.", "Can.", "Dr.", "Ftr.", "Lord", "Prof.", "Rev.", "Sir", "Unkn."
            };
            public static string Generate()
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                int index = random.Next(GetMale.Count);
                return GetMale[index];
            }
        }

        public static class GetFemaleTitle
        {
            private static readonly List<string> GetFemale = new List<string>
            {
                "Mrs", "Miss", "Dame", "Can.", "Dr.", "Lady", "Prof.", "Sstr.", "Unkn."
            };
            public static string Generate()
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                int index = random.Next(GetFemale.Count);
                return GetFemale[index];
            }
        }
    }
}
