using System.Collections.Generic;

namespace Alvianda.Software.Service.iCOMD.Data
{
    public static class MockPermissionsData
    {
        public static List<KeyValuePair<string, string>> UserPermissions;

        static MockPermissionsData()
        {
            UserPermissions = new List<KeyValuePair<string, string>>();

            UserPermissions.Add(new KeyValuePair<string, string>("User1", "CanRead"));
            UserPermissions.Add(new KeyValuePair<string, string>("User1", "CanWrite"));
            UserPermissions.Add(new KeyValuePair<string, string>("User2", "CanRead"));
        }
    }
}
