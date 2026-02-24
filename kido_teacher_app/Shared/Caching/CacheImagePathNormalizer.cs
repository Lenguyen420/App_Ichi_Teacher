using System;
using System.Collections.Generic;
using System.Linq;
using kido_teacher_app.Model;

namespace kido_teacher_app.Shared.Caching
{
    /// <summary>
    /// Normalize image fields before saving to cache so UI can consistently read them.
    /// This is a light-touch normalizer: it only fills missing fields from other known fields.
    /// </summary>
    public static class CacheImagePathNormalizer
    {
        public static List<ClassDto> NormalizeClassesForCache(IEnumerable<ClassDto>? classes)
        {
            var list = (classes ?? Enumerable.Empty<ClassDto>())
                .Where(x => x != null)
                .ToList();

            foreach (var c in list)
            {
                var best = FirstNonEmpty(c.currentImage, c.avatarImage, c.avatar, c.imageUrl);
                if (string.IsNullOrEmpty(best))
                    continue;

                if (string.IsNullOrEmpty(c.currentImage))
                    c.currentImage = best;
                if (string.IsNullOrEmpty(c.avatarImage))
                    c.avatarImage = best;
                if (string.IsNullOrEmpty(c.avatar))
                    c.avatar = best;
                if (string.IsNullOrEmpty(c.imageUrl))
                    c.imageUrl = best;
            }

            return list;
        }

        public static List<CourseDto> NormalizeCoursesForCache(IEnumerable<CourseDto>? courses)
        {
            var list = (courses ?? Enumerable.Empty<CourseDto>())
                .Where(x => x != null)
                .ToList();

            foreach (var c in list)
            {
                var best = FirstNonEmpty(c.image, c.thumbnailImage);
                if (string.IsNullOrEmpty(best))
                    continue;

                if (string.IsNullOrEmpty(c.image))
                    c.image = best;
                if (string.IsNullOrEmpty(c.thumbnailImage))
                    c.thumbnailImage = best;
            }

            return list;
        }

        public static List<LectureDto> NormalizeLecturesForCache(IEnumerable<LectureDto>? lectures)
        {
            var list = (lectures ?? Enumerable.Empty<LectureDto>())
                .Where(x => x != null)
                .ToList();

            foreach (var l in list)
            {
                if (string.IsNullOrEmpty(l.avatar))
                {
                    // No alternative field today; keep as-is.
                    continue;
                }
            }

            return list;
        }

        private static string? FirstNonEmpty(params string?[] values)
        {
            foreach (var v in values)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    return v;
            }

            return null;
        }
    }
}
