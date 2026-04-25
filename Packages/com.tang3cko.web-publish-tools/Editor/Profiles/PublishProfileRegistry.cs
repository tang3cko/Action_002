using System.Collections.Generic;
using System.Linq;

namespace Tang3cko.WebPublishTools.Editor.Profiles
{
    public static class PublishProfileRegistry
    {
        private static readonly IReadOnlyList<IPublishProfile> profiles = new IPublishProfile[]
        {
            new UnityroomProfile(),
        };

        public static IReadOnlyList<IPublishProfile> All => profiles;

        public static IPublishProfile GetById(string id)
        {
            return profiles.FirstOrDefault(p => p.Id == id) ?? profiles[0];
        }
    }
}
