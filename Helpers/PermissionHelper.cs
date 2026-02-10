using elbanna.Models;
using Newtonsoft.Json;

namespace elbanna.Helpers
{
    public static class PermissionHelper
    {
        // =========================
        // صلاحيات الشاشات
        // =========================
        public static bool Can(
            int screenId,
            string action,
            HttpContext context)
        {
            var data = context.Session.GetString("permissions");
            if (string.IsNullOrEmpty(data))
                return false;

            var perms = JsonConvert
                .DeserializeObject<List<st_userPermission>>(data);

            return perms.Any(p =>
                p.screen == screenId &&
                (
                    (action == "Add" && p.canAdd) ||
                    (action == "Edit" && p.canEdit) ||
                    (action == "Delete" && p.canDelete) ||
                    (action == "Print" && p.canPrint)
                )
            );
        }

        // =========================
        // صلاحيات المواقع (أي صلاحية)
        // =========================
        public static bool CanCostCenter(
            int costCenterId,
            HttpContext context)
        {
            var data = context.Session.GetString("cc_permissions");
            if (string.IsNullOrEmpty(data))
                return false;

            var perms = JsonConvert
                .DeserializeObject<List<st_UserCCPermission>>(data);

            return perms.Any(p =>
                p.costcenter == costCenterId &&
                (
                    p.canAdd ||
                    p.canEdit ||
                    p.canDelete ||
                    p.canPrint
                )
            );
        }

        // =========================
        // صلاحية موقع + Action
        // =========================
        public static bool CanCostCenter(
            int costCenterId,
            string action,
            HttpContext context)
        {
            var data = context.Session.GetString("cc_permissions");
            if (string.IsNullOrEmpty(data))
                return false;

            var perms = JsonConvert
                .DeserializeObject<List<st_UserCCPermission>>(data);

            var cc = perms.FirstOrDefault(p => p.costcenter == costCenterId);
            if (cc == null)
                return false;

            return action switch
            {
                "Add" => cc.canAdd,
                "Edit" => cc.canEdit,
                "Delete" => cc.canDelete,
                "Print" => cc.canPrint,
                _ => false
            };
        }
        public static bool CanReview(HttpContext c)
        {
            return
                SessionUser.UserJob(c) == "مراجع" ||
                SessionUser.CanReview(c);
        }


        // =========================
        // فتح شاشة (أي صلاحية)
        // =========================
        public static bool CanOpenScreen(int screenId, HttpContext ctx)
        {
            return
                Can(screenId, "Add", ctx) ||
                Can(screenId, "Edit", ctx) ||
                Can(screenId, "Delete", ctx) ||
                Can(screenId, "Print", ctx);
        }





        // =========================
        // شاشة + موقع + Action
        // =========================
        public static bool Can(
            int screenId,
            string action,
            int costCenterId,
            HttpContext context)
        {
            if (!CanCostCenter(costCenterId, action, context))
                return false;

            return Can(screenId, action, context);
        }

        public static List<int> GetAllowedCostCenters(HttpContext context)
        {
            var data = context.Session.GetString("cc_permissions");
            if (string.IsNullOrEmpty(data))
                return new List<int>();

            var perms = JsonConvert
                .DeserializeObject<List<st_UserCCPermission>>(data);

            return perms
                .Where(p =>
                    p.canAdd || p.canEdit || p.canDelete || p.canPrint
                )
                .Select(p => p.costcenter)
                .Distinct()
                .ToList();
        }

    }
}
