namespace DeKayaServer.BlazorApp.Constants;

public static class EndpointConstants
{
    public const string Dashboard = "/";
    public const string LoginPage = "/login";

    //Auth
    public const string LoginEndpoint = "auth/login";
    public const string ForgotPasswordEndpoint = "auth/forgotpassword";
    public const string ResetPasswordEndpoint = "auth/reset-password";
    public const string CheckForgotPasswordCodeEndpoint = "auth/check-forgot-password-code";

    //Roles
    public const string Roles = "/roles";
    public const string ODataRoles = "/odata/roles";

    //Users
    public const string Users = "/users";
    public const string ODataUsers = "/odata/users";

    //Permissions
    public const string Permissions = "/permissions";
    public const string UpdateRolePermissions = "/roles/update-permissions";

    //Customers
    public const string Customers = "/customers";
    public const string ODataCustomers = "/odata/customers";

    //CoolingRooms
    public const string CoolingRooms = "/coolingrooms";
    public const string ODataCoolingRooms = "/odata/coolingrooms";

    //CoolingRoomMaintenances
    public const string CoolingRoomMaintenances = "/coolingroommaintenances";

    //Reservations
    public const string Reservations = "/reservations";
    public const string ODataReservations = "/odata/reservations";
}
