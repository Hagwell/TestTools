namespace TestTools.Services
{
    public class GetMaleName
    {
        public static class GetName
        {
            private static readonly List<string> Names = new List<string>
            {
                "Liam", "Noah", "Oliver", "Elijah", "James", "William", "Benjamin", "Lucas", "Henry", "Alexander",
                "Mason", "Michael", "Ethan", "Daniel", "Jacob", "Logan", "Jackson", "Levi", "Sebastian", "Mateo",
                "Jack", "Owen", "Theodore", "Aiden", "Samuel", "Joseph", "John", "David", "Wyatt", "Matthew",
                "Luke", "Asher", "Carter", "Julian", "Grayson", "Leo", "Jayden", "Gabriel", "Isaac", "Lincoln",
                "Anthony", "Hudson", "Dylan", "Ezra", "Thomas", "Charles", "Christopher", "Jaxon", "Maverick", "Josiah"
            };

            public static string Generate()
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                int index = random.Next(Names.Count);
                return Names[index];
            }
        }
    }
}
