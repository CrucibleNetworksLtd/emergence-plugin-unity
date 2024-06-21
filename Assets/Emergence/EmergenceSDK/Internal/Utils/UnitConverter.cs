using System.Collections.Generic;

namespace EmergenceSDK.Internal.Utils
{
    public static class UnitConverter
    {
        public const int SIGNIFICANT_DIGITS = 3;

        public enum EtherUnitType
        {
            WEI = 0,
            KWEI = 1,
            MWEI = 2,
            GWEI = 3,
            TWEI = 4,
            PWEI = 5,
            ETHER = 6,
            KETHER = 7,
            METHER = 8,
            GETHER = 9,
            TETHER = 10
        }

        public static string Convert(string source, EtherUnitType sourceUnit, EtherUnitType targetUnit, string comaSeparator)
        {
            string result;

            if (sourceUnit == targetUnit)
            {
                return source;
            }

            string[] splitted = source.Split(new string[] { comaSeparator }, System.StringSplitOptions.None);
            if (splitted.Length == 2)
            {
                string beforeComa = splitted[0];
                string afterComa = splitted[1];

                string t = Convert(source, beforeComa, afterComa, sourceUnit, targetUnit, comaSeparator);

                string u = RemoveTrailingDecimals(t, comaSeparator);

                return u;
            }

            int resultLength = GetPowFactorDifference(sourceUnit, targetUnit);

            if (resultLength > 0)
            {
                result = source;

                for (int i = 0; i < resultLength; i++)
                {
                    result += "0";
                }

                return result;
            }

            if (source.Length > -resultLength)
            {
                result = source;
                result = result.Insert(source.Length + resultLength, comaSeparator);
                return RemoveTrailingDecimals(result, comaSeparator);
            }

            result = "0";
            result += comaSeparator;

            for (int i = (resultLength + source.Length); i < 0; i++)
            {
                result += "0";
            }

            result += source.TrimEnd('0');

            return RemoveTrailingDecimals(result, comaSeparator);
        }

        public static int GetPowFactorDifference(EtherUnitType sourceUnit, EtherUnitType targetUnit)
        {
            return GetPowFactor(sourceUnit) - GetPowFactor(targetUnit);
        }

        private static Dictionary<EtherUnitType, int> powFactor = new Dictionary<EtherUnitType, int>()
        {
            { EtherUnitType.WEI, 0 },
            { EtherUnitType.KWEI, 3 },
            { EtherUnitType.MWEI, 6 },
            { EtherUnitType.GWEI, 9 },
            { EtherUnitType.TWEI, 12 },
            { EtherUnitType.PWEI, 15 },
            { EtherUnitType.ETHER, 18 },
            { EtherUnitType.KETHER, 21 },
            { EtherUnitType.METHER, 24 },
            { EtherUnitType.GETHER, 27 },
            { EtherUnitType.TETHER, 30 },
        };

        public static int GetPowFactor(EtherUnitType unit)
        {
            return powFactor[unit];
        }

        private static string Convert(string source, string beforeComa, string afterComa, EtherUnitType sourceUnit, EtherUnitType targetUnit, string comaSeparator)
        {
            int resultLength = GetPowFactorDifference(sourceUnit, targetUnit);
            string result = beforeComa;

            if (resultLength > 0)
            {
                return ConvertToSmallerPow(source, beforeComa, afterComa, sourceUnit, targetUnit, comaSeparator, resultLength, result);
            }
            else
            {
                return ConvertToBiggerPow(source, beforeComa, afterComa, sourceUnit, targetUnit, comaSeparator, resultLength, result);
            }
        }

        private static string ConvertToSmallerPow(string source, string beforeComa, string afterComa, EtherUnitType sourceUnit, EtherUnitType targetUnit, string comaSeparator, int resultLength, string result)
        {
            if (afterComa.Length >= resultLength)
            {
                result += afterComa;

                if (result.Length > resultLength + beforeComa.Length)
                {
                    result = result.Insert(resultLength + beforeComa.Length, comaSeparator);
                }
                result = result.TrimStart('0');

                return result;
            }
            else
            {
                result += afterComa;

                for (int i = 0; i < resultLength - afterComa.Length; i++)
                {
                    result += "0";
                }
                return result.TrimStart('0');
            }
        }

        private static string ConvertToBiggerPow(string source, string beforeComa, string afterComa, EtherUnitType sourceUnit, EtherUnitType targetUnit, string comaSeparator, int resultLength, string result)
        {
            if (beforeComa.Length > -resultLength)
            {
                result += afterComa;

                if (result.Length > resultLength + beforeComa.Length)
                {
                    result = result.Insert(resultLength + beforeComa.Length, comaSeparator);
                }
                return result.TrimStart('0');
            }

            else
            {
                result += afterComa;

                for (int i = resultLength + beforeComa.Length; i < 0; i++)
                {
                    result = result.Insert(0, "0");
                }

                result = result.Insert(0, comaSeparator);
                result = result.Insert(0, "0");

                return result;
            }
        }

        private static string RemoveTrailingDecimals(string source, string comaSeparator)
        {
            if (!source.Contains(comaSeparator))
            {
                return source;
            }

            int trimIndex = source.Length;

            for (int i = source.Length - 1; i >= 0; i--)
            {
                if (source[i] != '0')
                {
                    trimIndex = i;
                    if (source[i] == comaSeparator[0])
                    {
                        trimIndex--;
                    }
                    break;
                }
            }

            return source.Substring(0, trimIndex + 1);
        }

        public static bool ConvertTest()
        {
            bool succeded = true;

            succeded &= Convert("2,3", EtherUnitType.ETHER, EtherUnitType.ETHER, ",") == "2,3";
            succeded &= Convert("0,0055", EtherUnitType.GWEI, EtherUnitType.GWEI, ",") == "0,0055";

            succeded &= Convert("2", EtherUnitType.ETHER, EtherUnitType.GWEI, ",") == "2000000000";
            succeded &= Convert("2", EtherUnitType.ETHER, EtherUnitType.WEI, ",") == "2000000000000000000";

            succeded &= Convert("2", EtherUnitType.GWEI, EtherUnitType.WEI, ",") == "2000000000";
            succeded &= Convert("2", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,000000002";

            succeded &= Convert("200", EtherUnitType.WEI, EtherUnitType.GWEI, ",") == "0,0000002";
            succeded &= Convert("200", EtherUnitType.WEI, EtherUnitType.ETHER, ",") == "0,0000000000000002";

            succeded &= Convert("200", EtherUnitType.GWEI, EtherUnitType.WEI, ",") == "200000000000";
            succeded &= Convert("200", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,0000002";

            succeded &= Convert("2004", EtherUnitType.WEI, EtherUnitType.GWEI, ",") == "0,000002004";
            succeded &= Convert("2004", EtherUnitType.WEI, EtherUnitType.ETHER, ",") == "0,000000000000002004";

            succeded &= Convert("2004", EtherUnitType.GWEI, EtherUnitType.WEI, ",") == "2004000000000";
            succeded &= Convert("2004", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,000002004";

            succeded &= Convert("8,43092", EtherUnitType.ETHER, EtherUnitType.GWEI, ",") == "8430920000";
            succeded &= Convert("8,43092", EtherUnitType.ETHER, EtherUnitType.WEI, ",") == "8430920000000000000";

            succeded &= Convert("532,3286", EtherUnitType.ETHER, EtherUnitType.GWEI, ",") == "532328600000";
            succeded &= Convert("532,3286", EtherUnitType.ETHER, EtherUnitType.WEI, ",") == "532328600000000000000";

            succeded &= Convert("532,3286", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,0000005323286";
            succeded &= Convert("532,3286", EtherUnitType.GWEI, EtherUnitType.WEI, ",") == "532328600000";

            succeded &= Convert("0,843092", EtherUnitType.ETHER, EtherUnitType.GWEI, ",") == "843092000";
            succeded &= Convert("0,843092", EtherUnitType.ETHER, EtherUnitType.WEI, ",") == "843092000000000000";

            succeded &= Convert("0,00843092", EtherUnitType.ETHER, EtherUnitType.GWEI, ",") == "8430920";
            succeded &= Convert("0,00843092", EtherUnitType.ETHER, EtherUnitType.WEI, ",") == "8430920000000000";

            succeded &= Convert("0,0084309200000002", EtherUnitType.ETHER, EtherUnitType.GWEI, ",") == "8430920,0000002";
            succeded &= Convert("0,0084309200000002", EtherUnitType.ETHER, EtherUnitType.WEI, ",") == "8430920000000200";

            succeded &= Convert("0,00843092", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,00000000000843092";
            succeded &= Convert("0,00843092", EtherUnitType.GWEI, EtherUnitType.WEI, ",") == "8430920";

            succeded &= Convert("12,0084309200000002", EtherUnitType.ETHER, EtherUnitType.GWEI, ",") == "12008430920,0000002";
            succeded &= Convert("12,0084309200000002", EtherUnitType.ETHER, EtherUnitType.WEI, ",") == "12008430920000000200";

            succeded &= Convert("12,0084309200000002", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,0000000120084309200000002";
            succeded &= Convert("12,0084309200000002", EtherUnitType.GWEI, EtherUnitType.WEI, ",") == "12008430920,0000002";

            succeded &= Convert("12,0084309200000002", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,0000000120084309200000002";
            succeded &= Convert("12,008430922", EtherUnitType.GWEI, EtherUnitType.WEI, ",") == "12008430922";

            succeded &= Convert("12008430,9200000002", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,0120084309200000002";
            succeded &= Convert("120084309,200000002", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,120084309200000002";
            succeded &= Convert("1200843092,00000002", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "1,20084309200000002";
            succeded &= Convert("12008430920,0000002", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "12,0084309200000002";

            succeded &= Convert("3,00843092", EtherUnitType.GWEI, EtherUnitType.WEI, ",") == "3008430920";
            succeded &= Convert("3,00843092", EtherUnitType.GWEI, EtherUnitType.ETHER, ",") == "0,00000000300843092";

            succeded &= Convert("3,00843092", EtherUnitType.ETHER, EtherUnitType.KWEI, ",") == "3008430920000000";
            succeded &= Convert("3,00843092", EtherUnitType.ETHER, EtherUnitType.TWEI, ",") == "3008430,92";
            succeded &= Convert("3,00843092", EtherUnitType.ETHER, EtherUnitType.METHER, ",") == "0,00000300843092";
            succeded &= Convert("3,00843092", EtherUnitType.ETHER, EtherUnitType.TETHER, ",") == "0,00000000000300843092";

            succeded &= Convert("1000034343434343434344", EtherUnitType.WEI, EtherUnitType.ETHER, ",") == "1000,034343434343434344";
            succeded &= Convert("1000034343434343434344", EtherUnitType.WEI, EtherUnitType.KETHER, ",") == "1,000034343434343434344";
            succeded &= Convert("100003434343434343434422", EtherUnitType.WEI, EtherUnitType.METHER, ",") == "0,100003434343434343434422";

            succeded &= Convert("53232860000000000000000000", EtherUnitType.WEI, EtherUnitType.ETHER, ",") == "53232860";
            succeded &= Convert("0", EtherUnitType.WEI, EtherUnitType.ETHER, ",") == "0";

            return succeded;
        }
    }
}
