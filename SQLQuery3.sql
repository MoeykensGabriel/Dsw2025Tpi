SELECT
    U.UserName,
    R.Name AS RoleName
FROM
    dbo.UsuariosRoles AS UR
JOIN
    dbo.Usuarios AS U ON UR.UserId = U.Id
JOIN
    dbo.Roles AS R ON UR.RoleId = R.Id;