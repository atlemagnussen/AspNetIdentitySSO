# ASP.NET Core Identity

## The cli
dotnet tool install --global dotnet-ef

## remove
dotnet ef database drop

## create migration

dotnet ef migrations add InitialCreate

## Apply 

dotnet ef database update InitialCreate

# LEARNING

ResponseType OpenIdConnectResponseType.Code requires clientSecret

OpenIdConnectResponseType.IdToken works without any secrets just don't validate issuer
