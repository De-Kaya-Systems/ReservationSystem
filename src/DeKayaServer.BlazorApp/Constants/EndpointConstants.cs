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
    public const string ODataRoles = "odata/roles";
}
