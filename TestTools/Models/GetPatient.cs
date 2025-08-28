namespace TestTools.Models
{
    public class Patient
    {
        public string GetNhsNumber { get; set; }
        public string GetWpasRef { get; set; } = WpasRef.GetWDSWpas;
        public string GetTitle { get; set; }
        public string GetSurname { get; set; }
        public string GetName { get; set; }
        public string GetDob { get; set; }
        public string GetAddress { get; set; }
        public string Sex { get; set; } // Add Sex property

        public static Patient GenerateRandomPatient()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            bool isMale = random.Next(2) == 0;

            if (isMale)
            {
                var patient = MalePatient.GenerateRandomMalePatient();
                patient.Sex = "M";
                return patient;
            }
            else
            {
                var patient = FemalePatient.GenerateRandomFemalePatient();
                patient.Sex = "F";
                return patient;
            }
        }

        public static string GenerateNhsNumber()
        {
            return Services.NhsNumberGenerator.Generate();
        }
    }

    public class MalePatient : Patient
    {
        public static MalePatient GenerateRandomMalePatient()
        {
            return new MalePatient
            {
                GetNhsNumber = Services.NhsNumberGenerator.Generate(),
                GetWpasRef = WpasRef.GetWDSWpas,
                GetTitle = Services.GetTitle.GetMaleTitle.Generate(),
                GetSurname = Services.GetSurname.Generate(),
                GetName = Services.GetMaleName.GetName.Generate(),
                GetDob = Services.DOBGenerator.DateOfBirth.Generate(),
                GetAddress = Services.RandomAddressGenerator.Generate(),
                Sex = "M" // Set Sex property
            };
        }
    }

    public class FemalePatient : Patient
    {
        public static FemalePatient GenerateRandomFemalePatient()
        {
            return new FemalePatient
            {
                GetNhsNumber = Services.NhsNumberGenerator.Generate(),
                GetWpasRef = WpasRef.GetWDSWpas,
                GetTitle = Services.GetTitle.GetFemaleTitle.Generate(),
                GetSurname = Services.GetSurname.Generate(),
                GetName = Services.GetFemaleNames.GetName.Generate(),
                GetDob = Services.DOBGenerator.DateOfBirth.Generate(),
                GetAddress = Services.RandomAddressGenerator.Generate(),
                Sex = "F" // Set Sex property
            };
        }
    }
}
