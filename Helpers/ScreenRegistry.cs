using elbanna.Data;
using elbanna.Models;
using Microsoft.EntityFrameworkCore;

namespace elbanna.Helpers
{
    /// <summary>
    /// Ensures that a screen exists in st_screen so it can be managed from Users -> Permissions.
    /// Some legacy pages in the project were missing permissions completely.
    /// This helper creates the missing st_screen row (if needed) and returns its id.
    /// </summary>
    public static class ScreenRegistry
    {
        /// <summary>
        /// Get the st_screen id for the given name. If it does not exist, it will be created.
        /// </summary>
        public static int EnsureScreen(AppDbContext db, string screenName)
        {
            if (string.IsNullOrWhiteSpace(screenName))
                throw new ArgumentException("screenName is required", nameof(screenName));

            // st_screen.screen is the display name shown in Users -> Permissions.
            var s = db.st_screen.AsNoTracking().FirstOrDefault(x => x.screen == screenName);
            if (s != null)
                return s.id;

            var row = new st_screen { screen = screenName };
            db.st_screen.Add(row);
            db.SaveChanges();

            return row.id;
        }
    }
}
