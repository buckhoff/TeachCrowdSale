using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Helper
{
    public static class DisplayHelpers
    {
        public static string GetCategoryIcon(string category)
        {
            return category.ToLower() switch
            {
                "blockchain" => "🔗",
                "platform" => "🏗️",
                "mobile" => "📱",
                "web" => "🌐",
                "api" => "⚡",
                "security" => "🔒",
                "testing" => "🧪",
                "documentation" => "📚",
                "deployment" => "🚀",
                _ => "📋"
            };
        }

        public static string GetUpdateTypeIcon(string updateType)
        {
            return updateType.ToLower() switch
            {
                "progress" => "📈",
                "milestone" => "🎯",
                "issue" => "🐛",
                "feature" => "✨",
                "release" => "🚀",
                "documentation" => "📝",
                _ => "ℹ️"
            };
        }

        public static string GetReleaseTypeIcon(string releaseType)
        {
            return releaseType.ToLower() switch
            {
                "major" => "🔥",
                "minor" => "✨",
                "patch" => "🔧",
                "beta" => "🧪",
                "alpha" => "⚠️",
                "hotfix" => "🚑",
                _ => "📦"
            };
        }

        public static string GetDependencyTypeIcon(string dependencyType)
        {
            return dependencyType.ToLower() switch
            {
                "blocking" => "🚫",
                "prerequisite" => "⬅️",
                "related" => "🔗",
                "optional" => "➡️",
                _ => "📎"
            };
        }
    }
}
