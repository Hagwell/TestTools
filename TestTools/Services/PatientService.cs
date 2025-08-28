using TestTools.Models;

namespace TestTools.Services
{
    public class PatientService
    {
        public static List<Patient> GeneratePatients(int count)
        {
            var patients = new List<Patient>();
            for (int i = 0; i < count; i++)
            {
                patients.Add(Patient.GenerateRandomPatient());
            }
            return patients;
        }

        public static List<Patient> GenerateMalePatients(int count)
        {
            var patients = new List<Patient>();
            for (int i = 0; i < count; i++)
            {
                patients.Add(MalePatient.GenerateRandomMalePatient());
            }
            return patients;
        }

        public static List<Patient> GenerateFemalePatients(int count)
        {
            var patients = new List<Patient>();
            for (int i = 0; i < count; i++)
            {
                patients.Add(FemalePatient.GenerateRandomFemalePatient());
            }
            return patients;
        }

        public static List<string> GenerateNhsNumbers(int count)
        {
            var nhsNumbers = new List<string>();
            for (int i = 0; i < count; i++)
            {
                nhsNumbers.Add(NhsNumberGenerator.Generate());
            }
            return nhsNumbers;
        }
    }
}
