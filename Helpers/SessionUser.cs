namespace elbanna.Helpers
{
    public static class SessionUser
    {
        public static int UserId(HttpContext c) =>
            c.Session.GetInt32("UserId") ?? 0;

        public static string UserName(HttpContext c) =>
            c.Session.GetString("Username") ?? "";

        public static string UserJob(HttpContext c) =>
            c.Session.GetString("UserJob") ?? "";

        public static bool CanReview(HttpContext c) =>
            c.Session.GetInt32("CanReview") == 1;
    }
}
