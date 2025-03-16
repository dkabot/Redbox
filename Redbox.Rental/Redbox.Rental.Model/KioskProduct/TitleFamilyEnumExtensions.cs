using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redbox.Rental.Model.KioskProduct
{
    public static class TitleFamilyEnumExtensions
    {
        public static IEnumerable<TitleType> GetAllTitleTypesOfTitleFamily(TitleFamily val)
        {
            var typesOfTitleFamily = new List<TitleType>();
            foreach (var field in typeof(TitleType).GetFields())
            {
                var titleFamilyAttribute = field.GetCustomAttributes(false).OfType<TitleFamilyAttribute>()
                    .FirstOrDefault<TitleFamilyAttribute>();
                if ((titleFamilyAttribute != null ? titleFamilyAttribute.TitleFamily == val ? 1 : 0 : 0) != 0)
                    typesOfTitleFamily.Add((TitleType)field.GetValue((object)null));
            }

            return (IEnumerable<TitleType>)typesOfTitleFamily;
        }

        public static TitleFamily GetTitleFamily(this Enum val)
        {
            var element =
                ((IEnumerable<MemberInfo>)val.GetType().GetMember(val.ToString())).FirstOrDefault<MemberInfo>();
            return ((object)element != null
                ? element.GetCustomAttribute<TitleFamilyAttribute>(false)?.TitleFamily
                : new TitleFamily?()).GetValueOrDefault();
        }
    }
}