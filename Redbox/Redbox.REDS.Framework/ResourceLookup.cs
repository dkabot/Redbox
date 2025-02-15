using System;
using System.Collections.Generic;
using System.Linq;
using Wintellect.PowerCollections;

namespace Redbox.REDS.Framework
{
    internal static class ResourceLookup
    {
        public static IResource GetResource(
            MultiDictionary<string, IResource> resources,
            string name,
            IResourceBundleFilter filter)
        {
            var resource1 = resources[name];
            if (resource1 == null)
                return null;
            if (resource1.Count == 1)
            {
                var resource2 = new List<IResource>(resource1)[0];
                return IsResourceEffective(resource2) ? resource2 : null;
            }

            var scoredResourceList = new List<ScoredResource>();
            foreach (var resource3 in resource1)
            {
                var each = resource3;
                if (IsResourceEffective(each))
                {
                    var num = 0;
                    if (filter != null)
                    {
                        var list = each.Type.GetFilterProperties().Where(f => each[f.Name] != null).ToList();
                        if (list.Count == 0)
                            ++num;
                        else if (list.Count(eachFilter =>
                                     each.PropertyEquals(eachFilter.Name, filter[eachFilter.Name])) == list.Count)
                            num += list.Sum(eachFilter => eachFilter.FilterScore);
                    }

                    scoredResourceList.Add(new ScoredResource
                    {
                        Score = num,
                        Resource = each
                    });
                }
            }

            scoredResourceList.Sort((x, y) => y.Score.CompareTo(x.Score));
            return scoredResourceList.Count <= 0 ? null : scoredResourceList[0].Resource;
        }

        public static IResource GetResource2(
            MultiDictionary<string, IResource> resources,
            string name,
            IResourceBundleFilter filter)
        {
            var resource1 = resources[name];
            if (resource1 == null)
                return null;
            if (resource1.Count == 1)
            {
                var resource2 = new List<IResource>(resource1)[0];
                return IsResourceEffective(resource2) ? resource2 : null;
            }

            var scoredResourceList = new List<ScoredResource>();
            foreach (var resource3 in resource1)
            {
                var each = resource3;
                if (IsResourceEffective(each))
                {
                    var num = 0;
                    if (filter != null)
                    {
                        var list = each.Type.GetFilterProperties().Where(f => each[f.Name] != null).ToList();
                        if (list.Count == 0)
                            ++num;
                        else if (list.Count(eachFilter => each.PropertyEquals2(eachFilter.Name,
                                     eachFilter.Name.Substring(0, 2) == "!="
                                         ? filter[eachFilter.Name.Substring(2)]
                                         : (object)filter[eachFilter.Name])) == list.Count)
                            num += list.Sum(eachFilter => eachFilter.FilterScore);
                    }

                    scoredResourceList.Add(new ScoredResource
                    {
                        Score = num,
                        Resource = each
                    });
                }
            }

            scoredResourceList.Sort((x, y) => y.Score.CompareTo(x.Score));
            return scoredResourceList.Count <= 0 ? null : scoredResourceList[0].Resource;
        }

        private static bool IsResourceEffective(IResource resource)
        {
            var now = DateTime.Now;
            var nullable1 = new DateTime?();
            if (resource["effective_date"] != null)
                nullable1 = (DateTime?)resource["effective_date"];
            var nullable2 = new DateTime?();
            if (resource["expiry_date"] != null)
                nullable2 = (DateTime?)resource["expiry_date"];
            DateTime? nullable3;
            if (nullable1.HasValue)
            {
                var dateTime = now;
                nullable3 = nullable1;
                if ((nullable3.HasValue ? dateTime < nullable3.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                    return false;
            }

            if (nullable2.HasValue)
            {
                var dateTime = now;
                nullable3 = nullable2;
                if ((nullable3.HasValue ? dateTime > nullable3.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                    return false;
            }

            if (resource["daypart_dayofweek_only"] != null &&
                !resource.PropertyEquals2("daypart_dayofweek_only", now.DayOfWeek.ToString()))
                return false;
            var nullable4 = new DayOfWeek?();
            if (resource["daypart_effective_weekday"] != null)
                nullable4 = (DayOfWeek?)resource["daypart_effective_weekday"];
            var nullable5 = new DayOfWeek?();
            if (resource["daypart_expiry_weekday"] != null)
                nullable5 = (DayOfWeek?)resource["daypart_expiry_weekday"];
            DayOfWeek? nullable6;
            if (nullable4.HasValue)
            {
                var dayOfWeek = (int)now.DayOfWeek;
                nullable6 = nullable4;
                var valueOrDefault = (int)nullable6.GetValueOrDefault();
                if ((dayOfWeek < valueOrDefault) & nullable6.HasValue)
                    return false;
            }

            if (nullable5.HasValue)
            {
                var dayOfWeek = (int)now.DayOfWeek;
                nullable6 = nullable5;
                var valueOrDefault = (int)nullable6.GetValueOrDefault();
                if ((dayOfWeek > valueOrDefault) & nullable6.HasValue)
                    return false;
            }

            var nullable7 = new TimeSpan?();
            if (resource["daypart_effective_time"] != null)
                nullable7 = (TimeSpan?)resource["daypart_effective_time"];
            var nullable8 = new TimeSpan?();
            if (resource["daypart_expiry_time"] != null)
                nullable8 = (TimeSpan?)resource["daypart_expiry_time"];
            TimeSpan? nullable9;
            if (nullable7.HasValue)
            {
                var timeOfDay = now.TimeOfDay;
                nullable9 = nullable7;
                if ((nullable9.HasValue ? timeOfDay < nullable9.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                    return false;
            }

            if (nullable8.HasValue)
            {
                var timeOfDay = now.TimeOfDay;
                nullable9 = nullable8;
                if ((nullable9.HasValue ? timeOfDay > nullable9.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                    return false;
            }

            return true;
        }

        internal sealed class ScoredResource
        {
            public int Score { get; set; }

            public IResource Resource { get; set; }
        }
    }
}