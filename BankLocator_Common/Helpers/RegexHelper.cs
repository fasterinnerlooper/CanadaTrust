using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BankLocator_Common.Helpers
{
    public static class RegexHelper
    {
        public const string UrlStringPattern = @"pp=(\d{2}\.\d*),(-\d{3}\.\d*);\d*;(\d+)";
        public const string MapIdNumberPattern = @"([0-9])\.";
        public const string BranchNumberPattern = @".*Branch # ([0-9][0-9][0-9][0-9]).*";
        public const string PhoneNumberPattern = @".*(\([0-9][0-9][0-9]\)[ ]?[0-9][0-9][0-9]\-[0-9][0-9][0-9][0-9]).*";
        public const string AddressPattern = @"(.*)";
        public const string Address2Pattern = @"(\w*, \w\w \w\d\w \d\w\d)";
        public const string HoursPattern = @"(Mon.*PM)";
        public const string DistancePattern = @"([0-9]*\.[0-9]* km)";

        public static Regex urlStringRegex = new Regex(RegexHelper.UrlStringPattern, RegexOptions.IgnoreCase);
        public static Regex mapIdNumberRegex = new Regex(RegexHelper.MapIdNumberPattern, RegexOptions.IgnoreCase);
        public static Regex branchNumberRegex = new Regex(RegexHelper.BranchNumberPattern, RegexOptions.IgnoreCase);
        public static Regex phoneNumberRegex = new Regex(RegexHelper.PhoneNumberPattern, RegexOptions.IgnoreCase);
        public static Regex addressRegex = new Regex(RegexHelper.AddressPattern, RegexOptions.IgnoreCase);
        public static Regex address2Regex = new Regex(RegexHelper.Address2Pattern, RegexOptions.IgnoreCase);
        public static Regex hoursRegex = new Regex(RegexHelper.HoursPattern, RegexOptions.IgnoreCase);
        public static Regex distanceRegex = new Regex(RegexHelper.DistancePattern, RegexOptions.IgnoreCase);
    }

}
