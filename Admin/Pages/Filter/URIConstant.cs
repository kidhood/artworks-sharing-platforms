﻿namespace User.Pages.Filter;

public static class URIConstant
{
    public static readonly string[] WhiteListUris = {         
        "/Login",
        "/Logout",
        "/"
    };

    public static readonly string[] ModeratorListUris = {
        "/Moderator/ArtWorksManagement",
        "/Moderator/ReportManagement",
    };

    public static readonly string[] AdminListUris = {
        "/Moderator/ArtWorksManagement"
    };

    public static readonly string HomePage = "/";

    public static readonly string Login = "/Login";
}
