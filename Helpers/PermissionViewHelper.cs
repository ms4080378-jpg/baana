namespace elbanna.Helpers
{
    public static class PermissionViewHelper
    {
        // =========================
        // فتح الشاشة
        // =========================
        public static bool CanOpen(HttpContext c, int screenId)
        {
            return PermissionHelper.CanOpenScreen(screenId, c);
        }


        // =========================
        // أزرار CRUD
        // =========================
        public static bool CanAdd(HttpContext c, int screenId)
            => PermissionHelper.Can(screenId, "Add", c);

        public static bool CanEdit(HttpContext c, int screenId)
            => PermissionHelper.Can(screenId, "Edit", c);

        public static bool CanDelete(HttpContext c, int screenId)
            => PermissionHelper.Can(screenId, "Delete", c);

        public static bool CanPrint(HttpContext c, int screenId)
            => PermissionHelper.Can(screenId, "Print", c);

        // =========================
        // مراجعة (مطابقة للديسك توب)
        // =========================
        public static bool CanReview(HttpContext c)
        {
            // المراجعة / الصرف: مراجع أو فتحي
            return SessionUser.UserJob(c) == "مراجع" || IsFathi(c);
        }

        public static bool IsFathi(HttpContext c)
        {
            var u = (SessionUser.UserName(c) ?? "").Trim();

            return
                u.Equals("fathi", StringComparison.OrdinalIgnoreCase) ||
                u.Equals("fathy", StringComparison.OrdinalIgnoreCase) ||
                u.Equals("fathi_", StringComparison.OrdinalIgnoreCase) ||
                u.Equals("فتحي", StringComparison.OrdinalIgnoreCase) ||
                u.Equals("فتحى", StringComparison.OrdinalIgnoreCase);
        }


    }
}
