namespace TestTools.Services
{
    public class NhsNumberGenerator
    {
        public static string Generate()
        {
            string nhsNumber = MakeNHSNumber();
            while (nhsNumber.Length > 10)
            {
                nhsNumber = MakeNHSNumber();
            }
            nhsNumber = $"{nhsNumber.Substring(0, 3)} {nhsNumber.Substring(3, 3)} {nhsNumber.Substring(6)}";
            return nhsNumber;
        }

        public static string ReturnNHSNumberString()
        {
            string nhsNumber = MakeNHSNumber();
            while (nhsNumber.Length > 10)
            {
                nhsNumber = MakeNHSNumber();
            }

            return nhsNumber;
        }

        private static string MakeNHSNumber()
        {
            string firstNumber = ChooseStartNumber();
            string middleNumbers = FillMiddleNumbers();
            string firstTen = firstNumber + middleNumbers;
            string finalNumber = CalculateEndNumber(firstTen);
            string nhsNumber = firstTen + finalNumber;
            return nhsNumber;
        }

        private static string ChooseStartNumber()
        {
            Random number = new Random(Guid.NewGuid().GetHashCode());
            int startNo = number.Next(1, 4); // 1, 2, or 3
            return startNo.ToString();
        }

        private static string FillMiddleNumbers()
        {
            Random number = new Random(Guid.NewGuid().GetHashCode());
            string middleNumbers = "";
            for (int i = 0; i < 8; i++)
            {
                int randNumber = number.Next(0, 10);
                middleNumbers += randNumber.ToString();
            }
            return middleNumbers;
        }

        private static string CalculateEndNumber(string nhsNumber)
        {
            int[] numberList = new int[9];
            for (int i = 0; i <= 8; i++)
            {
                string thisNumber = nhsNumber.Substring(i, 1);
                int number = Int32.Parse(thisNumber);
                numberList[i] = number;
            }

            int moduloDivisor = (numberList[0] * 10) + (numberList[1] * 9)
                + (numberList[2] * 8) + (numberList[3] * 7) + (numberList[4] * 6)
                + (numberList[5] * 5) + (numberList[6] * 4) + (numberList[7] * 3) + (numberList[8] * 2);

            int moduloResult = moduloDivisor % 11;
            int finalNumber = 11 - moduloResult;
            if (finalNumber == 11) finalNumber = 0;
            if (finalNumber == 10) finalNumber = 9; // NHS number rules: 10 is not valid, set to 9

            string finalNumberString = finalNumber.ToString();
            return finalNumberString;
        }
    }
}
